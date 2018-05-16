using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core.Projects;

namespace SkyEditor.Core.Tests
{
    [TestClass]
    public class ApplicationViewModelTests
    {
        public const string TestCategory = "UI - Application View Model";

        public class CoreMod : CoreSkyEditorPlugin
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
        public async Task GetIOFilters_SpecificExtensionWithSupportedFileAndAllFile()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                manager.RegisterIOFilter("*.txt", "Text Files");
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var filter = appViewModel.GetIOFilter(new string[] { "*.txt" }, true, true, false);
                    Assert.AreEqual("Supported Files|*.txt|Text Files|*.txt|All Files|*.*", filter);
                }
            }
        }


        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetIOFilters_SpecificExtensionWithSupportedFileAndAllFileAndSolution()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                manager.RegisterIOFilter("*.txt", "Text Files");
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var filter = appViewModel.GetIOFilter(new string[] { "*.txt" }, true, true, true);
                    Assert.AreEqual("Supported Files|*.txt;*.skysln|Text Files|*.txt|Sky Editor Solution|*.skysln|All Files|*.*", filter);
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_SetsSelectedFile()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                manager.CurrentIOProvider.WriteAllText("/test.txt", "testing");
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    Assert.IsNull(appViewModel.SelectedFile, "Selected File should be null before any file is opened");
                    await appViewModel.OpenFile("/test.txt", IOHelper.PickFirstDuplicateMatchSelector);
                    Assert.IsNotNull(appViewModel.SelectedFile, "Selected File should not be null after a file is opened");
                }     
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task TestProjectErrorsDetectedByApplicationViewModel()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var solution = await ProjectBase.CreateProject<TextPreprocessorSolution>("/projects", "Test Solution", manager);
                    await solution.Initialize();
                    await solution.LoadingTask;

                    (solution.GetProject("/Text Preprocessor Project") as TextPreprocessorProject).ReportInfoErrorOnBuild = true;
                    appViewModel.CurrentSolution = solution;
                    await appViewModel.CurrentSolution.Build();

                    Assert.AreEqual(1, appViewModel.Errors.Count);
                    var e = appViewModel.Errors[0];
                    Assert.AreEqual(ErrorType.Info, e.Type);
                    Assert.AreSame(solution.GetProject("/Text Preprocessor Project"), e.SourceProject, "Error did not originate from project.");
                }
            }
        }
    }
}
