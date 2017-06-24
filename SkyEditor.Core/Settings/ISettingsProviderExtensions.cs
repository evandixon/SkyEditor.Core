using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core.Settings
{
    public static class ISettingsProviderExtensions
    {
        private static bool GetBooleanSetting(ISettingsProvider provider, string settingName, bool defaultValue)
        {
            bool? output = null;

            var setting = provider.GetSetting(settingName);
            if (setting is bool?)
            {
                output = setting as bool?;
            }
            else if (setting is string)
            {
                bool parsed;
                if (bool.TryParse(setting as string, out parsed))
                {
                    output = parsed;
                }
            }

            if (output.HasValue)
            {
                return output.Value;
            }
            else
            {
                // Return default if not set
                return defaultValue;
            }
        }

        /// <summary>
        /// Determines whether or not Development Mode is enabled.
        /// </summary>
        /// <returns>A boolean indicating whether or not Development Mode is enabled.</returns>
        public static bool GetIsDevMode(this ISettingsProvider provider)
        {
#if DEBUG
            return GetBooleanSetting(provider, SettingNames.DevMode, true);
#else
            return GetBooleanSetting(provider, SettingNames.DevMode, false);
#endif
        }

        /// <summary>
        /// Sets whether or not Development Mode is enabled.
        /// </summary>
        /// <param name="value">Whether or not Development Mode is enabled.</param>
        public static void SetIsDevMode(this ISettingsProvider provider, bool value)
        {
            provider.SetSetting(SettingNames.DevMode, value);
        }

        /// <summary>
        /// Gets the full paths of all files that are scheduled for deletion.
        /// </summary>
        /// <returns>A list of string that contains paths of files scheduled for deletion.</returns>
        public static IList<string> GetFilesScheduledForDeletion(this ISettingsProvider provider)
        {
            var setting = provider.GetSetting(SettingNames.FilesForDeletion) as IList<string>;

            if (setting == null)
            {
                // Initialize the setting if it hasn't been set or if it's the wrong type
                setting = new List<string>();
            }

            return setting;
        }

        /// <summary>
        /// Schedules a file for deletion upon the next PluginManager load.
        /// </summary>
        /// <param name="path">Full path of the file to be deleted.</param>
        public static void ScheduleFileForDeletion(this ISettingsProvider provider, string path)
        {
            var settings = GetFilesScheduledForDeletion(provider);
            settings.Add(path);
            provider.SetSetting(SettingNames.FilesForDeletion, path);
        }

        /// <summary>
        /// Unschedules a file for deletion.
        /// </summary>
        /// <param name="path">Full path of the file to be unscheduled.</param>
        public static void UnscheduleFileForDeletion(this ISettingsProvider provider, string path)
        {
            var settings = GetFilesScheduledForDeletion(provider);
            settings.Remove(path);
            provider.SetSetting(SettingNames.FilesForDeletion, path);
        }

        /// <summary>
        /// Gets the full paths of all directories that are scheduled for deletion.
        /// </summary>
        /// <returns>A list of string that contains paths of directories scheduled for deletion.</returns>
        public static IList<string> GetDirectoriesScheduledForDeletion(this ISettingsProvider provider)
        {
            var setting = provider.GetSetting(SettingNames.DirectoriesForDeletion) as IList<string>;

            if (setting == null)
            {
                // Initialize the setting if it hasn't been set or if it's the wrong type
                setting = new List<string>();
            }

            return setting;
        }

        /// <summary>
        /// Schedules a directory for deletion upon the next PluginManager load.
        /// </summary>
        /// <param name="path">Full path of the directory to be deleted.</param>
        public static void ScheduleDirectoryForDeletion(this ISettingsProvider provider, string path)
        {
            var settings = GetFilesScheduledForDeletion(provider);
            settings.Add(path);
            provider.SetSetting(SettingNames.DirectoriesForDeletion, path);
        }

        /// <summary>
        /// Unschedules a directory for deletion.
        /// </summary>
        /// <param name="path">Full path of the directory to be unscheduled.</param>
        public static void UnscheduleDirectoryForDeletion(this ISettingsProvider provider, string path)
        {
            var settings = GetFilesScheduledForDeletion(provider);
            settings.Remove(path);
            provider.SetSetting(SettingNames.DirectoriesForDeletion, path);
        }

        /// <summary>
        /// Gets the endpoint URLs of the currently-configured online extension collections
        /// </summary>
        public static IList<string> GetExtensionCollections(this ISettingsProvider provider)
        {
            var setting = provider.GetSetting(SettingNames.OnlineExtensionCollections) as IList<string>;
            if (setting == null)
            {
                setting = new List<string>();
                setting.Add(BuildSettings.DefaultExtensionCollection);
                provider.SetSetting(SettingNames.OnlineExtensionCollections, setting);
            }
            return setting;
        }

        /// <summary>
        /// Sets the endpoint URLs of the currently-configured online extension collections
        /// </summary>
        /// <param name="value">IList containing the endpoint URLs of the extension collections</param>
        public static void SetExtensionCollections(this ISettingsProvider provider, IList<string> value)
        {
            provider.SetSetting(SettingNames.OnlineExtensionCollections, value);
        }

        /// <summary>
        /// Adds the given endpoint to the currently-configured online extension collections
        /// </summary>
        /// <param name="url">Endpoint of the online extension collection</param>
        public static void AddExtensionCollection(this ISettingsProvider provider, string url)
        {
            var collections = GetExtensionCollections(provider);
            collections.Add(url);
            SetExtensionCollections(provider, collections);
        }

        /// <summary>
        /// Removes the given endpoint from the currently-configured online extension collections
        /// </summary>
        /// <param name="url">Endpoint of the online extension collection</param>
        public static void RemoveExtensionCollection(this ISettingsProvider provider, string url)
        {
            var collections = GetExtensionCollections(provider);
            collections.Remove(url);
            SetExtensionCollections(provider, collections);
        }

        /// <summary>
        /// Gets whether or not to check for extension updates
        /// </summary>
        public static bool GetCheckExtensionUpdates(this ISettingsProvider provider)
        {
            return GetBooleanSetting(provider, SettingNames.CheckExtensionUpdates, true);
        }

        /// <summary>
        /// Sets whether or not to check for extension updates
        /// </summary>
        public static void SetCheckExtensionUpdates(this ISettingsProvider provider, bool value)
        {
            provider.SetSetting(SettingNames.CheckExtensionUpdates, value);
        }

        /// <summary>
        /// Gets whether or not to automatically update extensions
        /// </summary>
        public static bool GetAutoUpdateExtensions(this ISettingsProvider provider)
        {
            return GetBooleanSetting(provider, SettingNames.AutoUpdateExtensions, true);
        }

        /// <summary>
        /// Sets whether or not to automatically update extensions
        /// </summary>
        public static void SetAutoUpdateExtensions(this ISettingsProvider provider, bool value)
        {
            provider.SetSetting(SettingNames.AutoUpdateExtensions, value);
        }
    }
}
