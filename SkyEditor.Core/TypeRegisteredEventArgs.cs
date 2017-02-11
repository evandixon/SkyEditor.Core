using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    /// <summary>
    /// Event arguments for when <see cref="PluginManager"/> registers a type in the type registry.
    /// </summary>
    public class TypeRegisteredEventArgs
    {
        /// <summary>
        /// The registry in which the type was registered
        /// </summary>
        public TypeInfo BaseType { get; set; }

        /// <summary>
        /// The type that was registered
        /// </summary>
        public TypeInfo RegisteredType { get; set; }
    }
}
