using SkyEditor.Core;
using SkyEditor.Core.ConsoleCommands;
using System;
using System.Threading.Tasks;

namespace Sky_Editor_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var manager = new PluginManager())
            {
                manager.LoadCore(new ConsoleCorePlugin()).Wait();
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    appViewModel.CurrentConsoleShell.RunConsole().Wait();
                }
            }
        }
    }
}