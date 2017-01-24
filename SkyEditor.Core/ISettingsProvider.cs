using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    /// <summary>
    /// Represents a class that can get and set values of settings
    /// </summary>
    public interface ISettingsProvider : ISavable
    {
        /// <summary>
        /// Gets the value of a setting
        /// </summary>
        /// <param name="name">Name of the desired setting</param>
        /// <returns>The value of the setting with the given name or null if the setting does not have a value</returns>
        object GetSetting(string name);

        /// <summary>
        /// Sets a setting
        /// </summary>
        /// <param name="name">Name of the desired setting</param>
        /// <param name="value">New value of the setting</param>
        void SetSetting(string name, object value);
    }
}
