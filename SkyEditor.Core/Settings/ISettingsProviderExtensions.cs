using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core.Settings
{
    public static class ISettingsProviderExtensions
    {
        /// <summary>
        /// Determines whether or not Development Mode is enabled.
        /// </summary>
        /// <returns>A boolean indicating whether or not Development Mode is enabled.</returns>
        public static bool GetIsDevMode(this ISettingsProvider provider)
        {
            bool? output = null;

            var setting = provider.GetSetting(SettingNames.DevMode);
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
#if DEBUG
                return true;
#else
                return false;
#endif
            }
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
    }
}
