using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;

namespace SkyEditor.Core.ConsoleCommands
{
    public class ConsoleCorePlugin : CoreSkyEditorPlugin
    {
        public override string PluginName { get; }

        public override string PluginAuthor { get; }

        public override string Credits { get; }

        public override string GetExtensionDirectory()
        {
            return "extensions";
        }

        public override IIOProvider GetIOProvider()
        {
            var provider = new CompositeIOProvider(new PhysicalIOProvider());
            provider.MountProvider("ram", new MemoryIOProvider());
            return provider;
        }

        public override IConsoleProvider GetConsoleProvider()
        {
            return new StandardConsoleProvider();
        }

        public override ISettingsProvider GetSettingsProvider(PluginManager manager)
        {
            return SettingsProvider.Open("settings.json", manager);
        }
    }
}
