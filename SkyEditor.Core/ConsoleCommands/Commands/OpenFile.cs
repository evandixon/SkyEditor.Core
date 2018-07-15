using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class OpenFile : ConsoleCommand
    {
        public OpenFile(ApplicationViewModel applicationViewModel, PluginManager currentPluginManager)
        {
            CurrentApplicationViewModel = applicationViewModel;
            CurrentPluginManager = currentPluginManager;
        }

        protected ApplicationViewModel CurrentApplicationViewModel { get; }
        protected PluginManager CurrentPluginManager { get; }

        public FileTypeDetectionResult DuplicateMatchSelector(IEnumerable<FileTypeDetectionResult> matches)
        {
            var list = matches.ToList();
            Console.WriteLine(Properties.Resources.Console_OpenFile_DuplicateList);
            for (int i = 0;i<list.Count;i++)
            {
                Console.WriteLine($"{i}: {list[i].FileType} ({list[i].MatchChance.ToString("P")}");
            }

            int choiceIndex = -1;
            do
            {
                Console.Write(Properties.Resources.Console_OpenFile_DuplicateChoose);
                var choice = Console.ReadLine();
                if (!int.TryParse(choice, out choiceIndex))
                {
                    Console.WriteLine(Properties.Resources.Console_OpenFile_InvalidInput, choice);
                }
                if (choiceIndex >= list.Count)
                {
                    Console.WriteLine(Properties.Resources.Console_OpenFile_InputOutOfRange);
                }
            }
            while (choiceIndex < 0 && choiceIndex >= list.Count);
            return list[choiceIndex];
        }

        public async override Task MainAsync(string[] arguments)
        {
            await base.MainAsync(arguments);

            if (arguments.Length > 1)
            {
                var filename = arguments[1];
                var fileType = arguments.Length > 2 ? ReflectionHelpers.GetTypeByName(arguments[2], CurrentPluginManager) : null;

                if (fileType == null)
                {
                    await CurrentApplicationViewModel.OpenFile(filename);
                }
                else
                {
                    await CurrentApplicationViewModel.OpenFile(filename, fileType);
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.Console_OpenFile_Usage);
            }
        }
    }
}
