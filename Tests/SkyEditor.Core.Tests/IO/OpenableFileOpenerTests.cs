using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.IO;
using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.Core.TestComponents;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.IO
{
    [TestClass]
    public class OpenableFileOpenerTests
    {
        public const string TestCategory = "I/O";

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

            public Task OpenFile(string filename, IFileSystem provider)
            {
                Contents = provider.ReadAllText(filename);
                this.Filename = filename;
                return Task.CompletedTask;
            }

            public Task Save(IFileSystem provider)
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

            public Task OpenFile(string filename, IFileSystem provider)
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

            public Task OpenFile(string filename, IFileSystem provider)
            {
                Contents = provider.ReadAllText(filename);
                this.Filename = filename;
                return Task.CompletedTask;
            }

            public Task Save(IFileSystem provider)
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

        #endregion

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenableFileOpener_FunctionalityTest()
        {
            var opener = new OpenableFileOpener(new PluginManager());
            var provider = new MemoryFileSystem();
            provider.WriteAllText("/test.txt", "TEST");

            var creatable = await opener.OpenFile(typeof(CreatableTextFile).GetTypeInfo(), "/test.txt", provider);
            Assert.IsNotNull(creatable);
            Assert.IsInstanceOfType(creatable, typeof(CreatableTextFile));
            Assert.AreEqual("TEST", (creatable as CreatableTextFile).Contents);

            var openable = await opener.OpenFile(typeof(OpenableTextFile).GetTypeInfo(), "/test.txt", provider);
            Assert.IsNotNull(openable);
            Assert.IsInstanceOfType(openable, typeof(OpenableTextFile));
            Assert.AreEqual("TEST", (openable as OpenableTextFile).Contents);            
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenableFileOpener_InvalidType()
        {
            var opener = new OpenableFileOpener(new PluginManager());
            var provider = new MemoryFileSystem();
            provider.WriteAllText("/test.txt", "TEST");

            try
            {
                var custom = await opener.OpenFile(typeof(CustomTextFile).GetTypeInfo(), "/test.txt", provider);
            }
            catch (ArgumentException)
            {
                // Pass
                return;
            }
            Assert.Fail("No exception thrown.  Expected ArgumentException");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenableFileOpener_ArgumentNullCheck_fileType()
        {
            var opener = new OpenableFileOpener(new PluginManager());
            var provider = new MemoryFileSystem();
            provider.WriteAllText("/test.txt", "TEST");

            try
            {
                var custom = await opener.OpenFile(null, "/test.txt", provider);
            }
            catch (ArgumentNullException)
            {
                // Pass
                return;
            }
            Assert.Fail("No exception thrown for argument \"fileType\".  Expected ArgumentException");            
        }


        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenableFileOpener_ArgumentNullCheck_filename()
        {
            var opener = new OpenableFileOpener(new PluginManager());
            var provider = new MemoryFileSystem();
            provider.WriteAllText("/test.txt", "TEST");            

            try
            {
                var custom = await opener.OpenFile(typeof(CreatableTextFile).GetTypeInfo(), "", provider);
            }
            catch (ArgumentNullException)
            {
                // Pass
                return;
            }
            Assert.Fail("No exception thrown for argument \"filename\".  Expected ArgumentException");
        }


        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task OpenableFileOpener_ArgumentNullCheck_provider()
        {
            var opener = new OpenableFileOpener(new PluginManager());
            var provider = new MemoryFileSystem();
            provider.WriteAllText("/test.txt", "TEST");

            try
            {
                var custom = await opener.OpenFile(typeof(CreatableTextFile).GetTypeInfo(), "/test.txt", null);
            }
            catch (ArgumentNullException)
            {
                // Pass
                return;
            }
            Assert.Fail("No exception thrown for argument \"provider\".  Expected ArgumentException");
        }
    }
}
