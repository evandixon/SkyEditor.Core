using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Extensions;
using SkyEditor.Core.IO;
using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.Core.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.Projects
{
    [TestClass]
    public class NoteProjectTests
    {
        public const string TestCategory = "Projects - Note Project";

        private class TestCoreMod : CoreSkyEditorPlugin
        {
            public override string PluginName => "";

            public override string PluginAuthor => "";

            public override string Credits => "";
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task BuildIntegrationTest()
        {
            using (var manager = new PluginManager())
            {
                // Set up
                await manager.LoadCore(new TestCoreMod());

                var solution = await ProjectBase.CreateProject<Solution>("projects", "TestSolution", manager);
                await solution.AddNewProject("/", "TestProject", typeof(NoteProject), manager);

                var project = solution.GetProject("/TestProject");
                await project.CreateFile("/", "test.txt", typeof(TextFile));

                var theFile = await project.GetFile("/test.txt", IOHelper.PickFirstDuplicateMatchSelector, manager) as TextFile;
                theFile.Contents = "This is a test note.";
                await theFile.Save(manager.CurrentIOProvider);

                // Build
                await solution.Build();

                // Remove existing notes
                var extBank = ExtensionHelper.GetExtensionBank<NoteExtension>(manager);
                foreach (var item in extBank.GetExtensions(manager))
                {
                    await extBank.UninstallExtension(item.ID, manager);
                }
                var notes = extBank.GetNotes(manager);
                Assert.AreEqual(0, notes.Count);

                // Install extension
                await ExtensionHelper.InstallExtensionZip("projects/TestSolution/TestProject/Output/TestProject.zip", manager);

                // Check new notes
                notes = ExtensionHelper.GetExtensionBank<NoteExtension>(manager).GetNotes(manager);
                Assert.AreEqual(1, notes.Count);

                // Cleanup
                manager.CurrentIOProvider.DeleteDirectory("projects");
                foreach (var item in extBank.GetExtensions(manager))
                {
                    await extBank.UninstallExtension(item.ID, manager);
                }
            }
        }
    }
}
