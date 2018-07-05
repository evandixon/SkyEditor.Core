using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands
{
    public class DistPrep : ConsoleCommand
    {
        public DistPrep(PluginManager pluginManager, IIOProvider provider) : base(provider)
        {
            CurrentPluginManager = pluginManager;
        }
        
        protected PluginManager CurrentPluginManager { get; }

        public override async Task MainAsync(string[] arguments)
        {
            CurrentPluginManager.PreparePluginsForDistribution();
            await base.MainAsync(arguments);
        }
    }
}
