using SkyEditor.Core.IO;
using SkyEditor.Core.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class ListProperties : ConsoleCommand
    {
        public ListProperties(ApplicationViewModel applicationViewModel)
        {
            CurrentApplicationViewModel = applicationViewModel;
        }

        protected ApplicationViewModel CurrentApplicationViewModel { get; }

        protected override void Main(string[] arguments)
        {
            if (CurrentApplicationViewModel.SelectedFile != null)
            {
                var viewModel = CurrentApplicationViewModel.SelectedFile.GetViewModel<ReflectionViewModel>(CurrentApplicationViewModel);
                foreach (var item in viewModel.GetPropertyValues())
                {
                    Console.Write(item.Key);
                    Console.Write(": ");
                    Console.WriteLine(item.Value);
                }
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
    }
}
