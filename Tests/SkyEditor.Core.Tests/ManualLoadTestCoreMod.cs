using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.IO.FileSystem;

namespace SkyEditor.Core.Tests
{
    public class ManualLoadTestCoreMod : CoreSkyEditorPlugin
    {
        public override string Credits
        {
            get
            {
                return "credits";
            }
        }

        public override string PluginAuthor
        {
            get
            {
                return "author";
            }
        }

        public override string PluginName
        {
            get
            {
                return "plugin";
            }
        }

        public override string GetExtensionDirectory()
        {
            return "/extensions";
        }

        public override IFileSystem GetFileSystem()
        {
            return new MemoryFileSystem();
        }

        public override ISettingsProvider GetSettingsProvider(PluginManager manager)
        {
            return SettingsProvider.Open("/settings.json", manager);
        }

        public override void Load(PluginManager manager)
        {
            base.Load(manager);

            manager.LoadRequiredPlugin(new ManualLoadPlugin.BasicTestCoreMod(), this);
        }
    }
}
