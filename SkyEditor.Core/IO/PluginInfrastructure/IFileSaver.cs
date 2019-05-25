using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// Saves a model to a file
    /// </summary>
    public interface IFileSaver
    {
        /// <summary>
        /// Determines whether or not saving the given model without a filename is supported
        /// </summary>
        /// <param name="model">The model to save</param>
        /// <returns>A boolean indicating whether or not saving the given model without a filename is supported</returns>
        bool SupportsSave(object model);

        /// <summary>
        /// Determines whether or not saving the given model without a filename is supported
        /// </summary>
        /// <param name="model">The model to save</param>
        /// <returns>A boolean indicating whether or not saving the given model without a filename is supported</returns>
        bool SupportsSaveAs(object model);

        /// <summary>
        /// Saves the model to disk
        /// </summary>
        /// <param name="model">Model to save</param>
        /// <param name="provider">The IO provider to use</param>
        Task Save(object model, IFileSystem provider);

        /// <summary>
        /// Saves the model to a file at the given path
        /// </summary>
        /// <param name="model">Model to save</param>
        /// <param name="filename">Path of the file to which the model will be saved</param>
        /// <param name="provider">The IO provider to use</param>
        Task Save(object model, string filename, IFileSystem provider);

        /// <summary>
        /// Gets the default extension for the given model when using Save As.
        /// Should only be called if the <see cref="SupportsSaveAs(Object)"/> returns true.
        /// </summary>
        /// <param name="model">Model of which to determine the default extension</param>
        /// <returns>A string representing the default extension, or null if <see cref="SupportsSaveAs(Object)"/> returns false for <paramref name="model"/></returns>
        string GetDefaultExtension(object model);

        /// <summary>
        /// Gets the supported extensions for the given model when using Save As.
        /// Should only be called if the <see cref="SupportsSaveAs(Object)"/> returns true.
        /// </summary>
        /// <param name="model">Model of which to determine the supported extensions</param>
        /// <returns>An IEnumerable that contains every extension that can be used to save this file, or null if <see cref="SupportsSaveAs(Object)"/> returns false for <paramref name="model"/></returns>
        IEnumerable<string> GetSupportedExtensions(object model);
    }
}
