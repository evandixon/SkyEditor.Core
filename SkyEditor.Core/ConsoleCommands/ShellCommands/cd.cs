using SkyEditor.Core.IO;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class cd : ConsoleCommand
    {
        public cd(IFileSystem provider)
        {
            CurrentFileSystem = provider;
        }

        protected IFileSystem CurrentFileSystem { get; }

        protected override void Main(string[] arguments)
        {
            base.Main(arguments);
            if (arguments.Length > 1)
            {
                var directory = arguments[1];
                if (CurrentFileSystem.DirectoryExists(directory))
                {
                    CurrentFileSystem.WorkingDirectory = directory;
                }
                else
                {
                    Console.WriteLine(Properties.Resources.Console_DirectoryDoesNotExist, directory);
                }                
            }
            else
            {
                CurrentFileSystem.ResetWorkingDirectory();
            }
        }
    }
}
