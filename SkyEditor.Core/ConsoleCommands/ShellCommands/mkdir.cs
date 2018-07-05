using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class mkdir : ConsoleCommand
    {
        public mkdir(IIOProvider provider)
        {
            CurrentIOProvider = provider;
        }

        protected IIOProvider CurrentIOProvider { get; }

        protected override void Main(string[] arguments)
        {
            if (arguments.Length > 1)
            {
                var directory = arguments[1];
                if (!CurrentIOProvider.DirectoryExists(directory))
                {
                    CurrentIOProvider.CreateDirectory(directory);
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
