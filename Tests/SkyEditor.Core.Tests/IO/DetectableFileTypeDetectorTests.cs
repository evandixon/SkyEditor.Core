using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.IO
{
    [TestClass]
    public class DetectableFileTypeDetectorTests
    {
        public const string TestCategory = "I/O";
        public class File00 : IDetectableFileType
        {
            public async Task<bool> IsOfType(GenericFile file)
            {
                return await file.ReadAsync(0) == 0;
            }
        }

        public class FileFF : IDetectableFileType
        {
            public async Task<bool> IsOfType(GenericFile file)
            {
                return await file.ReadAsync(0) == 0xFF;
            }
        }

        public class AnyFile : IDetectableFileType
        {
            public Task<bool> IsOfType(GenericFile file)
            {
                return Task.FromResult(true);
            }
        }
        public class TestCoreMod : CoreSkyEditorPlugin
        {
            public override string PluginName => "";

            public override string PluginAuthor => "";

            public override string Credits => "";

            public bool AddAnyFile { get; set; }

            public override string GetExtensionDirectory()
            {
                return "/extensions";
            }

            public override IIOProvider GetIOProvider()
            {
                return new MemoryIOProvider();
            }

            public override IConsoleProvider GetConsoleProvider()
            {
                return new MemoryConsoleProvider();
            }

            public override bool IsCorePluginAssemblyDynamicTypeLoadEnabled()
            {
                return false;
            }

            public override void Load(PluginManager manager)
            {
                base.Load(manager);
                manager.RegisterType<IDetectableFileType, File00>();
                manager.RegisterType<IDetectableFileType, FileFF>();
                manager.RegisterType<IDetectableFileType, AnyFile>();
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task DetectableFileTypeDetectorTest()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                var detector = new DetectableFileTypeDetector();

                using (var file0 = new GenericFile())
                {
                    file0.CreateFile(new byte[] { 0 });
                    var results = await detector.DetectFileType(file0, manager);
                    Assert.AreEqual(2, results.Count());
                    Assert.IsTrue(results.All(x => x.MatchChance == 0.5f), "File0: Not all results have correct match percentage");
                    Assert.IsTrue(results.Any(x => ReflectionHelpers.IsOfType(x.FileType, typeof(File00).GetTypeInfo())), "File00 should have been a match File0.");
                    Assert.IsTrue(results.Any(x => ReflectionHelpers.IsOfType(x.FileType, typeof(AnyFile).GetTypeInfo())), "AnyFile should have been a match File0.");
                }

                using (var fileF = new GenericFile())
                {
                    fileF.CreateFile(new byte[] { 0xFF });
                    var results = await detector.DetectFileType(fileF, manager);
                    Assert.AreEqual(2, results.Count());
                    Assert.IsTrue(results.All(x => x.MatchChance == 0.5f), "FileF: Not all results have correct match percentage");
                    Assert.IsTrue(results.Any(x => ReflectionHelpers.IsOfType(x.FileType, typeof(FileFF).GetTypeInfo())), "FileFF should have been a match for FileF.");
                    Assert.IsTrue(results.Any(x => ReflectionHelpers.IsOfType(x.FileType, typeof(AnyFile).GetTypeInfo())), "AnyFile should have been a match FileF.");
                }
            }
               
        }
    }
}
