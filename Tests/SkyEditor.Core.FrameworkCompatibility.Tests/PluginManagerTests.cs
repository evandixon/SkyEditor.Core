using ManualLoadPlugin.FrameworkCompatibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task LoadCoreArgumentNull()
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
        public async Task CoreModInPlugins()
        {
            Core = new BasicTestCoreMod();
            Manager = new PluginManager();
            await Manager.LoadCore(Core);

            Assert.IsTrue(Manager.GetPlugins().Contains(Core));
            Assert.IsNotNull(Manager.CurrentIOProvider);
            Assert.IsNotNull(Manager.CurrentSettingsProvider);
            Assert.IsNotNull(Manager.CurrentConsoleProvider);
        }


        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ManualPluginLoad()
        {
            Core = new ManualLoadTestCoreMod();
            Manager = new PluginManager();
            await Manager.LoadCore(Core);

            var plugins = Manager.GetPlugins();
            Assert.AreEqual(1, plugins.Where(x => x is BasicTestCoreMod).Count());
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task AutoPluginLoad()
        {
            Core = new AutoLoadTestCoreMod();
            Manager = new PluginManager();
            await Manager.LoadCore(Core);

            var plugins = Manager.GetPlugins();
            Assert.AreEqual(1, plugins.Where(x => x.PluginName == "auto-load-plugin").Count());
        }
    }
}
