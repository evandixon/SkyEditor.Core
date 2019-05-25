using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions
{
    /// <summary>
    /// A collection of locally-installed extensions
    /// </summary>
    public abstract class LocalExtensionCollection : IExtensionCollection
    {
        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager CurrentPluginManager { get; set; }

        /// <summary>
        /// The internal name of the extension type used in paths.
        /// </summary>
        /// <returns></returns>
        public virtual string InternalName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Gets or sets the directory the ExtensionType stores extensions in.
        /// </summary>
        /// <returns></returns>
        public string RootExtensionDirectory
        {
            get
            {
                return CurrentPluginManager.ExtensionDirectory;
            }
        }

        /// <summary>
        /// The user-friendly name of the extension type.
        /// </summary>
        /// <returns></returns>
        public abstract Task<string> GetName();

        /// <summary>
        /// Gets the directory that stores a particular extension
        /// </summary>
        /// <param name="extensionID">ID of the extension</param>
        public virtual string GetExtensionDirectory(string extensionID)
        {
            return Path.Combine(RootExtensionDirectory, InternalName, extensionID);
        }

        /// <summary>
        /// Gets the installed extensions
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        public virtual IEnumerable<ExtensionInfo> GetExtensions(PluginManager manager)
        {
            List<ExtensionInfo> @out = new List<ExtensionInfo>();

            //Todo: cache this so paging works more efficiently
            if (manager.CurrentFileSystem.DirectoryExists(Path.Combine(RootExtensionDirectory, InternalName)))
            {
                foreach (var item in manager.CurrentFileSystem.GetDirectories(Path.Combine(RootExtensionDirectory, InternalName), true))
                {
                    if (manager.CurrentFileSystem.FileExists(Path.Combine(item, "info.skyext")))
                    {
                        var e = ExtensionInfo.OpenFromFile(Path.Combine(item, "info.skyext"), manager.CurrentFileSystem);
                        e.IsInstalled = true;
                        @out.Add(e);
                    }
                }
            }

            return @out;
        }

        /// <summary>
        /// Lists the extensions that are currently installed.
        /// </summary>
        public Task<IEnumerable<ExtensionInfo>> GetExtensions(int skip, int take, PluginManager manager)
        {
            return Task.FromResult(GetExtensions(manager).Skip(skip).Take(take));
        }

        /// <summary>
        /// Gets the number of installed extensions
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        public Task<int> GetExtensionCount(PluginManager manager)
        {
            return Task.FromResult(GetExtensions(manager).Count());
        }

        public Task<ExtensionInstallResult> InstallExtension(string extensionID, string version, PluginManager manager)
        {
            throw (new NotSupportedException(Properties.Resources.UI_ErrorLocalExtensionCollectionInstall));
        }

        /// <summary>
        /// Installs the extension that's stored in the given directory
        /// </summary>
        /// <param name="TempDir">Temporary directory that contains the extension's files</param>
        public virtual async Task<ExtensionInstallResult> InstallExtension(string extensionID, string TempDir)
        {
            await FileSystem.CopyDirectory(TempDir, GetExtensionDirectory(extensionID), CurrentPluginManager.CurrentFileSystem).ConfigureAwait(false);
            return ExtensionInstallResult.Success;
        }

        /// <summary>
        /// Uninstalls the given extension
        /// </summary>
        /// <param name="extensionID">ID of the extension to uninstall</param>
        public virtual Task<ExtensionUninstallResult> UninstallExtension(string extensionID, PluginManager manager)
        {
            CurrentPluginManager.CurrentFileSystem.DeleteDirectory(GetExtensionDirectory(extensionID));
            return Task.FromResult(ExtensionUninstallResult.Success);
        }

        /// <summary>
        /// Gets the child extensions
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The child collections</returns>
        public virtual Task<IEnumerable<IExtensionCollection>> GetChildCollections(PluginManager manager)
        {
            return Task.FromResult(Enumerable.Empty<IExtensionCollection>());
        }
    }
}
