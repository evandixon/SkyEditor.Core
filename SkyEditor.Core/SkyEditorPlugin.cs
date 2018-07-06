using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    public abstract class SkyEditorPlugin
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public abstract string PluginName { get; }

        /// <summary>
        /// Name of the person or group who authored the plugin
        /// </summary>
        public abstract string PluginAuthor { get; }

        /// <summary>
        /// The plugin's credits
        /// </summary>
        public abstract string Credits { get; }

        /// <summary>
        /// Registers types in the plugin manager and loads any required resources
        /// </summary>
        public virtual void Load(PluginManager manager)
        {
        }

        /// <summary>
        /// Unloads resources and disposes of things needing disposing.
        /// </summary>
        /// <param name="manager"></param>
        public virtual void Unload(PluginManager manager)
        {
        }

        /// <summary>
        /// Deletes temporary and user-specific files not required during distribution.
        /// </summary>
        /// <param name="manager"></param>
        public virtual void PrepareForDistribution(PluginManager manager)
        {
        }
    }
}
