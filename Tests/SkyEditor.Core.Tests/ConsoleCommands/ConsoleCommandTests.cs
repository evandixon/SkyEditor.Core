using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.ConsoleCommands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.ConsoleCommands
{
    [TestClass]
    public class ConsoleCommandTestIntegration
    {

        private const string ConsoleTestsCategory = "Console Tests";
        public class TestConsoleCommand : ConsoleCommand
        {

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

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public async Task ConsoleCommandTest()
        {
            PluginManager manager = new PluginManager();
            await manager.LoadCore(new BasicTestCoreMod());

            TestConsoleCommand command = new TestConsoleCommand();

            Assert.AreEqual("True%ntest%narguments%nstandard%nin%n".Replace("%n", Environment.NewLine), ConsoleManager.TestConsoleCommand(command, manager, new string[] { "test", "arguments" }, "standard\nin").Result);
        }
    }

}
