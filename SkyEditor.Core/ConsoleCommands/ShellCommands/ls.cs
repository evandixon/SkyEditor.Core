using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class ls : ConsoleCommand
    {
        public ls(IIOProvider provider)
        {
            CurrentIOProvider = provider;
        }

        protected IIOProvider CurrentIOProvider { get; }

        protected override void Main(string[] arguments)
        {
            base.Main(arguments);

            var provider = CurrentIOProvider;
            var directory = ".";
            if (arguments.Length > 1)
            {
                directory = arguments[1];
            }

            var originalColor = Console.ForegroundColor;
            if (provider.DirectoryExists(directory))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var item in provider.GetDirectories(directory, true))
                {
                    Console.WriteLine(Path.GetFileName(item));
                }
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var item in provider.GetFiles(directory, "*", true))
                {
                    Console.WriteLine(Path.GetFileName(item));
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.Console_DirectoryDoesNotExist, directory);
            }

            Console.ForegroundColor = originalColor;
        }
    }
}
