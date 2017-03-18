using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Extensions
{
    /// <summary>
    /// The metadata of an extension
    /// </summary>
    public class ExtensionInfo
    {

        /// <summary>
        /// Deserializes the serialized extension info
        /// </summary>
        /// <param name="serialized">The serialized Json representation of the extension info</param>
        /// <returns>A new instance of <see cref="ExtensionInfo"/> representing the serialized data</returns>
        public static ExtensionInfo Deserialize(string serialized)
        {
            return Json.Deserialize<ExtensionInfo>(serialized);
        }

        /// <summary>
        /// Loads the extension info from the given file
        /// </summary>
        /// <param name="filename">Path of the extension file</param>
        /// <param name="provider">I/O provider from which to load the file</param>
        /// <returns>A new instance of <see cref="ExtensionInfo"/> representing the given file</returns>
        public static ExtensionInfo OpenFromFile(string filename, IIOProvider provider)
        {
            var output = Json.DeserializeFromFile<ExtensionInfo>(filename, provider);
            output.Filename = filename;
            return output;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ExtensionInfo"/>
        /// </summary>
        public ExtensionInfo()
        {
            ID = Guid.NewGuid().ToString(); //Ensures it's unique
            ExtensionFiles = new List<string>();
            Name = "";
            Description = "";
            Author = "";
            Version = "";
            IsEnabled = true;
        }
        /// <summary>
        /// A string representation of the type of the extension
        /// </summary>
        public string ExtensionTypeName { get; set; }

        /// <summary>
        /// Unique ID of the extension
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// User-friendly name of the extension
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the extension
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Author of the extension
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Version of the extension.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Whether or not the extension is enabled for loading
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Paths of the files associated with the extension, relative to the extension directory
        /// </summary>
        public List<string> ExtensionFiles { get; set; }

        /// <summary>
        /// List of extension dependencies that must be satisfied before installing this extension
        /// </summary>
        public List<ExtensionDependency> Dependencies { get; set; }

        /// <summary>
        /// Whether or not the extension is installed
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Path of the extension info file
        /// </summary>
        /// <remarks>This is private because it should not be stored in the extension Json.</remarks>
        private string Filename { get; set; }

        /// <summary>
        /// Gets the path of the extension file
        /// </summary>
        public string GetFilename()
        {
            return Filename;
        }

        /// <summary>
        /// Saves the extension info to the given file
        /// </summary>
        /// <param name="filename">Path of the extension file</param>
        /// <param name="provider">I/O provider in which to save the file</param>
        public void Save(string filename, IIOProvider provider)
        {
            Json.SerializeToFile(filename, this, provider);
        }

    }
}
