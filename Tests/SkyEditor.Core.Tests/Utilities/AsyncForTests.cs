using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyEditor.Core.Tests.Utilities
{
    [TestClass]
    public class AsyncForTests
    {
        public const string TestCategory = "AsyncFor Tests";

        [TestMethod]
        [TestCategory(TestCategory)]
        public void RunsOnSet()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5 };

            var newSet = new ConcurrentBag<int>();
            testData.RunAsyncForEach(i => newSet.Add(i)).Wait();

            Assert.IsTrue(newSet.Contains(1));
            Assert.IsTrue(newSet.Contains(2));
            Assert.IsTrue(newSet.Contains(3));
            Assert.IsTrue(newSet.Contains(4));
            Assert.IsTrue(newSet.Contains(5));
        }
    }
}
