using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using SkyEditor.IO.FileSystem;
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
    public class ConsoleShell
    {
        /// <summary>
        /// Runs a console command with custom input and returns the output.
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <param name="appViewModel">Current application view model</param>
        /// <param name="arguments">Input arguments</param>
        /// <param name="stdIn">New-line separated standard input</param>
        /// <returns>New-line separated standard output</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="appViewModel"/> is null.</exception>
        public static async Task<string> TestConsoleCommand(ConsoleCommand command, ApplicationViewModel appViewModel, string[] arguments, string stdIn)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            if (appViewModel == null)
            {
                throw new ArgumentNullException(nameof(appViewModel));
            }

            if (arguments == null)
            {
                arguments = new string[] { };
            }
            if (stdIn == null)
            {
                stdIn = "";
            }

            var provider = new MemoryConsoleProvider();
            provider.StdIn.Append(stdIn);
            command.Console = provider;
            await command.MainAsync(arguments).ConfigureAwait(false);
            return provider.GetStdOut();
        }

        public ConsoleShell(ApplicationViewModel appViewModel, PluginManager manager, IConsoleProvider consoleProvider, IFileSystem FileSystem)
        {
            CurrentApplicationViewModel = appViewModel;
            CurrentPluginManager = manager;
            Console = consoleProvider;
            CurrentFileSystem = FileSystem;
            AllCommands = new Dictionary<string, ConsoleCommand>();
            foreach (ConsoleCommand item in manager.GetRegisteredObjects<ConsoleCommand>())
            {
                item.Console = Console;
                AllCommands.Add(item.CommandName, item);
            }
        }

        protected ApplicationViewModel CurrentApplicationViewModel { get; }
        protected PluginManager CurrentPluginManager { get; }
        protected IFileSystem CurrentFileSystem { get; }
        protected Dictionary<string, ConsoleCommand> AllCommands { get; set; }
        protected IConsoleProvider Console { get; set; }
        protected Regex ParameterRegex => new Regex("(\\\".*?\\\")|\\S+", RegexOptions.Compiled);

        /// <summary>
        /// Listens for user input from the console provider provided via the plugin manager from <see cref="ConsoleShell.New(PluginManager)"/> and handles commands accordingly.
        /// </summary>
        /// <returns></returns>
        public async Task RunConsole()
        {
            while (true)
            {
                // Write bash-style working directory
                Console.Write("~");
                Console.Write(CurrentFileSystem.WorkingDirectory);
                Console.Write(" $ ");

                // Accept input
                var line = Console.ReadLine();

                // Interpret input
                // - Exit if null
                if (line == null)
                {
                    break;
                }

                // - Break up the command into its parts
                var cmdParts = line.Split(" ".ToCharArray(), 2);
                var commandString = cmdParts[0].ToLower();

                // - Shell commands
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
                // - Other commands
                else if (AllCommands.Keys.Contains(commandString, StringComparer.CurrentCultureIgnoreCase))
                {
                    await RunCommand(commandString, line, true).ConfigureAwait(false);
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
        /// <param name="provider">The I/O provider to use with the command</param>
        /// <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        public async Task RunCommand(string commandName, string argumentString, bool reportErrorsToConsole = false, IFileSystem FileSystem = null)
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
            await RunCommand(commandName, args, reportErrorsToConsole, FileSystem).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the command with the given name using the given arguments.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="arguments">Arguments of the command.</param>
        /// <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        public async Task RunCommand(string commandName, IEnumerable<string> arguments, bool reportErrorsToConsole = false, IFileSystem FileSystem = null)
        {
            var command = AllCommands.Where(c => String.Compare(c.Key, commandName, StringComparison.CurrentCultureIgnoreCase) == 0).Select(c => c.Value).SingleOrDefault();
            if (FileSystem != null && CurrentPluginManager.CanCreateInstance(command.GetType()))
            {
                command = CurrentPluginManager.CreateNewInstance(command, FileSystem) as ConsoleCommand;
            }
            await RunCommand(command, arguments, reportErrorsToConsole);
        }

        /// <summary>
        /// Runs the given command with the given arguments.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="arguments">Arguments of the command.</param>
        /// <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        public async Task RunCommand(ConsoleCommand command, IEnumerable<string> arguments, bool reportErrorsToConsole = false)
        {
            try
            {
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
