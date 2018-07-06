using SkyEditor.Core.Extensions;
using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class Notes : ConsoleCommand
    {
        public Notes(PluginManager currentPluginManager)
        {
            CurrentPluginManager = currentPluginManager;
        }

        protected PluginManager CurrentPluginManager { get; }
        protected override void Main(string[] arguments)
        {
            var notes = ExtensionHelper.GetExtensionBank<NoteExtension>(CurrentPluginManager).GetNotes(CurrentPluginManager);
            foreach (var item in notes)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }
    }
}
