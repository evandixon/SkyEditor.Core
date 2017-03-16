using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.Core.Settings;

namespace SkyEditor.Core.Tests
{
    public class AutoLoadTestCoreMod : CoreSkyEditorPlugin
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

        public override ISettingsProvider GetSettingsProvider(PluginManager manager)
        {
            var provider = base.GetSettingsProvider(manager);
            provider.SetIsDevMode(true);
            return provider;
        }
    }
}
