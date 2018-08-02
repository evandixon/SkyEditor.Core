﻿using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.IO;

namespace SkyEditor.Core.ConsoleCommands
{
    public class ConsoleCorePlugin : CoreSkyEditorPlugin
    {
        public override string PluginName { get; }

        public override string PluginAuthor { get; }

        public override string Credits { get; }

        public override IIOProvider GetIOProvider()
        {
            return new PhysicalIOProvider();
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
