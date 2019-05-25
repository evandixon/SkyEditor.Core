using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests
{
    [TestClass]
    public class PluginManagerTests
    {
        private const string TestCategory = "Plugin Manager Tests";

        public CoreSkyEditorPlugin Core { get; set; }
        public PluginManager Manager { get; set; }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task LoadCoreArgumentNull_NetCore()
        {
            var manager = new PluginManager();
            try
            {
                await manager.LoadCore(null);
            }
            catch (ArgumentNullException)
            {
                // Pass
                return;
            }
            Assert.Fail("Failed to throw ArgumentNullException.");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task CoreModInPlugins_NetCore()
        {
            Core = new BasicTestCoreMod();
            Manager = new PluginManager();
            await Manager.LoadCore(Core);

            Assert.IsTrue(Manager.GetPlugins().Contains(Core));
            Assert.IsNotNull(Manager.CurrentFileSystem);
            Assert.IsNotNull(Manager.CurrentSettingsProvider);
            Assert.IsNotNull(Manager.CurrentConsoleProvider);
        }


        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ManualPluginLoad_NetCore()
        {
            Core = new ManualLoadTestCoreMod();
            Manager = new PluginManager();
            await Manager.LoadCore(Core);

            var plugins = Manager.GetPlugins();
            Assert.AreEqual(1, plugins.Where(x => x is ManualLoadPlugin.BasicTestCoreMod).Count());
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task AutoPluginLoad_NetCore()
        {
            Core = new AutoLoadTestCoreMod();
            Manager = new PluginManager();
            await Manager.LoadCore(Core);

            var plugins = Manager.GetPlugins();
            Assert.AreEqual(1, plugins.Where(x => x.PluginName == "auto-load-plugin").Count());
        }

        #region Dependency Injection
        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CanCreateInstance_WithoutRegisteredDependencies()
        {
            var manager = new PluginManager();
            Assert.IsTrue(manager.CanCreateInstance(typeof(PluginManagerTests)), "Failed to indicate that PluginManagerTests can have an instance created");
            Assert.IsFalse(manager.CanCreateInstance(typeof(CoreSkyEditorPlugin).GetTypeInfo()), "Incorrectly indicated an abstract class can have an instance created");
            Assert.IsTrue(manager.CanCreateInstance(typeof(TestContainerClassMulti)), "Failed to indicate a class with a default constructor can be created");
            Assert.IsFalse(manager.CanCreateInstance(typeof(TestContainerClassMultiNoDefaultConstructor)), "Incorrectly indicated a class with no default constructor can be created, when no dependencies have been registered.");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CanCreateInstance_WithRegisteredDependencies()
        {
            var manager = new PluginManager();
            manager.AddSingletonDependency<StringValue, StringValue>();
            manager.AddSingletonDependency<IntegerValue>(p => new IntegerValue { IntegerItem = 5 });
            Assert.IsTrue(manager.CanCreateInstance(typeof(TestContainerClassMultiNoDefaultConstructor)), "Failed to indicate a class with registered dependencies can be created.");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CreateInstanceTests()
        {
            var manager = new PluginManager();
            var multi = manager.CreateInstance(typeof(TestContainerClassMulti));
            Assert.IsNotNull(multi);
            Assert.IsInstanceOfType(multi, typeof(TestContainerClassMulti));
            Assert.AreEqual(7, (multi as TestContainerClassMulti).IntegerItem);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CreateInstanceWith1Dependencies()
        {
            var manager = new PluginManager();
            manager.AddSingletonDependency<StringValue, StringValue>();
            var multi = manager.CreateInstance(typeof(TestContainerClassMulti));
            Assert.IsNotNull(multi);
            Assert.IsInstanceOfType(multi, typeof(TestContainerClassMulti));
            Assert.AreEqual(8, (multi as TestContainerClassMulti).IntegerItem);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CreateInstanceWith2Dependencies()
        {
            var manager = new PluginManager();
            manager.AddSingletonDependency<StringValue, StringValue>();
            manager.AddSingletonDependency<IntegerValue>(p => new IntegerValue { IntegerItem = 5 });
            var multi = manager.CreateInstance(typeof(TestContainerClassMulti));
            Assert.IsNotNull(multi);
            Assert.IsInstanceOfType(multi, typeof(TestContainerClassMulti));
            Assert.AreEqual(5, (multi as TestContainerClassMulti).IntegerItem);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CreateInstance_TestTransiency()
        {
            int value = 10;
            var manager = new PluginManager();
            manager.AddSingletonDependency<StringValue, StringValue>();
            manager.AddTransientDependency<IntegerValue>(p => new IntegerValue { IntegerItem = ++value });

            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 11
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 12
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 13
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 14
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 15

            var multi = manager.CreateInstance(typeof(TestContainerClassMulti));
            Assert.IsNotNull(multi);
            Assert.IsInstanceOfType(multi, typeof(TestContainerClassMulti));
            Assert.AreEqual(16, (multi as TestContainerClassMulti).IntegerItem);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CreateInstance_TestSingleton()
        {
            int value = 10;
            var manager = new PluginManager();
            manager.AddSingletonDependency<StringValue, StringValue>();
            manager.AddSingletonDependency<IntegerValue>(p => new IntegerValue { IntegerItem = ++value });

            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 11
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 11
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 11
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 11
            manager.CreateInstance(typeof(TestContainerClassMulti)); // Value should now be 11

            var multi = manager.CreateInstance(typeof(TestContainerClassMulti));
            Assert.IsNotNull(multi);
            Assert.IsInstanceOfType(multi, typeof(TestContainerClassMulti));
            Assert.AreEqual(11, (multi as TestContainerClassMulti).IntegerItem);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void DependencyInjection_CreateNewInstanceTests()
        {
            var original = new TestContainerClassMulti();
            original.IntegerItem = 94;
            original.StringItem = "94";

            var manager = new PluginManager();
            var newInst = manager.CreateNewInstance(original);
            Assert.IsNotNull(newInst);
            Assert.IsInstanceOfType(newInst, typeof(TestContainerClassMulti));
            Assert.AreNotEqual(94, ((TestContainerClassMulti)newInst).IntegerItem, "CreateNewInstance either cloned the class, or returned the same instance");

            original.StringItem = "Altered";
            Assert.AreNotEqual(original.StringItem, (TestContainerClassMulti)newInst, "CreateNewInstance did not create a new instance, instead returning the same instance");
        }

        public class TestContainerClassMulti
        {

            public string StringItem { get; set; }

            public int IntegerItem { get; set; }

            public TestContainerClassMulti()
            {
                StringItem = "Test!!!";
                IntegerItem = 7;
            }

            public TestContainerClassMulti(StringValue s)
            {
                StringItem = s.StringItem;
                IntegerItem = 8;
            }

            public TestContainerClassMulti(StringValue s, IntegerValue v)
            {
                StringItem = s.StringItem;
                IntegerItem = v.IntegerItem;
            }
        }

        public class TestContainerClassMultiNoDefaultConstructor
        {

            public string StringItem { get; set; }

            public int IntegerItem { get; set; }

            public TestContainerClassMultiNoDefaultConstructor(StringValue s)
            {
                StringItem = s.StringItem;
                IntegerItem = 7;
            }

            public TestContainerClassMultiNoDefaultConstructor(StringValue s, IntegerValue v)
            {
                StringItem = s.StringItem;
                IntegerItem = v.IntegerItem;
            }
        }

        public class StringValue
        {
            public string StringItem { get; set; }
        }

        public class IntegerValue
        {
            public int IntegerItem { get; set; }
        }
        #endregion
    }
}
