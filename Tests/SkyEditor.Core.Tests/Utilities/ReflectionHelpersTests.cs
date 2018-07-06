using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.Utilities
{
    [TestClass]
    public class ReflectionHelpersTests
    {
        private const string ReflectionHelperCategory = "Reflection Tests";

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
                return true;
            }

            public override void Load(PluginManager manager)
            {
                base.Load(manager);
            }
        }

        [TestMethod]
        [TestCategory(ReflectionHelperCategory)]
        public void IsOfType_TypeInfo()
        {
            // Standard equality
            Assert.IsTrue(ReflectionHelpers.IsOfType(typeof(ReflectionHelpersTests).GetTypeInfo(), typeof(ReflectionHelpersTests).GetTypeInfo()), "Failed to see type equality (ReflectionHelpersTests is of type ReflectionHelpersTests)");
            // Interface checks
            Assert.IsTrue(ReflectionHelpers.IsOfType(typeof(GenericFile).GetTypeInfo(), typeof(IOpenableFile).GetTypeInfo()), "Failed to see interface IOpenableFile on GenericFile");
            // Inheritance tests
            Assert.IsTrue(ReflectionHelpers.IsOfType(typeof(CoreSkyEditorPlugin).GetTypeInfo(), typeof(SkyEditorPlugin).GetTypeInfo()), "Failed to see CoreSkyEditorPlugin inherits SkyEditorPlugin");
            // Make sure it returns false sometimes
            Assert.IsFalse(ReflectionHelpers.IsOfType(typeof(string).GetTypeInfo(), typeof(int).GetTypeInfo()), "Failed to see String is not of type Integer");
        }

        [TestMethod]
        [TestCategory(ReflectionHelperCategory)]
        public void IsOfType_Overloads()
        {
            // Standard equality
            Assert.IsTrue(ReflectionHelpers.IsOfType(typeof(ReflectionHelpersTests).GetTypeInfo(), typeof(ReflectionHelpersTests)), "Failed to see type equality (ReflectionHelpersTests is of type ReflectionHelpersTests)");
            // Interface checks
            Assert.IsTrue(ReflectionHelpers.IsOfType(new GenericFile(), typeof(IOpenableFile)), "Failed to see interface IOpenableFile on GenericFile");
            // Inheritance tests
            Assert.IsTrue(ReflectionHelpers.IsOfType(typeof(CoreSkyEditorPlugin), typeof(SkyEditorPlugin)), "Failed to see CoreSkyEditorPlugin inherits SkyEditorPlugin");
            // Make sure it returns false sometimes
            Assert.IsFalse(ReflectionHelpers.IsOfType(typeof(string), typeof(int)), "Failed to see String is not of type Integer");
        }

        [TestMethod]
        [TestCategory(ReflectionHelperCategory)]
        public async Task GetTypeByNameTests()
        {
            // This one is a little hard to test due to the nature of reflection in a plugin-based environment
            // We will only test types that this function has no excuse to not be able to find

            using (PluginManager manager = new PluginManager())
            {
                await manager.LoadCore(new TestCoreMod());

                // Test PluginManager
                var managerType = ReflectionHelpers.GetTypeByName(typeof(PluginManager).AssemblyQualifiedName, manager);
                Assert.IsNotNull(managerType);
                Assert.AreEqual(typeof(PluginManager), managerType);

                // Test String
                var stringType = ReflectionHelpers.GetTypeByName(typeof(string).AssemblyQualifiedName, manager);
                Assert.IsNotNull(stringType);
                Assert.AreEqual(typeof(string), stringType);

                // Test something that's NOT an assembly qualified name, and SHOULD return null
                var bogusType = ReflectionHelpers.GetTypeByName(Guid.NewGuid().ToString(), manager);
                Assert.IsNull(bogusType);
            }

        }    

        [TestMethod]
        [TestCategory(ReflectionHelperCategory)]
        public void GetTypeFriendlyNameTests()
        {
            // Test default names
            Assert.IsFalse(string.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(typeof(ReflectionHelpersTests))));
            Assert.IsFalse(string.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(typeof(int))));
            Assert.IsFalse(string.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(typeof(GenericFile))));
            Assert.IsFalse(string.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(typeof(Solution))));

            // Test the pre-defined friendly name for TestHelpers
            Assert.AreEqual("Test Helper Class", ReflectionHelpers.GetTypeFriendlyName(typeof(TestHelpers)));
        }
    }
}
