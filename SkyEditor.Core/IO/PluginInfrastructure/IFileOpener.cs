using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// Opens a file into a supported type
    /// </summary>
    public interface IFileOpener : IBaseFileOpener
    {
        /// <summary>
        /// Creates an instance of fileType from the given filename
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <param name="filename">Full path of the file to open</param>
        /// <param name="provider">Instance of the current IO provider</param>
        /// <returns>An object representing the requested file</returns>
        Task<object> OpenFile(TypeInfo fileType, string filename, IFileSystem provider);
    }
}
