using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    public abstract class SkyEditorPlugin
    {
        public SkyEditorPlugin()
        {
        }

        public abstract string PluginName { get; }

        public abstract string PluginAuthor { get; }
        public abstract string Credits { get; }
        public virtual void Load(PluginManager manager)
        {

        }
    }
}
