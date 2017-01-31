using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands
{
    /// <summary>
    /// Command that can be run in the Sky Editor console
    /// </summary>
    public abstract class ConsoleCommand
    {
        public virtual PluginManager CurrentPluginManager { get; set; }
        public virtual IConsoleProvider Console { get; set; }
        public virtual string CommandName
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// The main method of the command.
        /// </summary>
        /// <remarks>The default implementation calls <see cref="Main(string[])"/>.  Overriding this method is preferred, but if no async operations are done, <see cref="Main(string[])"/> is available to avoid needing to deal with async syntax.</remarks>
        public virtual Task MainAsync(string[] arguments)
        {
            Main(arguments);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The main method of the command.
        /// </summary>
        /// <remarks><see cref="MainAsync(string[])"/> calls this method if not already overridden.</remarks>
        protected virtual void Main(string[] arguments)
        {
        }
    }
}
