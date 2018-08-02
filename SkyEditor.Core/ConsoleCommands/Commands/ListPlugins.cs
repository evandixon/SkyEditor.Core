using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class ListPlugins : ConsoleCommand
    {
        public ListPlugins(PluginManager pluginManager)
        {
            CurrentPluginManager = pluginManager;
        }

        protected PluginManager CurrentPluginManager { get; }

        protected override void Main(string[] arguments)
        {
            Console.WriteLine("Plugins:");
            foreach (var item in CurrentPluginManager.GetPlugins())
            {
                Console.WriteLine($"{item.PluginName} ({item.GetType().GetTypeInfo().Assembly.FullName})");
            }
        }
    }
}
