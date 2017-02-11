using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Projects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Tests.Projects
{
    [TestClass]
    public class DirectoryStructureComparerTests
    {
        public const string TestCategory = "Project Tests";

        [TestMethod]
        [TestCategory(TestCategory)]
        public void FirstLevelCharacterTest()
        {
            var c = new DirectoryStructureComparer();

            var alphabet = "abcdefghijklmnopqrstuvwxyz1234567890";
            foreach (var c1 in alphabet)
            {
                foreach (var c2 in alphabet)
                {
                    Assert.AreEqual(string.Compare(c1.ToString(), c2.ToString()), c.Compare(c1.ToString(), c2.ToString()), $"Characters \"{c1}\" and \"{c2}\" not properly compared.");
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void CaseInsensitivityTest()
        {
            var c = new DirectoryStructureComparer();

            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var lower = "abcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < upper.Length - 1; i++)
            {
                var c1 = upper[i].ToString();
                var c2 = lower[i].ToString();
                Assert.AreEqual(0, c.Compare(c1, c2), $"Characters \"{c1}\" and \"{c2}\" should be treated the same.");
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void SortTest()
        {
            var testList = new List<string>();
            testList.Add("/a/a/b");
            testList.Add("/a/b/y");
            testList.Add("/a/b/r");
            testList.Add("/a/a/w");
            testList.Add("/a/a/c");
            testList.Add("/a/a/x");
            testList.Add("/a/a/q");
            testList.Add("/a/b/z");
            testList.Add("/b/2");
            testList.Add("/b");
            testList.Add("/a/a/b");
            testList.Add("/a/a/y");
            testList.Add("/a/a/r");
            testList.Add("/a/b/w");
            testList.Add("/a/a/c");
            testList.Add("/a/a/x");
            testList.Add("/a/b/q");
            testList.Add("/a/a/z");
            testList.Sort(new DirectoryStructureComparer());

            Assert.AreEqual("/a/a/b", testList[0]);
            Assert.AreEqual("/a/a/b", testList[1]);
            Assert.AreEqual("/a/a/c", testList[2]);
            Assert.AreEqual("/a/a/c", testList[3]);
            Assert.AreEqual("/a/a/q", testList[4]);
            Assert.AreEqual("/a/a/r", testList[5]);
            Assert.AreEqual("/a/a/w", testList[6]);
            Assert.AreEqual("/a/a/x", testList[7]);
            Assert.AreEqual("/a/a/x", testList[8]);
            Assert.AreEqual("/a/a/y", testList[9]);
            Assert.AreEqual("/a/a/z", testList[10]);
            Assert.AreEqual("/a/b/q", testList[11]);
            Assert.AreEqual("/a/b/r", testList[12]);
            Assert.AreEqual("/a/b/w", testList[13]);
            Assert.AreEqual("/a/b/y", testList[14]);
            Assert.AreEqual("/a/b/z", testList[15]);
            Assert.AreEqual("/b", testList[16]);
            Assert.AreEqual("/b/2", testList[17]);
        }
    }
}
