using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.TestComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Tests.TestComponents
{
    [TestClass]
    public class MemoryConsoleProviderTests
    {
        private const string ConsoleTestsCategory = "Console Tests";

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public void TestStdIn()
        {
            var provider = new MemoryConsoleProvider();

            provider.StdIn.Append("!Line ");
            provider.StdIn.AppendLine("1");
            provider.StdIn.AppendLine("Line 2");

            Assert.AreEqual(Convert.ToInt32('!'), provider.Read());
            Assert.AreEqual("Line 1", provider.ReadLine());
            Assert.AreEqual("Line 2", provider.ReadLine());
        }

        [TestMethod]
        [TestCategory(ConsoleTestsCategory)]
        public void TestStdOut()
        {
            var provider = new MemoryConsoleProvider();

            provider.Write("Test".ToCharArray());
            provider.Write("ing ");
            provider.Write(this);
            provider.Write(" ");
            provider.Write("asserts {0} {1} ", "All", "Tests");
            provider.Write(true);
            provider.Write("This is .  Garbage string".ToCharArray(), 8, 2);
            provider.WriteLine();
            provider.WriteLine("Test line".ToCharArray());
            provider.WriteLine("Test line 2");
            provider.WriteLine("Test {0}", "formatted line");
            provider.WriteLine(this);
            provider.WriteLine("Testing a specific substring.".ToCharArray(), 8, 20);

            var meString = this.ToString();
            Assert.AreEqual(string.Format("Testing {0} asserts All Tests True. " + Environment.NewLine +
                "Test line" + Environment.NewLine +
                "Test line 2" + Environment.NewLine +
                "Test formatted line" + Environment.NewLine +
                "{0}" + Environment.NewLine +
                "a specific substring" + Environment.NewLine, meString), provider.StdOut.ToString());
        }
    }
}
