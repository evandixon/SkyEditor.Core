using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using System.Threading.Tasks;
using SkyEditor.Core.UI;
using SkyEditor.IO.FileSystem;

namespace SkyEditor.Core.Tests.UI
{
    [TestClass]
    public class FileViewModelTests
    {
        public const string TestCategory = "UI - FileViewModel";

        public class TestCoreMod : CoreSkyEditorPlugin
        {
            public override string PluginName { get; }

            public override string PluginAuthor { get; }

            public override string Credits { get; }

            public override string GetExtensionDirectory()
            {
                return "/extensions";
            }

            public override IFileSystem GetFileSystem()
            {
                return new MemoryFileSystem();
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CanSave_NewFile()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                var testFile = new TextFile();
                testFile.CreateFile("Test");

                var testFileViewModel = new FileViewModel(testFile, manager);
                Assert.AreEqual(false, testFileViewModel.CanSave(manager));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CanSave_OpenedFile()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                manager.CurrentFileSystem.WriteAllText("/test.txt", "testing");

                var testFile = new TextFile();
                await testFile.OpenFile("/test.txt", manager.CurrentFileSystem);

                var testFileViewModel = new FileViewModel(testFile, manager);
                Assert.AreEqual(true, testFileViewModel.CanSave(manager));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CanSaveAs()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                var testFile = new TextFile();
                testFile.CreateFile("Test");

                var testFileViewModel = new FileViewModel(testFile, manager);
                Assert.AreEqual(true, testFileViewModel.CanSaveAs(manager));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task Save_OpenedFile()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                manager.CurrentFileSystem.WriteAllText("/test.txt", "testing");

                var testFile = new TextFile();
                await testFile.OpenFile("/test.txt", manager.CurrentFileSystem);
                testFile.Contents = "saved";

                var testFileViewModel = new FileViewModel(testFile, manager);
                await testFileViewModel.Save(manager);
                Assert.AreEqual("saved", manager.CurrentFileSystem.ReadAllText("/test.txt"));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task SaveAs()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                var testFile = new TextFile();
                testFile.CreateFile("Test");
                testFile.Contents = "saved";

                var testFileViewModel = new FileViewModel(testFile, manager);
                await testFileViewModel.Save("/test.txt", manager);
                Assert.AreEqual("saved", manager.CurrentFileSystem.ReadAllText("/test.txt"));
            }
        }
    }
}
