using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions
{
    public class ExtensionHelper : INamed
    {

        static ExtensionHelper()
        {
            ExtensionBanks = new Dictionary<string, LocalExtensionCollection>();
        }

        public string Name
        {
            get
            {
                return "Extensions";
            }
        }

        private static Dictionary<string, LocalExtensionCollection> ExtensionBanks { get; set; }

        /// <summary>
        /// Gets the <see cref="LocalExtensionCollection"/> with the given type name
        /// </summary>
        /// <param name="extensionTypeName">Name of the type of the extension bank</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An instance of <see cref="ExtensionType"/> corresponsing to <paramref name="extensionTypeName"/>, or null if it cannot be found</returns>
        public static LocalExtensionCollection GetExtensionBank(string extensionTypeName, PluginManager manager)
        {
            if (!ExtensionBanks.ContainsKey(extensionTypeName))
            {
                var extensionType = ReflectionHelpers.GetTypeByName(extensionTypeName, manager);
                if (extensionType != null)
                {
                    var bank = manager.CreateInstance(extensionType) as LocalExtensionCollection;
                    bank.CurrentPluginManager = manager;
                    ExtensionBanks.Add(extensionTypeName, bank);
                }
                else
                {
                    return null;
                }
            }
            return ExtensionBanks[extensionTypeName];
        }

        /// <summary>
        /// Gets the <see cref="LocalExtensionCollection"/> with the given type name
        /// </summary>
        /// <typeparam name="T">Type of the extension bank</typeparam>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An instance of <see cref="ExtensionType"/> corresponsing to <paramref name="extensionTypeName"/>, or null if it cannot be found</returns>
        public static T GetExtensionBank<T>(PluginManager manager) where T : LocalExtensionCollection
        {
            var extensionTypeName = typeof(T).AssemblyQualifiedName;
            if (!ExtensionBanks.ContainsKey(extensionTypeName))
            {
                var extensionType = typeof(T);
                var bank = manager.CreateInstance(extensionType) as LocalExtensionCollection;
                bank.CurrentPluginManager = manager;
                ExtensionBanks.Add(extensionTypeName, bank);
            }
            return ExtensionBanks[extensionTypeName] as T;
        }

        /// <summary>
        /// Determines whether or not the extension is installed
        /// </summary>
        /// <param name="info">The extension to check</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        public static bool IsExtensionInstalled(ExtensionInfo info, PluginManager manager)
        {
            var extensionType = ReflectionHelpers.GetTypeByName(info.ExtensionTypeName, manager);
            if (extensionType == null)
            {
                return false;
            }
            else
            {
                var bank = GetExtensionBank(info.ExtensionTypeName, manager);
                return bank.GetExtensions(manager).Where(x => x.ID == info.ID).Any();
            }
        }

        /// <summary>
        /// Installs all extension zips at the root of the extension directory.  Should be run before plugins are loaded.
        /// </summary>
        /// <param name="extensionDirectory">Directory in which extensions are stored</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        public static async Task InstallPendingExtensions(string extensionDirectory, PluginManager manager)
        {
            if (manager.CurrentIOProvider.DirectoryExists(extensionDirectory))
            {
                foreach (var item in manager.CurrentIOProvider.GetFiles(extensionDirectory, "*.zip", true))
                {
                    var result = await InstallExtensionZip(item, manager);
                    if (result == ExtensionInstallResult.Success || result == ExtensionInstallResult.RestartRequired)
                    {
                        manager.CurrentIOProvider.DeleteFile(item);
                    }
                }
            }
        }

        /// <summary>
        /// Installs the zip file.
        /// </summary>
        /// <param name="extensionZipPath">Path of the extension file to install</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The result of the install</returns>
        public static async Task<ExtensionInstallResult> InstallExtensionZip(string extensionZipPath, PluginManager manager)
        {
            var provider = manager.CurrentIOProvider;
            ExtensionInstallResult result;

            //Get the temporary directory
            var tempDir = provider.GetTempDirectory();

            //Ensure it contains no files
            await FileSystem.EnsureDirectoryExistsEmpty(tempDir, provider).ConfigureAwait(false);

            //Extract the given zip file to it
            await Zip.UnzipDir(extensionZipPath, tempDir, provider);

            //Open the info file
            string infoFilename = Path.Combine(tempDir, "info.skyext");
            if (provider.FileExists(infoFilename))
            {
                //Open the file itself
                var info = ExtensionInfo.OpenFromFile(infoFilename, provider);
                //Get the type
                var extType = GetExtensionBank(info.ExtensionTypeName, manager);
                //Determine if the type is supported
                if (ReferenceEquals(extType, null))
                {
                    result = ExtensionInstallResult.UnsupportedFormat;
                }
                else
                {
                    result = await extType.InstallExtension(info.ID, tempDir).ConfigureAwait(false);
                }
            }
            else
            {
                result = ExtensionInstallResult.InvalidFormat;
            }

            // Cleanup
            provider.DeleteDirectory(tempDir);

            return result;
        }

        public static async Task<ExtensionUninstallResult> UninstallExtension(string extensionTypeName, string extensionID, PluginManager manager)
        {
            var bank = GetExtensionBank(extensionTypeName, manager);
            return await bank.UninstallExtension(extensionID, manager).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<ExtensionInfo>> GetExtensions(string extensionTypeName, int skip, int take, PluginManager manager)
        {
            var bank = GetExtensionBank(extensionTypeName, manager);
            return await bank.GetExtensions(skip, take, manager);
        }
    }
}
