using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands
{
    public class DistPrep : ConsoleCommand
    {
        public override async Task MainAsync(string[] arguments)
        {
            CurrentApplicationViewModel.CurrentPluginManager.PreparePluginsForDistribution();
            await base.MainAsync(arguments);
        }
    }
}
