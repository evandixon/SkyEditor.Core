using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Opens a file into a supported type
    /// </summary>
    public interface IFileOpener
    {
        /// <summary>
        /// Creates an instance of fileType from the given filename
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <param name="filename">Full path of the file to open</param>
        /// <param name="provider">Instance of the current IO provider</param>
        /// <returns>An object representing the requested file</returns>
        Task<object> OpenFile(TypeInfo fileType, string filename, IIOProvider provider);

        /// <summary>
        /// Determines whether or not the IFileOpener supports opening a file of the given type
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <returns>A boolean indicating whether or not the given file type is supported</returns>
        bool SupportsType(TypeInfo fileType);

        /// <summary>
        /// Gets the priority of this <see cref="IFileOpener"/> to be used for the given type
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <returns>An integer indicating the usage priority for the given file type.  Higher numbers give higher priority.</returns>
        int GetUsagePriority(TypeInfo fileType);
    }
}
