using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Extensions
{
    /// <summary>
    /// A dependency of an extension
    /// </summary>
    public class ExtensionDependency
    {
        /// <summary>
        /// ID of the extension
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Version of the extension
        /// </summary>
        public string Version { get; set; }
    }
}
