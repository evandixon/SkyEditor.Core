using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class ListPlugins : ConsoleCommand
    {
        public ListPlugins(ApplicationViewModel applicationViewModel, IIOProvider provider) : base(provider)
        {
            CurrentApplicationViewModel = applicationViewModel;
        }

        protected ApplicationViewModel CurrentApplicationViewModel { get; }

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
