using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class ListFiles : ConsoleCommand
    {

        protected override void Main(string[] arguments)
        {
            var files = CurrentApplicationViewModel.OpenFiles;
            Console.WriteLine($"{files.Count} files:");
            for (var count = 0; count <= files.Count - 1; count++)
            {
                if (ReferenceEquals(CurrentApplicationViewModel.SelectedFile, files[count]))
                {
                    Console.Write("*");
                }
                Console.WriteLine($"{count}: {files[count].Title} - {files[count].Model.GetType().FullName}");
            }
        }
    }
}
