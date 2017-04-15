using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Extensions for IIOProvider implementations
    /// </summary>
    public static class IIOProviderExtensions
    {
        /// <summary>
        /// Creates a directory if it does not exist, doing nothing if the directory does exist
        /// </summary>
        /// <param name="provider">The current I/O provider</param>
        /// <param name="path">Path of the directory to create</param>
        public static void CreateDirectoryIfNotExists(this IIOProvider provider, string path)
        {
            if (!provider.DirectoryExists(path))
            {
                provider.CreateDirectory(path);
            }
        }
    }
}
