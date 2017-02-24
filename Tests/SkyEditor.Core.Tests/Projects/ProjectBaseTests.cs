using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Projects;
using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.IO;
using System.Threading.Tasks;
using System.Linq;

namespace SkyEditor.Core.Tests.Projects
{
    [TestClass]
    public class ProjectBaseTests
    {
        public const string TestCategory = "Projects - ProjectBase";

        public class TestProject : ProjectBase
        {
            public override string ProjectFileExtension => throw new NotImplementedException();

            protected override Task<IOnDisk> LoadProjectItem(ItemValue item)
            {
                throw new NotImplementedException();
            }

            public new void AddItem(string path, IOnDisk item)
            {
                base.AddItem(path, item);
            }
        }

        public class SampleIOnDisk : IOnDisk
        {
            public SampleIOnDisk(string filename)
            {
                this.Filename = filename;
            }
            public string Filename { get; set; }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetItems_SpecificPath()
        {
            using (var proj = new TestProject())
            {
                proj.AddItem("/file1", new SampleIOnDisk("/file1"));
                proj.AddItem("/Dir1/file1", new SampleIOnDisk("/Dir1/file1"));
                proj.AddItem("/Dir1/file2", new SampleIOnDisk("/Dir1/file2"));
                proj.AddItem("/Dir1/Dir2/file1", new SampleIOnDisk("/Dir1/Dir2/file1"));
                proj.AddItem("/Dir1/Dir2/file2", new SampleIOnDisk("/Dir1/Dir2/file2"));
                proj.AddItem("/Dir1/Dir2/Dir3/file1", new SampleIOnDisk("/Dir1/Dir2/Dir3/file1"));

                var items = proj.GetItems("/Dir1", false);
                Assert.AreEqual(2, items.Count);
                Assert.IsTrue(items.Any(x => x.Value.Filename == "/Dir1/file1"));
                Assert.IsTrue(items.Any(x => x.Value.Filename == "/Dir1/file2"));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetItems_Recursive()
        {
            using (var proj = new TestProject())
            {
                proj.AddItem("/file1", new SampleIOnDisk("/file1"));
                proj.AddItem("/Dir1/file1", new SampleIOnDisk("/Dir1/file1"));
                proj.AddItem("/Dir1/file2", new SampleIOnDisk("/Dir1/file2"));
                proj.AddItem("/Dir1/Dir2/file1", new SampleIOnDisk("/Dir1/Dir2/file1"));
                proj.AddItem("/Dir1/Dir2/file2", new SampleIOnDisk("/Dir1/Dir2/file2"));
                proj.AddItem("/Dir1/Dir2/Dir3/file1", new SampleIOnDisk("/Dir1/Dir2/Dir3/file1"));

                var items = proj.GetItems("/Dir1/Dir2", true);
                Assert.AreEqual(3, items.Count);
                Assert.IsTrue(items.Any(x => x.Value.Filename == "/Dir1/Dir2/file1"));
                Assert.IsTrue(items.Any(x => x.Value.Filename == "/Dir1/Dir2/file2"));
                Assert.IsTrue(items.Any(x => x.Value.Filename == "/Dir1/Dir2/Dir3/file1"));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetDirectories_SpecificPath()
        {
            using (var proj = new TestProject())
            {
                proj.CreateDirectory("/Dir1/dir1");
                proj.CreateDirectory("/Dir1/dir2");
                proj.CreateDirectory("/Dir1/Dir2/dir1");
                proj.CreateDirectory("/Dir1/Dir2/dir2");
                proj.CreateDirectory("/Dir1/Dir2/Dir3");
                proj.CreateDirectory("/Dir1/Dir2/Dir3/dir1");

                var items = proj.GetDirectories("/Dir1", false);
                Assert.AreEqual(2, items.Count());
                Assert.IsTrue(items.Any(x => x == "/Dir1/dir1"));
                Assert.IsTrue(items.Any(x => x == "/Dir1/dir2"));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetDirectories_Recursive()
        {
            using (var proj = new TestProject())
            {
                proj.CreateDirectory("/Dir1/dir1");
                proj.CreateDirectory("/Dir1/dir2");
                proj.CreateDirectory("/Dir1/Dir2/dir1");
                proj.CreateDirectory("/Dir1/Dir2/dir2");
                proj.CreateDirectory("/Dir1/Dir2/Dir3");
                proj.CreateDirectory("/Dir1/Dir2/Dir3/dir1");                

                var items = proj.GetDirectories("/Dir1/Dir2", true);
                Assert.AreEqual(4, items.Count());
                Assert.IsTrue(items.Any(x => x == "/Dir1/Dir2/dir1"));
                Assert.IsTrue(items.Any(x => x == "/Dir1/Dir2/dir2"));
                Assert.IsTrue(items.Any(x => x == "/Dir1/Dir2/Dir3"));
                Assert.IsTrue(items.Any(x => x == "/Dir1/Dir2/Dir3/dir1"));
            }
        }
    }
}
