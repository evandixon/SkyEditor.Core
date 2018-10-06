using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// The result of a file type detection
    /// </summary>
    public class FileTypeDetectionResult
    {
        /// <summary>
        /// The type of the class that can model the file in question
        /// </summary>
        public TypeInfo FileType { get; set; }

        /// <summary>
        /// The percentage chance that this file type can model the file in question
        /// </summary>
        /// <remarks>The FileTypeDetectionResult with the highest MatchChance will be used, and if there are duplicates (e.g. two FileTypeDetectionResult instances with a chance of 1.0), the user may be prompted.</remarks>
        public float MatchChance { get; set; }
    }
}
