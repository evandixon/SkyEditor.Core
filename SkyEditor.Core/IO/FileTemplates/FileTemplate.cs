using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO.FileTemplates
{
    /// <summary>
    /// A template for an openable file
    /// </summary>
    public class FileTemplate
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
        /// Full path of the file
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Definitions of the placeholders in the template.  Key: Name of the placeholder, Value: Value of the placeholder
        /// </summary>
        public Dictionary<string, object> Placeholders { get; set; }
    }
}
