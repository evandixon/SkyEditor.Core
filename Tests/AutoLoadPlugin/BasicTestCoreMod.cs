using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core;

namespace AutoLoadPlugin
{
    public class BasicTestCoreMod : CoreSkyEditorPlugin
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

        public override IIOProvider GetIOProvider()
        {
            return new MemoryIOProvider();
        }

        public override ISettingsProvider GetSettingsProvider(PluginManager manager)
        {
            return SettingsProvider.Open("/settings.json", manager);
        }
    }
}
