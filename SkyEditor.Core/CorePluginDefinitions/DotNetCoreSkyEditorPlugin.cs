using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.CorePluginDefinitions
{
    /// <summary>
    /// The base class of a plugin that runs on .Net Core
    /// </summary>
    public abstract class DotNetCoreSkyEditorPlugin : CoreSkyEditorPlugin
    {
        public override Assembly LoadAssembly(string assemblyPath)
        {
            if (!Path.IsPathRooted(assemblyPath))
            {
                assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), assemblyPath);
            }
            
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }
    }
}
