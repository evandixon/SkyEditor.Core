using SkyEditor.Core.IO;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class mkdir : ConsoleCommand
    {
        public mkdir(IFileSystem provider)
        {
            CurrentFileSystem = provider;
        }

        protected IFileSystem CurrentFileSystem { get; }

        protected override void Main(string[] arguments)
        {
            if (arguments.Length > 1)
            {
                var directory = arguments[1];
                if (!CurrentFileSystem.DirectoryExists(directory))
                {
                    CurrentFileSystem.CreateDirectory(directory);
                }
                else
                {
                    Console.WriteLine(Properties.Resources.Console_DirectoryAlreadyExists, directory);
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.Console_mkdir_Usage);
            }
        }
    }
}
