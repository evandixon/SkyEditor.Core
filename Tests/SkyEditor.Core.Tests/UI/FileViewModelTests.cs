using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using System.Threading.Tasks;
using SkyEditor.Core.UI;

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

            public override IIOProvider GetIOProvider()
            {
                return new MemoryIOProvider();
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

                manager.CurrentIOProvider.WriteAllText("/test.txt", "testing");

                var testFile = new TextFile();
                await testFile.OpenFile("/test.txt", manager.CurrentIOProvider);

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

                manager.CurrentIOProvider.WriteAllText("/test.txt", "testing");

                var testFile = new TextFile();
                await testFile.OpenFile("/test.txt", manager.CurrentIOProvider);
                testFile.Contents = "saved";

                var testFileViewModel = new FileViewModel(testFile, manager);
                await testFileViewModel.Save(manager);
                Assert.AreEqual("saved", manager.CurrentIOProvider.ReadAllText("/test.txt"));
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
                Assert.AreEqual("saved", manager.CurrentIOProvider.ReadAllText("/test.txt"));
            }
        }
    }
}
