using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    public class SettingsProvider : ISettingsProvider, INotifyModified
    {

        #region Child Classes
        protected class SerializedSettings
        {
            public SerializedSettings()
            {
                Settings = new Dictionary<string, SerializedValue>();
            }
            public Dictionary<string, SerializedValue> Settings { get; set; }
        }

        protected class SerializedValue
        {
            public string Value { get; set; }
            public string TypeName { get; set; }
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Deserializes the given data into a settings provider
        /// </summary>
        /// <param name="data">Data to deserialize.  If null, a new provider will be created.</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>A settings provider corresponding to the given data</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public static SettingsProvider Deserialize(string data, PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var provider = new SettingsProvider(manager);
            SerializedSettings s;

            if (string.IsNullOrEmpty(data)) {
                s = new SerializedSettings();
            }
            else
            {
                s = Json.Deserialize<SerializedSettings>(data);
            }
            
            foreach (var item in s.Settings)
            {
                var valueType = ReflectionHelpers.GetTypeByName(item.Value.TypeName, manager);
                if (valueType == null)
                {
                    // If the type cannot be loaded, then it's possible that the PluginManager hasn't fully loaded everything yet.
                    // Store the serialized value and try this part again if anyone requests the property.
                    provider.UnloadableSettings[item.Key] = item.Value;
                }
                else
                {
                    provider.Settings[item.Key] = Json.Deserialize(valueType.AsType(), item.Value.Value);
                }
            }

            return provider;
        }

        /// <summary>
        /// Opens a settings provider stored at the given filename or creates a new one if the file does not exist
        /// </summary>
        /// <param name="filename">File from which to load the settings</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>A settings provider corresponding to the given file</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public static SettingsProvider Open(string filename, PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (manager.CurrentIOProvider.FileExists(filename))
            {
                var provider = Deserialize(manager.CurrentIOProvider.ReadAllText(filename), manager);
                provider.Filename = filename;
                return provider;
            }
            else
            {
                var provider = new SettingsProvider(manager);
                provider.Filename = filename;
                return provider;
            }
        }

        #endregion

        public SettingsProvider(PluginManager manager)
        {
            Settings = new ConcurrentDictionary<string, object>();
            UnloadableSettings = new ConcurrentDictionary<string, SerializedValue>();
            CurrentPluginManager = manager;
        }

        public event EventHandler FileSaved;
        public event EventHandler Modified;

        public string Filename { get; set; }
        protected ConcurrentDictionary<string,object> Settings { get; set; }
        protected ConcurrentDictionary<string, SerializedValue> UnloadableSettings { get; set; }        
        protected PluginManager CurrentPluginManager { get; set; }

        /// <summary>
        /// Gets the value of the setting with the given name
        /// </summary>
        /// <param name="name">Name of the setting.</param>
        /// <returns>The value of the setting with the given name, or null if the setting does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public object GetSetting(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (Settings.ContainsKey(name))
            {
                return Settings[name];
            }
            else
            {
                // If the setting doesn't exist, check to see if it originally couldn't be loaded
                if (UnloadableSettings.ContainsKey(name))
                {
                    // If it couldn't be loaded, try again
                    var valueType = ReflectionHelpers.GetTypeByName(UnloadableSettings[name].TypeName, CurrentPluginManager);
                    if (valueType == null)
                    {
                        // Still can't be loaded.  Return null as if we don't have it.
                        return null;
                    }
                    else
                    {
                        // It was loaded.  Add to main settings.
                        Settings[name] = Json.Deserialize(valueType.AsType(), UnloadableSettings[name].Value);

                        // Remove from unloadable settings
                        // If it fails, no harm done, as it will be ignored later.
                        SerializedValue dummy;
                        UnloadableSettings.TryRemove(name, out dummy);

                        return Settings[name];
                    }
                }
                else
                {
                    // Setting doesn't exist
                    return null;
                }
            }
        }

        /// <summary>
        /// Sets a setting.
        /// </summary>
        /// <param name="name">Name of the setting.</param>
        /// <param name="value">Value of the setting.  Must be serializable to JSON.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public void SetSetting(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Settings[name] = value;
            Modified?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Serializes the current settings
        /// </summary>
        public string Serialize()
        {
            var s = new SerializedSettings();

            // Save the settings
            foreach (var item in Settings)
            {
                s.Settings.Add(item.Key,
                    new SerializedValue
                    {
                        TypeName = item.Value.GetType().AssemblyQualifiedName,
                        Value = Json.Serialize(item.Value)
                    });
            }

            // Save the settings that couldn't be opened
            foreach (var item in UnloadableSettings)
            {
                // Make sure there isn't a duplicate
                // Cases there could be a duplicate:
                // 1. A setting with the same name was set without a successful load beforehand
                // 2. A setting was loaded properly, but not removed from UnloadableSettings due to threading

                if (!s.Settings.ContainsKey(item.Key))
                {
                    // This isn't a duplicate
                    s.Settings.Add(item.Key, item.Value);
                }
            }

            return Json.Serialize(s);
        }

        /// <summary>
        /// Saves the settings to the original settings file
        /// </summary>
        public Task Save(IIOProvider provider)
        {
            provider.WriteAllText(Filename, Serialize());
            return Task.CompletedTask;
        }
    }
}
