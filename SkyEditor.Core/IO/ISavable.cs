using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Represents a class that supports saving
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Raised when the file or object is saved
        /// </summary>
        event EventHandler FileSaved;

        /// <summary>
        /// Saves the class
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task Save(IFileSystem provider);  
    }
}
