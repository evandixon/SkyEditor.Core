using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Represents a class that can save itself to disk at a specific filename
    /// </summary>
    public interface ISavableAs : ISavable
    {
        /// <summary>
        /// Saves to the given filename
        /// </summary>
        /// <param name="filename">Path of the destination file</param>
        /// <param name="provider">Instance of the current I/O provider</param>
        Task Save(string filename, IFileSystem provider);

        /// <summary>
        /// Gets the default extension for the file
        /// </summary>
        /// <returns>The default extension of the file</returns>
        string GetDefaultExtension();

        /// <summary>
        /// Gets the supported extensions for the file
        /// </summary>
        /// <returns>The supported extensions that can be used to save the file</returns>
        IEnumerable<string> GetSupportedExtensions();
    }
}
