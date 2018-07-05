using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class cd : ConsoleCommand
    {
        public cd(IIOProvider provider) : base(provider)
        {
        }

        protected override void Main(string[] arguments)
        {
            base.Main(arguments);
            if (arguments.Length > 1)
            {
                var directory = arguments[1];
                if (CurrentIOProvider.DirectoryExists(directory))
                {
                    CurrentIOProvider.WorkingDirectory = directory;
                }
                else
                {
                    Console.WriteLine(Properties.Resources.Console_DirectoryDoesNotExist, directory);
                }                
            }
            else
            {
                CurrentIOProvider.ResetWorkingDirectory();
            }
        }
    }
}
