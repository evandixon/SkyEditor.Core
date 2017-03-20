using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.Extensions;
using SkyEditor.Core.IO;
using System.IO;
using SkyEditor.Core.Utilities;

namespace SkyEditor.Core.Projects
{
    /// <summary>
    /// A project that can build <see cref="NoteExtension"/>
    /// </summary>
    public class NoteProject : Project
    {
        public override IEnumerable<TypeInfo> GetSupportedFileTypes(string path, PluginManager manager)
        {
            return new TypeInfo[] { typeof(TextFile).GetTypeInfo() };
        }

        public override bool CanCreateDirectory(string parentPath)
        {
            return false;
        }

        public string GetOutputDirectory()
        {
            return Path.Combine(GetRootDirectory(), "Output");
        }

        public override async Task Build()
        {
            var rootDir = GetRootDirectory();
            var outputDir = GetOutputDirectory();
            var provider = CurrentPluginManager.CurrentIOProvider;
            var info = new ExtensionInfo();
            info.Name = this.Name;
            info.ExtensionTypeName = typeof(NoteExtension).AssemblyQualifiedName;

            // Copy files to temp directory
            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), rootDir, "Output", "Temp");
            await FileSystem.EnsureDirectoryExistsEmpty(tempDir, provider);

            foreach (var file in GetItems("/", false))
            {
                provider.CopyFile(file.Value.Filename, Path.Combine(tempDir, Path.GetFileName(file.Value.Filename)));
                info.ExtensionFiles.Add(file.Key);
            }

            // Create extension file in temp directory
            info.Save(Path.Combine(tempDir, "info.skyext"), provider);

            // Zip the temp directory to create the extension
            await Zip.ZipDir(tempDir, Path.Combine(outputDir, this.Name + ".zip"), provider);

            await base.Build();
        }
    }
}
