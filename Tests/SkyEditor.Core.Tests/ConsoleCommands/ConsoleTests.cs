﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.ConsoleCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;

namespace SkyEditor.Core.Tests.ConsoleCommands
{
    [TestClass]
    public class ConsoleTests
    {

        private const string ConsoleTestsCategory = "Console Tests";
        public class TestConsoleCommand : ConsoleCommand
        {

            protected override void Main(string[] arguments)
            {
                Console.WriteLine(CurrentApplicationViewModel.CurrentPluginManager != null);
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

            protected override void Main(string[] arguments)
            {
                Console.WriteLine(CurrentApplicationViewModel.CurrentPluginManager != null);
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

            public override IIOProvider GetIOProvider()
            {
                return new MemoryIOProvider();
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

            public override IIOProvider GetIOProvider()
            {
                return new MemoryIOProvider();
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

                    var command = new TestConsoleCommand();

                    Assert.AreEqual("True%ntest%narguments%nstandard%nin%n".Replace("%n", Environment.NewLine), ConsoleManager.TestConsoleCommand(command, appViewModel, new string[] { "test", "arguments" }, "standard\nin").Result);
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

                    var command = new TestConsoleCommand2();

                    Assert.AreEqual("True%narguments%ntest%nstandard%nin%n".Replace("%n", Environment.NewLine), ConsoleManager.TestConsoleCommand(command, appViewModel, new string[] { "test", "arguments" }, "standard\nin").Result);
                }
            }
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public void DefaultCommandName()
        {
            var x = new TestConsoleCommand();
            Assert.AreEqual("TestConsoleCommand", x.CommandName);
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public void TestConsoleCommand_ArgumentNullChecks()
        {
            TestHelpers.TestStaticFunctionNullParameters(typeof(ConsoleManager), nameof(ConsoleManager.TestConsoleCommand), "command",
                new Type[] { typeof(ConsoleCommand), typeof(ApplicationViewModel), typeof(string[]), typeof(string) },
                new object[] { null, new ApplicationViewModel(new PluginManager()), new string[] { }, "" });

            TestHelpers.TestStaticFunctionNullParameters(typeof(ConsoleManager), nameof(ConsoleManager.TestConsoleCommand), "appViewModel",
                new Type[] { typeof(ConsoleCommand), typeof(ApplicationViewModel), typeof(string[]), typeof(string) },
                new object[] { new TestConsoleCommand(), null, new string[] { }, "" });
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
                    await appViewModel.CurrentConsoleManager.RunConsole();

                    // Check
                    var output = c.GetStdOut();
                    Assert.AreEqual(Properties.Resources.ConsoleTests_TestRunConsole_Output, c.GetStdOut());
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
                    await appViewModel.CurrentConsoleManager.RunCommand("TestConsoleCommand2", "main arg");

                    // Check
                    var output = c.GetStdOut();
                    Assert.AreEqual(Properties.Resources.ConsoleTests_TestRunCommand_ArgString_Output, c.GetStdOut());
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
                    await appViewModel.CurrentConsoleManager.RunCommand("TestConsoleCommand", new string[] { "main", "arg" });

                    // Check
                    var output = c.GetStdOut();
                    Assert.AreEqual(Properties.Resources.ConsoleTests_TestRunCommand_ArgArr_Output, c.GetStdOut());
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
                    await appViewModel.CurrentConsoleManager.RunCommand("TestConsoleCommandException", "main arg", true);

                    Assert.IsTrue(c.GetStdOut().Contains(nameof(TestException)), "Console output does not contain correct exception.");

                    try
                    {
                        await appViewModel.CurrentConsoleManager.RunCommand("TestConsoleCommandException", "main arg", false);
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
                    await appViewModel.CurrentConsoleManager.RunCommand("TestConsoleCommandException", new string[] { "main", "arg" }, true);

                    Assert.IsTrue(c.GetStdOut().Contains(nameof(TestException)), "Console output does not contain correct exception.");

                    try
                    {
                        await appViewModel.CurrentConsoleManager.RunCommand("TestConsoleCommandException", new string[] { "main", "arg" }, false);
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