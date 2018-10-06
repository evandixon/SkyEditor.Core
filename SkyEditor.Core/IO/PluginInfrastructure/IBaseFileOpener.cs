using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    public interface IBaseFileOpener
    {
        /// <summary>
        /// Determines whether or not the file opener supports opening a file of the given type
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <returns>A boolean indicating whether or not the given file type is supported</returns>
        bool SupportsType(TypeInfo fileType);

        /// <summary>
        /// Gets the priority of this file opener to be used for the given type
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <returns>An integer indicating the usage priority for the given file type.  Higher numbers give higher priority.</returns>
        int GetUsagePriority(TypeInfo fileType);
    }
}
