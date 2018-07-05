using SkyEditor.Core.IO;
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
        public ConsoleCommand(IIOProvider ioProvider)
        {
            _defaultIOProvider = ioProvider;
        }
        
        public virtual IConsoleProvider Console { get; set; }
        public virtual string CommandName
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// The current I/O provider for the command.  May be different from the one used by <see cref="ApplicationViewModel"/>.
        /// </summary>
        /// <remarks>This can be used to run a command against a different I/O provider.  Set to null to reset to the default provider.</remarks>
        public IIOProvider CurrentIOProvider
        {
            get
            {
                if (_currentIOProvider == null)
                {
                    return _defaultIOProvider;
                }
                else
                {
                    return _currentIOProvider;
                }
            }
            set
            {
                _currentIOProvider = value;
            }
        }
        private IIOProvider _defaultIOProvider;
        private IIOProvider _currentIOProvider;

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
