using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core.Projects;

namespace SkyEditor.Core.Tests.Projects
{
    [TestClass]
    public class ProjectSolutionIntegration
    {
        public const string TestCategory = "Projects - Project/Solution Integration";

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
        public async Task TextPreprocessorIntegration()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                var provider = manager.CurrentIOProvider;

                // Create the solution
                var solution = await ProjectBase.CreateProject<TextPreprocessorSolution>("/projects", "Test Solution", manager);
                await solution.Save(provider);
                await solution.Initialize();
                await solution.LoadingTask;

                // Verify files were created correctly
                Assert.IsTrue(provider.DirectoryExists("/projects"));
                Assert.IsTrue(provider.DirectoryExists("/projects/Test Solution"));
                Assert.IsTrue(provider.FileExists($"/projects/Test Solution/Test Solution." + Solution.SolutionFileExt));

                // Verify project was created correctly
                var project = solution.GetProject("/Text Preprocessor Project");
                Assert.IsNotNull(project);
                Assert.AreSame(solution, project.ParentSolution);
                Assert.IsInstanceOfType(project, typeof(TextPreprocessorProject));
                Assert.IsTrue(project.FileExists("/variables.txt"));
                Assert.IsTrue(project.DirectoryExists("/files"));
                Assert.IsNotNull(project.Settings, "Settings are null");

                // Set up the project
                await project.CreateFile("/files", "File1.txt", typeof(TextFile));
                await project.CreateFile("/files", "File2.txt", typeof(TextFile));

                Assert.IsTrue(project.FileExists("/files/File1.txt"));
                Assert.IsTrue(project.FileExists("/files/File2.txt"));

                var variablesFile = (await project.GetFile("/variables.txt", IOHelper.PickFirstDuplicateMatchSelector, manager)) as TextFile;
                var file1 = (await project.GetFile("/files/File1.txt", IOHelper.PickFirstDuplicateMatchSelector, manager)) as TextFile;
                var file2 = (await project.GetFile("/files/File2.txt", IOHelper.PickFirstDuplicateMatchSelector, manager)) as TextFile;

                Assert.IsNotNull(file1);
                Assert.IsNotNull(file2);

                variablesFile.Contents = "var1=placeholder\nvar2=placeholder2";
                await variablesFile.Save(provider);

                file1.Contents = "var1 is %var1%";
                await file1.Save(provider);

                file2.Contents = "var2 is %var2%";
                await file2.Save(provider);

                // Build
                await project.Build();

                // Verify correct build
                Assert.IsTrue(provider.DirectoryExists("/projects/Test Solution/Text Preprocessor Project/output"));
                Assert.IsTrue(provider.FileExists("/projects/Test Solution/Text Preprocessor Project/output/File1.txt"));
                Assert.IsTrue(provider.FileExists("/projects/Test Solution/Text Preprocessor Project/output/File2.txt"));
                Assert.AreEqual("var1 is placeholder", provider.ReadAllText("/projects/Test Solution/Text Preprocessor Project/output/File1.txt"));
                Assert.AreEqual("var2 is placeholder2", provider.ReadAllText("/projects/Test Solution/Text Preprocessor Project/output/File2.txt"));
            }
        }
        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ProjectSaveAndReopen()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                var provider = manager.CurrentIOProvider;

                // Create the solution
                var solution = await ProjectBase.CreateProject<TextPreprocessorSolution>("/projects", "Test Solution", manager);
                await solution.Initialize();
                await solution.LoadingTask;

                // Set up the project
                var project = solution.GetProject("/Text Preprocessor Project");
                await project.CreateFile("/files", "File1.txt", typeof(TextFile));
                await project.CreateFile("/files", "File2.txt", typeof(TextFile));
                await solution.SaveWithAllProjects();

                // Re-open the solution
                var solution2 = await ProjectBase.OpenProjectFile("/projects/Test Solution/Test Solution.skysln", manager) as TextPreprocessorSolution;
                
                // Verify project was opened correctly
                var project2 = solution2.GetProject("/Text Preprocessor Project");
                Assert.IsNotNull(project2);
                Assert.IsInstanceOfType(project2, typeof(TextPreprocessorProject));
                Assert.IsTrue(project2.FileExists("/variables.txt"));
                Assert.IsTrue(project2.DirectoryExists("/files"));
                Assert.IsNotNull(project2.Settings, "Settings are null");
                Assert.IsTrue(project2.FileExists("/files/File1.txt"));
                Assert.IsTrue(project2.FileExists("/files/File2.txt"));

                var variablesFile = (await project2.GetFile("/variables.txt", IOHelper.PickFirstDuplicateMatchSelector, manager)) as TextFile;
                var file1 = (await project2.GetFile("/files/File1.txt", IOHelper.PickFirstDuplicateMatchSelector, manager)) as TextFile;
                var file2 = (await project2.GetFile("/files/File2.txt", IOHelper.PickFirstDuplicateMatchSelector, manager)) as TextFile;

                Assert.IsNotNull(file1);
                Assert.IsNotNull(file2);

                // Set data
                variablesFile.Contents = "var1=placeholder\nvar2=placeholder2";
                await variablesFile.Save(provider);

                file1.Contents = "var1 is %var1%";
                await file1.Save(provider);

                file2.Contents = "var2 is %var2%";
                await file2.Save(provider);

                // Build
                await project2.Build();

                // Verify correct build
                Assert.IsTrue(provider.DirectoryExists("/projects/Test Solution/Text Preprocessor Project/output"));
                Assert.IsTrue(provider.FileExists("/projects/Test Solution/Text Preprocessor Project/output/File1.txt"));
                Assert.IsTrue(provider.FileExists("/projects/Test Solution/Text Preprocessor Project/output/File2.txt"));
                Assert.AreEqual("var1 is placeholder", provider.ReadAllText("/projects/Test Solution/Text Preprocessor Project/output/File1.txt"));
                Assert.AreEqual("var2 is placeholder2", provider.ReadAllText("/projects/Test Solution/Text Preprocessor Project/output/File2.txt"));
            }
        }
    }
}
