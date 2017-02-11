using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Finds a type that can represent a file, for use with a <see cref="IFileOpener"/>
    /// </summary>
    public interface IFileTypeDetector
    {
        /// <summary>
        /// Finds a type that can represent a file, for use with a <see cref="IFileOpener"/>
        /// </summary>
        /// <param name="file">File for which to detect the type</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>Returns a type that can represent a file along with the percent chance that it is the correct type</returns>
        Task<IEnumerable<FileTypeDetectionResult>> DetectFileType(GenericFile file, PluginManager manager);
    }
}
