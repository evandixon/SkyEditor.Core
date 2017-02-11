using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions
{
    /// <summary>
    /// A collection of extensions
    /// </summary>
    public interface IExtensionCollection
    {
        /// <summary>
        /// Gets the name of the collection
        /// </summary>
        Task<string> GetName();

        /// <summary>
        /// Gets the child collections
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        Task<IEnumerable<IExtensionCollection>> GetChildCollections(PluginManager manager);

        /// <summary>
        /// Gets the metadata of the extensions in the collection
        /// </summary>
        /// <param name="skip">Used with paging.  How many extensions to skip before seleting data to return.</param>
        /// <param name="take">Used with paging.  How many extensions to select at a time (i.e. the page size).</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        Task<IEnumerable<ExtensionInfo>> GetExtensions(int skip, int take, PluginManager manager);

        /// <summary>
        /// The number of extensions in the collection
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        Task<int> GetExtensionCount(PluginManager manager);

        /// <summary>
        /// Installs the given extension
        /// </summary>
        /// <param name="extensionID">ID of the extension to install.  The extension must be contained in the current extension collection</param>
        /// <param name="version">Version of the extension to install</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The result of the install</returns>
        Task<ExtensionInstallResult> InstallExtension(string extensionID, string version, PluginManager manager);

        /// <summary>
        /// Uninstalls the given extension
        /// </summary>
        /// <param name="extensionID">ID of the extension to uninstall.  The extension must be contained in the current extension collection</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The result of the uninstall</returns>
        Task<ExtensionUninstallResult> UninstallExtension(string extensionID, PluginManager manager);
    }
}
