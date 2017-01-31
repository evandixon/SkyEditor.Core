using SkyEditor.Core.TestComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands
{
    /// <summary>
    /// Provides the core logic for the Sky Editor console.
    /// </summary>
    public class ConsoleManager
    {
        /// <summary>
        /// Runs a console command with custom input and returns the output.
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <param name="manager">Plugin manager to use</param>
        /// <param name="arguments">Input arguments</param>
        /// <param name="stdIn">New-line separated standard input</param>
        /// <returns>New-line separated standard output</returns>
        public static async Task<string> TestConsoleCommand(ConsoleCommand command, PluginManager manager, string[] arguments, string stdIn)
        {
            var provider = new MemoryConsoleProvider();
            provider.StdIn.Append(stdIn);
            command.CurrentPluginManager = manager;
            command.Console = provider;
            await command.MainAsync(arguments).ConfigureAwait(false);
            return provider.GetStdOut();
        }

        public ConsoleManager(PluginManager manager)
        {
            Console = manager.CurrentConsoleProvider;
            foreach (ConsoleCommand item in manager.GetRegisteredObjects<ConsoleCommand>())
            {
                item.CurrentPluginManager = manager;
                item.Console = Console;
                AllCommands.Add(item.CommandName, item);
            }
        }

        protected Dictionary<string, ConsoleCommand> AllCommands { get; set; }
        protected IConsoleProvider Console { get; set; }
        protected Regex ParameterRegex => new Regex("(\\\".*?\\\")|\\S+)", RegexOptions.Compiled);

        /// <summary>
        /// Listens for user input from the console provider provided via the plugin manager from <see cref="ConsoleManager.New(PluginManager)"/> and handles commands accordingly.
        /// </summary>
        /// <returns></returns>
        public async Task RunConsole()
        {
            while (true)
            {
                var cmdParts = Console.ReadLine().Split(" ".ToCharArray(), 2);

                var commandString = cmdParts[0].ToLower();
                var argumentString = cmdParts.Length > 1 ? cmdParts[1] : null;

                if (commandString == "exit")
                {
                    // Stop listening
                    break;
                }
                else if (commandString == "help")
                {
                    // List available commands
                    Console.WriteLine(Properties.Resources.Console_AvailableCommands);
                    foreach (var item in AllCommands.Keys.OrderBy(x => x))
                    {
                        Console.WriteLine(item);
                    }
                }
                else if (AllCommands.Keys.Contains(commandString, StringComparer.CurrentCultureIgnoreCase))
                {
                    await RunCommand(commandString, argumentString, true).ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine(string.Format(Properties.Resources.Console_CommandNotFound, commandString));
                }
            }
        }

        /// <summary>
        /// Runs the command with the given name using the given arguments.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="argumentString">String containing the arguments of the command, separated by spaces.  Use quotation marks to include spaces in a parameter.</param>
        /// <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        public async Task RunCommand(string commandName, string argumentString, bool reportErrorsToConsole = false)
        {
            // Split arg on spaces, while respecting quotation marks
            var args = new List<string>();
            if (argumentString != null)
            {
                foreach (Match item in ParameterRegex.Matches(argumentString))
                {
                    args.Add(item.Value.Trim('\"'));
                }
            }

            // Run the command
            await RunCommand(commandName, args, reportErrorsToConsole).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the command with the given name using the given arguments.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="arguments">Arguments of the command.</param>
        /// <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        public async Task RunCommand(string commandName, IEnumerable<string> arguments, bool reportErrorsToConsole = false)
        {
            try
            {
                var command = AllCommands.Where(c => String.Compare(c.Key, commandName, StringComparison.CurrentCultureIgnoreCase) == 0).Select(c => c.Value).SingleOrDefault();
                await command.MainAsync(arguments.ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (reportErrorsToConsole)
                {
                    Console.WriteLine(ex.ToString());
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
