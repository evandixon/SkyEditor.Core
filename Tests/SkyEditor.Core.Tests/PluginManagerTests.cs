using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        public CoreSkyEditorPlugin core { get; set; }
        public PluginManager manager { get; set; }

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
            core = new BasicTestCoreMod();
            manager = new PluginManager();
            await manager.LoadCore(core);

            Assert.IsTrue(manager.GetPlugins().Contains(core));
            Assert.IsNotNull(manager.CurrentIOProvider);
            Assert.IsNotNull(manager.CurrentSettingsProvider);
        }


        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ManualPluginLoad()
        {
            core = new ManualLoadTestCoreMod();
            manager = new PluginManager();
            await manager.LoadCore(core);

            var plugins = manager.GetPlugins();
            Assert.AreEqual(1, plugins.Where(x => x.GetType() == typeof(ManualLoadPlugin.BasicTestCoreMod)).Count());
        }


    }
}
