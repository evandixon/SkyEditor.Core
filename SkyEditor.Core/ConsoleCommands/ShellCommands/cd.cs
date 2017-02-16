using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class cd : ConsoleCommand
    {
        protected override void Main(string[] arguments)
        {
            base.Main(arguments);
            if (arguments.Length > 1)
            {
                var directory = arguments[1];
                if (CurrentApplicationViewModel.CurrentIOProvider.DirectoryExists(directory))
                {
                    CurrentApplicationViewModel.CurrentPluginManager.CurrentIOProvider.WorkingDirectory = directory;
                }
                else
                {
                    Console.WriteLine(Properties.Resources.Console_DirectoryDoesNotExist, directory);
                }                
            }
            else
            {
                CurrentApplicationViewModel.CurrentPluginManager.CurrentIOProvider.ResetWorkingDirectory();
            }
        }
    }
}
