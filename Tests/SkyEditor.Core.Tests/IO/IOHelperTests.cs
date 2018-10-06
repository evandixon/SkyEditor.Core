using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.IO;
using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.IO
{
    [TestClass]
    public class IOHelperTests
    {
        public const string TestCategory = "I/O - IOHelper";

        #region Child Classes
        public class CreatableTextFile : ICreatableFile
        {
            public string Filename { get; set; }

            public string Name { get; set; }

            public string Contents { get; set; }

            public event EventHandler FileSaved;

            public void CreateFile(string name)
            {
                Contents = string.Empty;
                this.Name = name;
            }

            public Task OpenFile(string filename, IIOProvider provider)
            {
                Contents = provider.ReadAllText(filename);
                this.Filename = filename;
                return Task.CompletedTask;
            }

            public Task Save(IIOProvider provider)
            {
                provider.WriteAllText(Filename, Contents);
                FileSaved?.Invoke(this, new EventArgs());
                return Task.CompletedTask;
            }
        }

        public class OpenableTextFile : IOpenableFile, IDetectableFileType
        {
            public string Contents { get; set; }

            public Task<bool> IsOfType(GenericFile file)
            {
                return Task.FromResult(true);
            }

            public Task OpenFile(string filename, IIOProvider provider)
            {
                Contents = provider.ReadAllText(filename);
                return Task.CompletedTask;
            }
        }

        public class NoDefaultConstructorCreatableFile : ICreatableFile
        {
            public NoDefaultConstructorCreatableFile(object dummy)
            {
            }

            public string Filename { get; set; }

            public string Name { get; set; }

            public string Contents { get; set; }

            public event EventHandler FileSaved;

            public void CreateFile(string name)
            {
                Contents = string.Empty;
                this.Name = name;
            }

            public Task OpenFile(string filename, IIOProvider provider)
            {
                Contents = provider.ReadAllText(filename);
                this.Filename = filename;
                return Task.CompletedTask;
            }

            public Task Save(IIOProvider provider)
            {
                provider.WriteAllText(Filename, Contents);
                FileSaved?.Invoke(this, new EventArgs());
                return Task.CompletedTask;
            }
        }

        public class CustomTextFile
        {
            public string Contents { get; set; }
        }

        public class CustomFileOpener : IFileOpener
        {
            public int GetUsagePriority(TypeInfo fileType)
            {
                return 10;
            }

            public Task<object> OpenFile(TypeInfo fileType, string filename, IIOProvider provider)
            {
                var output = new CustomTextFile();
                output.Contents = provider.ReadAllText(filename);
                return Task.FromResult(output as object);
            }

            public bool SupportsType(TypeInfo fileType)
            {
                return (fileType == typeof(CustomTextFile).GetTypeInfo());
            }
        }

        /// <summary>
        /// Detects files supported by <see cref="CustomTextFile"/> with the "custom" file extension.  Has higher priority than IDetectableFileType
        /// </summary>
        public class CustomFileTypeDetector : IFileTypeDetector
        {

            public Task<IEnumerable<FileTypeDetectionResult>> DetectFileType(GenericFile file, PluginManager manager)
            {
                if (Path.GetExtension(file.Filename) == ".custom")
                {
                    return Task.FromResult(new FileTypeDetectionResult[] { new FileTypeDetectionResult { FileType = typeof(CustomTextFile).GetTypeInfo(), MatchChance = 1 } } as IEnumerable<FileTypeDetectionResult>);
                }
                else
                {
                    return Task.FromResult(Enumerable.Empty<FileTypeDetectionResult>());
                }
            }
        }

        public class ADirectoryFormat
        {
            public string Path { get; set; }
        }

        public class AnotherDirectoryFormat
        {
            public string Path { get; set; }
        }

        public class DirectoryOpener : IFileOpener
        {
            public int GetUsagePriority(TypeInfo fileType)
            {
                return 0;
            }

            public Task<object> OpenFile(TypeInfo fileType, string filename, IIOProvider provider)
            {
                var output = new ADirectoryFormat();
                output.Path = filename;
                return Task.FromResult(output as object);
            }

            public bool SupportsType(TypeInfo fileType)
            {
                return fileType == typeof(ADirectoryFormat).GetTypeInfo();
            }
        }

        public class DirectoryDetector : IDirectoryTypeDetector
        {
            public Task<IEnumerable<FileTypeDetectionResult>> DetectDirectoryType(string path, PluginManager manager)
            {
                if (path.ToLower().Contains("another"))
                {
                    return Task.FromResult(new FileTypeDetectionResult[] { new FileTypeDetectionResult { FileType = typeof(AnotherDirectoryFormat).GetTypeInfo(), MatchChance = 1 } } as IEnumerable<FileTypeDetectionResult>);
                }
                else
                {
                    return Task.FromResult(new FileTypeDetectionResult[] { new FileTypeDetectionResult { FileType = typeof(ADirectoryFormat).GetTypeInfo(), MatchChance = 0.5f } } as IEnumerable<FileTypeDetectionResult>);
                }
            }
        }

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

            public override bool IsCorePluginAssemblyDynamicTypeLoadEnabled()
            {
                return false;
            }

            public override void Load(PluginManager manager)
            {
                //base.Load(manager);

                manager.RegisterType<IFileOpener, OpenableFileOpener>();
                manager.RegisterType<IFileTypeDetector, DetectableFileTypeDetector>();

                manager.RegisterType<IFileOpener, CustomFileOpener>();
                manager.RegisterType<IFileOpener, DirectoryOpener>();
                manager.RegisterType<IFileTypeDetector, CustomFileTypeDetector>();
                manager.RegisterType<IDirectoryTypeDetector, DirectoryDetector>();

                manager.RegisterType<ICreatableFile, CreatableTextFile>();
                manager.RegisterType<ICreatableFile, NoDefaultConstructorCreatableFile>();
                manager.RegisterType<IOpenableFile, CreatableTextFile>();
                manager.RegisterType<IOpenableFile, OpenableTextFile>();
                manager.RegisterType<IDetectableFileType, OpenableTextFile>();
            }
        }

        #endregion

        #region Null Parameter Tests
        private void TestStaticFunction(Type type, string functionName, string testParamName, Type[] types, params object[] args)
        {
            var method = type.GetTypeInfo().GetMethod(functionName, types);
            try
            {
                var result = method.Invoke(null, args);
                if (result is Task)
                {
                    (result as Task).Wait();
                }
            }
            catch (ArgumentNullException)
            {
                // Pass
                return;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count > 1)
                {
                    Assert.Fail($"Too many exceptions for parameter \"{testParamName}\".  Details: " + ex.ToString());
                }
                else if (ex.InnerExceptions.Count == 1)
                {
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(ArgumentNullException));
                    return;
                }
                else
                {
                    Assert.Fail("Got an AggregateException without any inner exceptions.");
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is ArgumentNullException)
                {
                    // Pass
                    return;
                }
                else
                {
                    Assert.Fail("Got an TargetInvocationException with incorrect exception type.  Details: " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Incorrect exception thrown for param \"{testParamName}\".  Should be ArgumentNullException.  Details: " + ex.ToString());
            }
            Assert.Fail($"No exception thrown for param \"{testParamName}\".");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetCreatableFileTypes_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetCreateableFileTypes), "manager", new Type[] { typeof(PluginManager) }, new object[] { null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetOpenableFileTypes_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetOpenableFileTypes), "manager", new Type[] { typeof(PluginManager) }, new object[] { null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void CreateNewFile_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.CreateNewFile), "fileType", new Type[] { typeof(string), typeof(TypeInfo) }, new object[] { null, null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void OpenFile_SpecificType_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "filename", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "fileType", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "test.txt", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "test.txt", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void OpenFile_AutoDetect_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "path", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "duplicateFileTypeSelector", new Type[] { typeof(string), typeof(TypeInfo), typeof(PluginManager) }, new object[] { "test.txt", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "test.txt", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void OpenDirectory_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenDirectory), "path", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenDirectory), "duplicateFileTypeSelector", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenDirectory), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetFileType_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetFileType), "path", new Type[] { typeof(GenericFile), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetFileType), "duplicateFileTypeSelector", new Type[] { typeof(GenericFile), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { new GenericFile(), null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetFileType), "manager", new Type[] { typeof(GenericFile), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { new GenericFile(), new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetDirectoryType_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetDirectoryType), "path", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetDirectoryType), "duplicateFileTypeSelector", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetDirectoryType), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        #endregion

        #region Functionality Tests

        [TestMethod]
        [TestCategory(TestCategory)]
        public void PickFirstDuplicateMatchSelector_FunctionalityTest()
        {
            var testData = new List<FileTypeDetectionResult>();
            var item1 = new FileTypeDetectionResult();
            testData.Add(item1);
            testData.Add(new FileTypeDetectionResult());
            testData.Add(new FileTypeDetectionResult());
            testData.Add(new FileTypeDetectionResult());
            testData.Add(new FileTypeDetectionResult());
            testData.Add(new FileTypeDetectionResult());
            testData.Add(new FileTypeDetectionResult());

            Assert.ReferenceEquals(item1, IOHelper.PickFirstDuplicateMatchSelector(testData));
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetCreatableFileTypes_FunctionalityTest()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            var types = IOHelper.GetCreateableFileTypes(manager).ToList();
            Assert.AreEqual(1, types.Count, "Incorrect number of ICreatableFile types.");
            Assert.IsTrue(types.All(x => ReflectionHelpers.IsOfType(x, typeof(CreatableTextFile).GetTypeInfo())));
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetOpenableFileTypes_FunctionalityTest()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            var types = IOHelper.GetOpenableFileTypes(manager).ToList();
            Assert.AreEqual(2, types.Count, "Incorrect number of IOpenableFile types.");
            Assert.IsTrue(types.Any(x => ReflectionHelpers.IsOfType(x, typeof(CreatableTextFile).GetTypeInfo())));
            Assert.IsTrue(types.Any(x => ReflectionHelpers.IsOfType(x, typeof(OpenableTextFile).GetTypeInfo())));
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CreateNewFile_FunctionalityTest_ValidType()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            var newFile = IOHelper.CreateNewFile("Test", typeof(CreatableTextFile).GetTypeInfo(), manager);
            Assert.IsNotNull(newFile);
            Assert.IsInstanceOfType(newFile, typeof(CreatableTextFile));
            Assert.AreEqual("Test", newFile.Name);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CreateNewFile_FunctionalityTest_IncompatibleType()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            try
            {
                var newFile = IOHelper.CreateNewFile("Test", typeof(OpenableTextFile).GetTypeInfo(), manager);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Incorrect exception thrown.  Details: " + ex.ToString());
            }
            Assert.Fail("Expected ArgumentException");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CreateNewFile_FunctionalityTest_NoDefaultConstructor()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            try
            {
                var newFile = IOHelper.CreateNewFile("Test", typeof(NoDefaultConstructorCreatableFile).GetTypeInfo(), manager);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Incorrect exception thrown.  Details: " + ex.ToString());
            }
            Assert.Fail("Expected ArgumentException");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_SpecificType_FunctionalityTest_ValidType()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());
            manager.CurrentIOProvider.WriteAllText("/test.txt", "magic data");

            // Test 1
            var file1 = await IOHelper.OpenFile("/test.txt", typeof(OpenableTextFile).GetTypeInfo(), manager);
            Assert.IsNotNull(file1);
            Assert.IsInstanceOfType(file1, typeof(OpenableTextFile));
            Assert.AreEqual("magic data", (file1 as OpenableTextFile).Contents);

            // Test 2
            var file2 = await IOHelper.OpenFile("/test.txt", typeof(CreatableTextFile).GetTypeInfo(), manager);
            Assert.IsNotNull(file2);
            Assert.IsInstanceOfType(file2, typeof(CreatableTextFile));
            Assert.AreEqual("magic data", (file2 as CreatableTextFile).Contents);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_SpecificType_FunctionalityTest_Directory()
        {
            // Set up
            var manager = new PluginManager();
            var coreMod = new TestCoreMod();
            await manager.LoadCore(coreMod);
            manager.CurrentIOProvider.CreateDirectory("/test");

            // Test
            var dir = await IOHelper.OpenFile("/test", typeof(ADirectoryFormat).GetTypeInfo(), manager);
            Assert.IsNotNull(dir);
            if (dir is ADirectoryFormat)
            {
                Assert.AreEqual("/test", (dir as ADirectoryFormat).Path);
            }
            else
            {
                Assert.Fail("Opened type should have been ADirectoryFormat.  Actual type: " + dir.GetType().Name);
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_SpecificType_FunctionalityTest_IncompatibleType()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            try
            {
                var newFile = await IOHelper.OpenFile("Test", typeof(string).GetTypeInfo(), manager);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Incorrect exception thrown.  Details: " + ex.ToString());
            }
            Assert.Fail("Expected ArgumentException");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_SpecificType_FunctionalityTest_NoDefaultConstructor()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());

            // Assert
            try
            {
                var newFile = await IOHelper.OpenFile("Test", typeof(NoDefaultConstructorCreatableFile).GetTypeInfo(), manager);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Incorrect exception thrown.  Details: " + ex.ToString());
            }
            Assert.Fail("Expected ArgumentException");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_AutoDetect_FunctionalityTest_ValidType()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());
            manager.CurrentIOProvider.WriteAllText("/test.txt", "magic data");

            // Test
            var file = await IOHelper.OpenFile("/test.txt", IOHelper.PickFirstDuplicateMatchSelector, manager);
            Assert.IsNotNull(file);
            if (file is OpenableTextFile)
            {
                Assert.AreEqual("magic data", (file as OpenableTextFile).Contents);
            }
            else if (file is CreatableTextFile)
            {
                Assert.AreEqual("magic data", (file as CreatableTextFile).Contents);
            }
            else
            {
                Assert.Fail("Opened file should implement OpenableTextFile or CreatableTextFile.  Actual type: " + file.GetType().Name);
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_AutoDetect_FunctionalityTest_CustomOpenerAndDetector()
        {
            // Set up
            var manager = new PluginManager();
            var coreMod = new TestCoreMod();
            await manager.LoadCore(coreMod);
            manager.CurrentIOProvider.WriteAllText("/test.custom", "magic data");

            // Test
            var file = await IOHelper.OpenFile("/test.custom", IOHelper.PickFirstDuplicateMatchSelector, manager);
            Assert.IsNotNull(file);
            if (file is CustomTextFile)
            {
                Assert.AreEqual("magic data", (file as CustomTextFile).Contents);
            }
            else
            {
                Assert.Fail("Opened type should have been CustomTextFile.  Actual type: " + file.GetType().Name);
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenFile_AutoDetect_FunctionalityTest_MissingFile()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());
            manager.CurrentIOProvider.WriteAllText("/test.txt", "magic data");

            // Assert
            try
            {
                var newFile = await IOHelper.OpenFile("/anotherFile.txt", IOHelper.PickFirstDuplicateMatchSelector, manager);
            }
            catch (FileNotFoundException)
            {
                // Pass
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Incorrect exception thrown.  Details: " + ex.ToString());
            }
            Assert.Fail("Expected ArgumentException");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenDirectory_AutoDetect_FunctionalityTest_ValidType()
        {
            // Set up
            var manager = new PluginManager();
            var coreMod = new TestCoreMod();
            await manager.LoadCore(coreMod);
            manager.CurrentIOProvider.CreateDirectory("/test");

            // Test
            var dir = await IOHelper.OpenDirectory("/test", IOHelper.PickFirstDuplicateMatchSelector, manager);
            Assert.IsNotNull(dir);
            if (dir is ADirectoryFormat)
            {
                Assert.AreEqual("/test", (dir as ADirectoryFormat).Path);
            }
            else
            {
                Assert.Fail("Opened type should have been ADirectoryFormat.  Actual type: " + dir.GetType().Name);
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetFileType_FunctionalityTest()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());
            manager.CurrentIOProvider.WriteAllText("/test.txt", "magic data");
            manager.CurrentIOProvider.WriteAllText("/test.custom", "custom data");

            // Test
            using (var txtFile = new GenericFile())
            {
                await txtFile.OpenFile("/test.txt", manager.CurrentIOProvider);
                Assert.AreEqual(typeof(OpenableTextFile).GetTypeInfo(), await IOHelper.GetFileType(txtFile, IOHelper.PickFirstDuplicateMatchSelector, manager));
            }
            using (var customFile = new GenericFile())
            {
                await customFile.OpenFile("/test.custom", manager.CurrentIOProvider);
                Assert.AreEqual(typeof(CustomTextFile).GetTypeInfo(), await IOHelper.GetFileType(customFile, IOHelper.PickFirstDuplicateMatchSelector, manager));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetDirectoryType_FunctionalityTest()
        {
            // Set up
            var manager = new PluginManager();
            await manager.LoadCore(new TestCoreMod());
            manager.CurrentIOProvider.CreateDirectory("/Dir");
            manager.CurrentIOProvider.CreateDirectory("/anotherDir");

            // Test
            Assert.AreEqual(typeof(ADirectoryFormat).GetTypeInfo(), await IOHelper.GetDirectoryType("/Dir", IOHelper.PickFirstDuplicateMatchSelector, manager));
            Assert.AreEqual(typeof(AnotherDirectoryFormat).GetTypeInfo(), await IOHelper.GetDirectoryType("/anotherDir", IOHelper.PickFirstDuplicateMatchSelector, manager));
        }
        #endregion
    }
}
