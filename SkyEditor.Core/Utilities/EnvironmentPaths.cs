using SkyEditor.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Determines various file paths
    /// </summary>
    public static class EnvironmentPaths
    {
        /// <summary>
        /// Gets the root resource directory, where plugins, settings, etc. are stored.
        /// </summary>
        /// <param name="throwIfCreateFails">Whether or not to throw an exception if the directory cannot be created</param>
        /// <returns>The path of the root resource directory</returns>
        public static string GetRootResourceDirectory(bool throwIfCreateFails = false)
        {
#if NET462
            // Store things in local app data
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            }
#endif
            // Store things in working directory
            var resourcesDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
            if (!Directory.Exists(resourcesDir))
            {
                try
                {
                    Directory.CreateDirectory(resourcesDir);
                }
                catch (UnauthorizedAccessException)
                {
                    if (throwIfCreateFails)
                    {
                        throw;
                    }                    
                }
            }
            return resourcesDir;
        }

        /// <summary>
        /// Gets the path of the settings file
        /// </summary>
        public static string GetSettingsFilename()
        {
            return Path.Combine(GetRootResourceDirectory(), "settings.json");
        }

        /// <summary>
        /// Gets directory in which extensions are stored
        /// </summary>
        public static string GetExtensionsDirectory()
        {
            return Path.Combine(GetRootResourceDirectory(), "Extensions");
        }

        /// <summary>
        /// Gets the Plugins extension directory
        /// </summary>
        public static string GetPluginsExtensionDirectory()
        {
            return Path.Combine(GetExtensionsDirectory(), PluginExtensionType.PluginsInternalName);
        }

#if NET462

        /// <summary>
        /// Gets the directory for a plugin with the given assembly name
        /// </summary>
        [Obsolete] public static string GetResourceDirectory(string assemblyName, bool throwIfCreateFails = false)
        {
            var devDir = Path.Combine(GetRootResourceDirectory(), "Extensions", "Plugins", "Development", assemblyName);
            var pluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), assemblyName);
            if (Directory.Exists(devDir))
            {
                return devDir;
            }
            else if (Directory.Exists(pluginDirectory))
            {
                return pluginDirectory;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(devDir);
                }
                catch (Exception)
                {
                    if (throwIfCreateFails)
                    {
                        throw;
                    }
                }
                return devDir;
            }
        }

        /// <summary>
        /// Returns the calling plugin's resource directory
        /// </summary>
        /// <returns></returns>
        [Obsolete] public static string GetResourceDirectory()
        {
            return GetResourceDirectory(Assembly.GetCallingAssembly().GetName().Name);
        }

        [Obsolete] public static string GetResourceName(string resourcePath, string pluginName, bool throwIfCreateFails = false)
        {
            var fullPath = Path.Combine(GetResourceDirectory(pluginName), resourcePath);
            var baseDir = Path.GetDirectoryName(fullPath);
            try
            {
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }
            }
            catch (Exception)
            {
                if (throwIfCreateFails)
                {
                    throw;
                }
            }
            return fullPath;
        }

        /// <summary>
        /// Combine's the given path with the calling plugin's resource directory
        /// </summary>
        [Obsolete] public static string GetResourceName(string path)
        {
            return GetResourceName(path, Assembly.GetCallingAssembly().GetName().Name);
        }
#endif
    }
}
