using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.ShellCommands
{
    public class echo : ConsoleCommand
    {
        protected override void Main(string[] arguments)
        {
            foreach (var item in arguments)
            {
                Console.Write(item);
            }

            Console.Write("\n");
        }
    }
}
