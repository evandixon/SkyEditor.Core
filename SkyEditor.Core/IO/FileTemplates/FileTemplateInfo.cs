using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO.FileTemplates
{
    /// <summary>
    /// The structure of an entry in the file template index.
    /// </summary>
    internal class FileTemplateInfo
    {
        /// <summary>
        /// Name of the template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Author of the template
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The path of the file relative to the current extension directory
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Definitions of the placeholders in the template.  Key: Name of the placeholder, Value: Name of the type of the placeholder
        /// </summary>
        public Dictionary<string, string> Placeholders { get; set; }
    }
}
