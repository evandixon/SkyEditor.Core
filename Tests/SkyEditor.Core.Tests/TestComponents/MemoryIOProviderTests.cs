using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.TestComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.TestComponents
{
    [TestClass]
    public class MemoryIOProviderTests
    {

        public const string MemoryIOProviderCategory = "Memory IOProvider Tests";
        public MemoryIOProvider Provider { get; set; }

        [TestInitialize]
        public void Init()
        {
            Provider = new MemoryIOProvider();
        }

        [TestMethod]
        [TestCategory(MemoryIOProviderCategory)]
        public void FileExistsNegativeTest()
        {
            Assert.IsFalse(Provider.FileExists(""), "No files should exist.");
            Assert.IsFalse(Provider.FileExists("/temp/0"), "No files should exist.");
            Assert.IsFalse(Provider.FileExists("/directory"), "No files should exist.");
            Assert.IsFalse(Provider.FileExists("/"), "No files should exist.");
        }

        [TestMethod]
        [TestCategory(MemoryIOProviderCategory)]
        public void DirectoryExistsNegativeTest()
        {
            Assert.IsFalse(Provider.DirectoryExists(""), "No directories should exist.");
            Assert.IsFalse(Provider.DirectoryExists("/temp/0"), "No directories should exist.");
            Assert.IsFalse(Provider.DirectoryExists("/directory"), "No directories should exist.");
            Assert.IsFalse(Provider.DirectoryExists("/"), "No directories should exist.");
        }

        [TestMethod]
        [TestCategory(MemoryIOProviderCategory)]
        public void CreateDirectory()
        {
            Provider.CreateDirectory("/directory");
            Assert.IsTrue(Provider.DirectoryExists("/directory"), "Directory \"/directory\" not created");

            Provider.CreateDirectory("/directory/subDirectory");
            Assert.IsTrue(Provider.DirectoryExists("/directory/subDirectory"), "Directory \"/directory/subDirectory\" not created");
        }

        [TestMethod]
        [TestCategory(MemoryIOProviderCategory)]
        public void CreateDirectoryRecursive()
        {
            Provider.CreateDirectory("/root/directory");
            if (!Provider.DirectoryExists("/root/directory"))
            {
                Assert.Inconclusive("Directory /root/directory not created.");
            }
            Assert.IsTrue(Provider.DirectoryExists("/root"), "Directory \"/root\" not created when \"/root/directory\" was created.");
        }

        [TestMethod]
        [TestCategory(MemoryIOProviderCategory)]
        public void ByteReadWrite()
        {
            byte[] testSequence = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Provider.WriteAllBytes("/testFile.bin", testSequence);

            var read = Provider.ReadAllBytes("/testFile.bin");
            Assert.IsTrue(testSequence.SequenceEqual(read));
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void TextReadWrite()
        {
            string testSequence = "ABCDEFGHIJKLMNOPQRSTUVWXYZqbcdefghijklmnopqrstuvwxyz0123456789àèéêç";
            Provider.WriteAllText("/testFile.bin", testSequence);

            var read = Provider.ReadAllText("/testFile.bin");
            Assert.IsTrue(testSequence.SequenceEqual(read));
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void FileLength()
        {
            byte[] testSequence = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Provider.WriteAllBytes("/testFile.bin", testSequence);

            Assert.AreEqual(Convert.ToInt64(testSequence.Length), Provider.GetFileLength("/testFile.bin"));
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void DeleteDirectory()
        {
            Provider.CreateDirectory("/directory/subDirectory");
            Provider.CreateDirectory("/test/directory");
            if (!Provider.DirectoryExists("/directory") || !Provider.DirectoryExists("/test"))
            {
                Assert.Inconclusive("Couldn't create test directory");
            }

            Provider.DeleteDirectory("/test/directory");
            Assert.IsFalse(Provider.DirectoryExists("/test/directory"), "Directory \"/test/directory\" not deleted.");
            Assert.IsTrue(Provider.DirectoryExists("/test"), "Incorrect directory deleted: \"/test\"");
            Assert.IsTrue(Provider.DirectoryExists("/directory/subDirectory"), "Incorrect directory deleted: \"/directory/subDirectory\"");
            Assert.IsTrue(Provider.DirectoryExists("/directory"), "Incorrect directory deleted: \"/directory\"");
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void DeleteDirectoryRecursive()
        {
            Provider.CreateDirectory("/directory/subDirectory");
            Provider.CreateDirectory("/test/directory");
            if (!Provider.DirectoryExists("/directory") || !Provider.DirectoryExists("/test"))
            {
                Assert.Inconclusive("Couldn't create test directory");
            }

            Provider.DeleteDirectory("/test");
            Assert.IsFalse(Provider.DirectoryExists("/test/directory"), "Directory \"/test/directory\" not deleted recursively.");
            Assert.IsFalse(Provider.DirectoryExists("/test"), "Directory \"/test\" not deleted.");
            Assert.IsTrue(Provider.DirectoryExists("/directory/subDirectory"), "Incorrect directory deleted: \"/directory/subDirectory\"");
            Assert.IsTrue(Provider.DirectoryExists("/directory"), "Incorrect directory deleted: \"/directory\"");
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void DeleteFile()
        {
            byte[] testSequence = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Provider.WriteAllBytes("/testFile.bin", testSequence);

            if (!Provider.FileExists("/testFile.bin"))
            {
                Assert.Inconclusive("Unable to create test file.");
            }

            Provider.DeleteFile("/testFile.bin");

            Assert.IsFalse(Provider.FileExists("/testFile.bin"), "File not deleted.");
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void CopyFile()
        {
            byte[] testSequence = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Provider.WriteAllBytes("/testFile.bin", testSequence);

            if (!Provider.FileExists("/testFile.bin"))
            {
                Assert.Inconclusive("Unable to create test file.");
            }

            Provider.CopyFile("/testFile.bin", "/testFile2.bin");

            Assert.IsTrue(Provider.FileExists("/testFile2.bin"), "File not copied.");
            Assert.AreEqual(testSequence, Provider.ReadAllBytes("/testFile2.bin"), "Copied file has incorrect contents.");
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void GetFiles()
        {
            Provider.WriteAllText("/dir1/file1.txt", "");
            Provider.WriteAllText("/dir1/file2.txt", "");
            Provider.WriteAllText("/dir1/file3.txt", "");
            Provider.WriteAllText("/dir1/file1.bin", "");
            Provider.WriteAllText("/dir1/file2.bin", "");
            Provider.WriteAllText("/dir1/file3.bin", "");
            Provider.WriteAllText("/file1.txt", "");
            Provider.WriteAllText("/file2.txt", "");
            Provider.WriteAllText("/file3.txt", "");
            Provider.WriteAllText("/file1.bin", "");
            Provider.WriteAllText("/file2.bin", "");
            Provider.WriteAllText("/file3.bin", "");
            Provider.WriteAllText("/file.txt", "");

            var allDir1 = Provider.GetFiles("/dir1", "*", false);
            var antiRecursionBin = Provider.GetFiles("/", "*.bin", true);
            var rootFiles = Provider.GetFiles("/", "file?.txt", true);

            Assert.AreEqual(6, allDir1.Length);
            Assert.IsTrue(allDir1.Any(x => x == "/dir1/file1.txt"));
            Assert.IsTrue(allDir1.Any(x => x == "/dir1/file2.txt"));
            Assert.IsTrue(allDir1.Any(x => x == "/dir1/file3.txt"));
            Assert.IsTrue(allDir1.Any(x => x == "/dir1/file1.bin"));
            Assert.IsTrue(allDir1.Any(x => x == "/dir1/file2.bin"));
            Assert.IsTrue(allDir1.Any(x => x == "/dir1/file3.bin"));

            Assert.AreEqual(3, antiRecursionBin.Length);
            Assert.IsTrue(antiRecursionBin.Any(x => x == "/file1.bin"));
            Assert.IsTrue(antiRecursionBin.Any(x => x == "/file2.bin"));
            Assert.IsTrue(antiRecursionBin.Any(x => x == "/file3.bin"));

            Assert.AreEqual(4, rootFiles.Length);
            Assert.IsTrue(rootFiles.Any(x => x == "/file1.txt"));
            Assert.IsTrue(rootFiles.Any(x => x == "/file2.txt"));
            Assert.IsTrue(rootFiles.Any(x => x == "/file3.txt"));
            Assert.IsTrue(rootFiles.Any(x => x == "/file.txt"));
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void GetTempFilename_NotNull()
        {
            var tempFilename = Provider.GetTempFilename();
            Assert.IsNotNull(tempFilename);
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void GetTempFilename_FileExists()
        {
            var tempFilename = Provider.GetTempFilename();
            Assert.IsTrue(Provider.FileExists(tempFilename), "Temp file does not exist");
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void GetTempDirectory_NotNull()
        {
            var tempDirectory = Provider.GetTempDirectory();
            Assert.IsNotNull(tempDirectory);
        }

        [TestMethod()]
        [TestCategory(MemoryIOProviderCategory)]
        public void GetTempDirectory_FileExists()
        {
            var tempDirectory = Provider.GetTempDirectory();
            Assert.IsTrue(Provider.DirectoryExists(tempDirectory), "Temp directory does not exist");
        }
    }
}