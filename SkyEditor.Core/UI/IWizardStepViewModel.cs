using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Represents a view model for a wizard step
    /// </summary>
    /// <remarks>It is up to the main application and the appropriate plugin to supply a user interface for this view model</remarks>
    public interface IWizardStepViewModel : INamed
    {

        /// <summary>
        /// Whether or not the current step is complete and proceeding is allowed
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Gets a console command (<see cref="ConsoleCommand"/>) that can be used to run this wizard step in the console.
        /// </summary>
        /// <remarks>
        /// This command will be run without any arguments. The wizard step should be complete following execution. If this returns null, then this workflow step will be skipped.
        /// </remarks>
        ConsoleCommand GetConsoleCommand();
    }
}
