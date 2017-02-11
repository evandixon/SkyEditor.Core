using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class ListPlugins : ConsoleCommand
    {

        protected override void Main(string[] arguments)
        {
            Console.WriteLine("Plugins:");
            foreach (var item in CurrentApplicationViewModel.CurrentPluginManager.GetPlugins())
            {
                Console.WriteLine($"{item.PluginName} ({item.GetType().GetTypeInfo().Assembly.FullName})");
            }
        }
    }
}
