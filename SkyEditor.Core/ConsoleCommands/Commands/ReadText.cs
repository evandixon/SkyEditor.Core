using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class ReadText : ConsoleCommand
    {
        protected override void Main(string[] arguments)
        {
            if (!arguments.Any())
            {
                Console.WriteLine("Usage: ReadText <filename>");
                return;
            }

            Console.WriteLine(CurrentIOProvider.ReadAllText(arguments[0]));
        }
    }
}
