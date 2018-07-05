using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class SelectFile : ConsoleCommand
    {
        public SelectFile(ApplicationViewModel applicationViewModel, IIOProvider provider) : base(provider)
        {
            CurrentApplicationViewModel = applicationViewModel;
        }

        protected ApplicationViewModel CurrentApplicationViewModel { get; }

        protected override void Main(string[] arguments)
        {
            if (arguments.Length > 1)
            {
                int fileIndex = 0;
                if (int.TryParse(arguments[1], out fileIndex))
                {
                    CurrentApplicationViewModel.SelectedFile = CurrentApplicationViewModel.OpenFiles[fileIndex];
                    Console.WriteLine(Path.GetFileName(CurrentApplicationViewModel.SelectedFile.Filename));
                }
                else
                {
                    Console.WriteLine(Properties.Resources.Console_SelectFile_InvalidInput);
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.Console_SelectFile_Usage);
            }
        }
    }
}
