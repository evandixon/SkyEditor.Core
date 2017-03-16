using SkyEditor.Core.Settings;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions
{
    public class PluginExtensionType : LocalExtensionCollection
    {

        public override Task<string> GetName()
        {
            return Task.FromResult("Plugins");
        }

        public override string InternalName
        {
            get
            {
                return "Plugins";
            }
        }

        public override async Task<ExtensionInstallResult> InstallExtension(string extensionID, string TempDir)
        {
            await base.InstallExtension(extensionID, TempDir).ConfigureAwait(false);
            return ExtensionInstallResult.RestartRequired;
        }

        /// <summary>
        /// Gets the directory that stores the files for the given extension ID.
        /// </summary>
        /// <param name="extensionID">ID of the extension for which to get the directory.</param>
        /// <returns></returns>
        /// <remarks>If the extensionID is an empty Guid, returns the plugin development directory.</remarks>
        public override string GetExtensionDirectory(string extensionID)
        {
            if (extensionID == Guid.Empty.ToString())
            {
                return GetDevDirectory();
            }
            else
            {
                return base.GetExtensionDirectory(extensionID);
            }
        }

        /// <summary>
        /// Gets the plugin development directory.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDevDirectory()
        {
            return Path.Combine(RootExtensionDirectory, InternalName, "Development");
        }

        public override IEnumerable<ExtensionInfo> GetExtensions(PluginManager manager)
        {
            List<ExtensionInfo> extensions = new List<ExtensionInfo>();
            extensions.AddRange(base.GetExtensions(manager));
            if (manager.CurrentSettingsProvider.GetIsDevMode())
            {
                //Load the development plugins
                var devDir = GetDevDirectory();
                ExtensionInfo info = new ExtensionInfo();
                info.ID = Guid.Empty.ToString();
                info.Name = Properties.Resources.PluginDevExtName;
                info.Description = Properties.Resources.PluginDevExtDescription;
                info.Author = Properties.Resources.PluginDevExtAuthor;
                info.IsInstalled = true;
                info.IsEnabled = true;
                info.Version = Properties.Resources.PluginDevExtVersion;
                
                if (manager.CurrentIOProvider.DirectoryExists(devDir))
                {
                    foreach (var item in manager.CurrentIOProvider.GetFiles(devDir, "*.dll", true))
                    {
                        info.ExtensionFiles.Add(Path.GetFileName(item));
                    }
                    foreach (var item in manager.CurrentIOProvider.GetFiles(devDir, "*.exe", true))
                    {
                        info.ExtensionFiles.Add(Path.GetFileName(item));
                    }

                    if (info.ExtensionFiles.Count == 0)
                    {
                        // Look in each subdirectory if the root was empty
                        foreach (var dir in manager.CurrentIOProvider.GetDirectories(devDir, true))
                        {
                            foreach (var item in manager.CurrentIOProvider.GetFiles(dir, "*.dll", true))
                            {
                                info.ExtensionFiles.Add(dir + "/" + Path.GetFileName(item));
                            }
                            foreach (var item in manager.CurrentIOProvider.GetFiles(dir, "*.exe", true))
                            {
                                info.ExtensionFiles.Add(dir + "/" + Path.GetFileName(item));
                            }
                        }
                    }
                }
                extensions.Add(info);
            }
            return extensions;
        }

        /// <summary>
        /// Uninstalls the given extension.
        /// </summary>
        /// <param name="extensionID">ID of the extension to uninstall</param>
        public override Task<ExtensionUninstallResult> UninstallExtension(string extensionID, PluginManager manager)
        {
            CurrentPluginManager.CurrentSettingsProvider.ScheduleDirectoryForDeletion(GetExtensionDirectory(extensionID));
            return Task.FromResult(ExtensionUninstallResult.RestartRequired);
        }

        public override Task<IEnumerable<IExtensionCollection>> GetChildCollections(PluginManager manager)
        {
            return Task.FromResult(Enumerable.Empty<IExtensionCollection>());
        }

    }
}
