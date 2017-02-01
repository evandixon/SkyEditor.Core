using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Represents an object that can report its progress
    /// </summary>
    public interface IReportProgress
    {
        event EventHandler<ProgressReportedEventArgs> ProgressChanged;
        event EventHandler Completed;

        /// <summary>
        /// A percentage representing the current progress of the operation
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// A user-readable string identifying what the operation is doing
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Whether or not progress can be somewhat-accurately determined
        /// </summary>
        bool IsIndeterminate { get; }

        /// <summary>
        /// Whether or not the operation has completed
        /// </summary>
        bool IsCompleted { get; }
    }
}
