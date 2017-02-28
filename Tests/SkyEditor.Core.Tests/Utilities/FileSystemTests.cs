using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.Utilities
{
    [TestClass]
    public class FileSystemTests
    {
        public const string TestCategory = "Utilities - File System";

        [TestMethod]
        [TestCategory(TestCategory)]
        public void MakeRelativePath()
        {
            var target = "/dir1/file1";
            var relative = "/dir1/";
            Assert.AreEqual("file1", FileSystem.MakeRelativePath(target, relative));
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void MakeRelativePath_Spaces()
        {
            var target = "/dir1/file 1";
            var relative = "/dir1/";
            Assert.AreEqual("file 1", FileSystem.MakeRelativePath(target, relative));
        }
    }
}
