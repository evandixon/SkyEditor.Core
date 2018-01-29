using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class WriteText : ConsoleCommand
    {
        protected override void Main(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                Console.WriteLine("Usage: WriteText <filename> <text>");
                return;
            }

            var text = new StringBuilder();
            foreach (var item in arguments)
            {
                text.Append(item);
                text.Append(" ");
            }

            CurrentIOProvider.WriteAllText(arguments[0], text.ToString());
        }
    }
}
