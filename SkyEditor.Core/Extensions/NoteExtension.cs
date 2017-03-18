using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions
{
    /// <summary>
    /// An extension designed for testing purposes that stores small text snippets readable with a console command
    /// </summary>
    public class NoteExtension : LocalExtensionCollection
    {
        public override string InternalName => "Notes";

        private Dictionary<string, string> ExtensionCache { get; set; }

        public override Task<string> GetName()
        {
            return Task.FromResult(InternalName);
        }

        private void ResetCache()
        {
            ExtensionCache = null;
        }

        private void LoadCache(PluginManager manager)
        {
            ExtensionCache = new Dictionary<string, string>();
            foreach (var ext in GetExtensions(manager))
            {
                foreach (var file in ext.ExtensionFiles)
                {
                    var name = Path.GetFileName(file);
                    var path = Path.Combine(GetExtensionDirectory(ext.ID), file.TrimStart('/'));
                    if (!ExtensionCache.ContainsKey(name) && File.Exists(path))
                    {
                        ExtensionCache.Add(name, File.ReadAllText(path));
                    }
                }
            }
        }

        public Dictionary<string, string> GetNotes(PluginManager manager)
        {
            if (ExtensionCache == null)
            {
                LoadCache(manager);
            }
            return ExtensionCache;
        }

        public override async Task<ExtensionInstallResult> InstallExtension(string extensionID, string TempDir)
        {
            ResetCache();
            return await base.InstallExtension(extensionID, TempDir);
        }

        public override async Task<ExtensionUninstallResult> UninstallExtension(string extensionID, PluginManager manager)
        {
            ResetCache();
            return await base.UninstallExtension(extensionID, manager);
        }
    }
}
