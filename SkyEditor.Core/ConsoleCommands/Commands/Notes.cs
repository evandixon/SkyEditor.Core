using SkyEditor.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class Notes : ConsoleCommand
    {
        protected override void Main(string[] arguments)
        {
            var notes = ExtensionHelper.GetExtensionBank<NoteExtension>(CurrentApplicationViewModel.CurrentPluginManager).GetNotes(CurrentApplicationViewModel.CurrentPluginManager);
            foreach (var item in notes)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }
    }
}
