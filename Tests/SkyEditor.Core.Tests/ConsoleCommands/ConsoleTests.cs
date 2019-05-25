using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.ConsoleCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.IO.FileSystem;

namespace SkyEditor.Core.Tests.ConsoleCommands
{
    [TestClass]
    public class ConsoleTests
    {

        private const string ConsoleTestsCategory = "Console Tests";
        public class TestConsoleCommand : ConsoleCommand
        {
            public TestConsoleCommand(PluginManager pluginManager)
            {
                CurrentPluginManager = pluginManager;
            }

            protected PluginManager CurrentPluginManager { get; }

            protected override void Main(string[] arguments)
            {
                Console.WriteLine(CurrentPluginManager != null);
                foreach (var item in arguments)
                {
                    Console.WriteLine(item);
                }

                var stdInLine = Console.ReadLine();
                while (!string.IsNullOrEmpty(stdInLine))
                {
                    Console.WriteLine(stdInLine);
                    stdInLine = Console.ReadLine();
                }
            }
        }

        public class TestConsoleCommand2 : ConsoleCommand
        {
            public TestConsoleCommand2(PluginManager pluginManager)
            {
                CurrentPluginManager = pluginManager;
            }

            protected PluginManager CurrentPluginManager { get; }

            protected override void Main(string[] arguments)
            {
                Console.WriteLine(CurrentPluginManager != null);
                foreach (var item in arguments.Reverse())
                {
                    Console.WriteLine(item);
                }

                var stdInLine = Console.ReadLine();
                while (!string.IsNullOrEmpty(stdInLine))
                {
                    Console.WriteLine(stdInLine);
                    stdInLine = Console.ReadLine();
                }
            }
        }

        public class TestException : Exception
        {

        }

        public class TestConsoleCommandException : ConsoleCommand
        {
            public TestConsoleCommandException(IFileSystem provider)
            {
            }

            protected override void Main(string[] arguments)
            {
                throw new TestException();
            }
        }

        public class TestCoreMod1 : CoreSkyEditorPlugin
        {
            public override string PluginName => "";

            public override string PluginAuthor => "";

            public override string Credits => "";

            public override string GetExtensionDirectory()
            {
                return "/extensions";
            }

            public override IFileSystem GetFileSystem()
            {
                return new MemoryFileSystem();
            }

            public override IConsoleProvider GetConsoleProvider()
            {
                return new MemoryConsoleProvider();
            }

            public override bool IsCorePluginAssemblyDynamicTypeLoadEnabled()
            {
                return false;
            }

            public override void Load(PluginManager manager)
            {
                // Do NOT load base.  That would register more console commands than wanted here.
                // base.Load(manager);

                manager.AddSingletonDependency(GetApplicationViewModel(manager));
                manager.AddSingletonDependency(GetFileSystem());
                manager.AddSingletonDependency(GetSettingsProvider(manager));
                manager.AddSingletonDependency(GetConsoleProvider());

                manager.RegisterType<ConsoleCommand, TestConsoleCommand>();
            }
        }

        public class TestCoreMod2 : CoreSkyEditorPlugin
        {
            public override string PluginName => "";

            public override string PluginAuthor => "";

            public override string Credits => "";

            public override string GetExtensionDirectory()
            {
                return "/extensions";
            }

            public override IFileSystem GetFileSystem()
            {
                return new MemoryFileSystem();
            }

            public override IConsoleProvider GetConsoleProvider()
            {
                return new MemoryConsoleProvider();
            }

            public override bool IsCorePluginAssemblyDynamicTypeLoadEnabled()
            {
                return false;
            }

            public override void Load(PluginManager manager)
            {
                base.Load(manager);
                manager.RegisterType<ConsoleCommand, TestConsoleCommand>();
                manager.RegisterType<ConsoleCommand, TestConsoleCommand2>();
                manager.RegisterType<ConsoleCommand, TestConsoleCommandException>();
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestConsoleCommandTest()
        {
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(new BasicTestCoreMod());

                    var command = manager.CreateInstance<TestConsoleCommand>();

                    Assert.AreEqual("True%ntest%narguments%nstandard%nin%n".Replace("%n", Environment.NewLine), ConsoleShell.TestConsoleCommand(command, appViewModel, new string[] { "test", "arguments" }, "standard\nin").Result);
                }
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestConsoleCommand2Test()
        {
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(new BasicTestCoreMod());

                    var command = manager.CreateInstance<TestConsoleCommand2>();

                    Assert.AreEqual("True%narguments%ntest%nstandard%nin%n".Replace("%n", Environment.NewLine), ConsoleShell.TestConsoleCommand(command, appViewModel, new string[] { "test", "arguments" }, "standard\nin").Result);
                }
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public void DefaultCommandName()
        {
            var x = new TestConsoleCommand(null);
            Assert.AreEqual("TestConsoleCommand", x.CommandName);
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public void TestConsoleCommand_ArgumentNullChecks()
        {
            TestHelpers.TestStaticFunctionNullParameters(typeof(ConsoleShell), nameof(ConsoleShell.TestConsoleCommand), "command",
                new Type[] { typeof(ConsoleCommand), typeof(ApplicationViewModel), typeof(string[]), typeof(string) },
                new object[] { null, new ApplicationViewModel(new PluginManager()), new string[] { }, "" });

            TestHelpers.TestStaticFunctionNullParameters(typeof(ConsoleShell), nameof(ConsoleShell.TestConsoleCommand), "appViewModel",
                new Type[] { typeof(ConsoleCommand), typeof(ApplicationViewModel), typeof(string[]), typeof(string) },
                new object[] { new TestConsoleCommand(null), null, new string[] { }, "" });
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestRunConsole()
        {
            var coreMod = new TestCoreMod1();
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(coreMod);

                    // Set up
                    var c = manager.CurrentConsoleProvider as MemoryConsoleProvider;
                    c.StdIn.AppendLine("help");
                    c.StdIn.AppendLine("blarg");
                    c.StdIn.AppendLine("TestConsoleCommand2"); // Should not be registered
                    c.StdIn.AppendLine("TestConsoleCommand"); // Should work properly
                    c.StdIn.AppendLine("exit");

                    // Test
                    await appViewModel.CurrentConsoleShell.RunConsole();

                    // Check
                    var output = c.GetStdOut();
                    Assert.AreEqual(Properties.Resources.ConsoleTests_TestRunConsole_Output.Replace("%n", Environment.NewLine), c.GetStdOut());
                }
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestRunCommand_ArgumentString_Output()
        {
            var coreMod = new TestCoreMod2();
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(coreMod);

                    var c = manager.CurrentConsoleProvider as MemoryConsoleProvider;

                    // Test
                    await appViewModel.CurrentConsoleShell.RunCommand("TestConsoleCommand2", "main arg");

                    // Check
                    var output = c.GetStdOut();
                    Assert.AreEqual(Properties.Resources.ConsoleTests_TestRunCommand_ArgString_Output.Replace("%n", Environment.NewLine), c.GetStdOut());
                }                    
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestRunCommand_ArgumentArray_Output()
        {
            var coreMod = new TestCoreMod2();
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(coreMod);

                    var c = manager.CurrentConsoleProvider as MemoryConsoleProvider;

                    // Test
                    await appViewModel.CurrentConsoleShell.RunCommand("TestConsoleCommand", new string[] { "main", "arg" });

                    // Check
                    var output = c.GetStdOut();
                    Assert.AreEqual(Properties.Resources.ConsoleTests_TestRunCommand_ArgArr_Output.Replace("%n", Environment.NewLine), c.GetStdOut());
                }                    
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestRunCommand_ArgumentString_Exception()
        {
            var coreMod = new TestCoreMod2();
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(coreMod);

                    var c = manager.CurrentConsoleProvider as MemoryConsoleProvider;

                    // Test
                    await appViewModel.CurrentConsoleShell.RunCommand("TestConsoleCommandException", "main arg", true);

                    Assert.IsTrue(c.GetStdOut().Contains(nameof(TestException)), "Console output does not contain correct exception.");

                    try
                    {
                        await appViewModel.CurrentConsoleShell.RunCommand("TestConsoleCommandException", "main arg", false);
                    }
                    catch (TestException)
                    {
                        // Pass
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Incorrect exeption thrown: " + ex.ToString());
                    }
                }                    
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task TestRunCommand_ArgumentArray_Exception()
        {
            var coreMod = new TestCoreMod2();
            using (var manager = new PluginManager())
            {
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    await manager.LoadCore(coreMod);

                    var c = manager.CurrentConsoleProvider as MemoryConsoleProvider;

                    // Test
                    await appViewModel.CurrentConsoleShell.RunCommand("TestConsoleCommandException", new string[] { "main", "arg" }, true);

                    Assert.IsTrue(c.GetStdOut().Contains(nameof(TestException)), "Console output does not contain correct exception.");

                    try
                    {
                        await appViewModel.CurrentConsoleShell.RunCommand("TestConsoleCommandException", new string[] { "main", "arg" }, false);
                    }
                    catch (TestException)
                    {
                        // Pass
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Incorrect exeption thrown: " + ex.ToString());
                    }
                }                    
            }
        }
    }

}
