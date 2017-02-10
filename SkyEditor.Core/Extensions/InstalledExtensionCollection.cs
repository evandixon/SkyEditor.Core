using System.Threading.Tasks;
using SkyEditor;
using SkyEditor.Core.Extensions;
using System.Collections.Generic;
using SkyEditor.Core;
using System.Linq;
using System;

namespace SkyEditor.Core.Extensions
{
    public class InstalledExtensionCollection : IExtensionCollection
    {

        public Task<string> GetName()
        {
            return Task.FromResult(Properties.Resources.UI_InstalledExtension);
        }

        public Task<IEnumerable<IExtensionCollection>> GetChildCollections(PluginManager manager)
        {
            // manager.GetRegisteredObjects returns shared objects, and it is generally not recommended to return these.
            // Creating new instances takes time, LocalExtensionCollection doesn't store any important stateful information, and there really only needs to be one of each type at any one time
            // Therefore, shared instances are OK.
            return Task.FromResult(manager.GetRegisteredObjects<LocalExtensionCollection>().AsEnumerable<IExtensionCollection>());
        }

        public Task<int> GetExtensionCount(PluginManager manager)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<ExtensionInfo>> GetExtensions(int skip, int take, PluginManager manager)
        {
            return Task.FromResult(Enumerable.Empty<ExtensionInfo>());
        }

        public Task<ExtensionInstallResult> InstallExtension(string extensionID, string version, PluginManager manager)
        {
            throw (new NotSupportedException());
        }

        public Task<ExtensionUninstallResult> UninstallExtension(string extensionID, PluginManager manager)
        {
            throw (new NotSupportedException());
        }
    }

}