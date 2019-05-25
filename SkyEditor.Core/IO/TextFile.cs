using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    public class TextFile : ICreatableFile, IOpenableFile, ISavableAs, IDetectableFileType
    {
        public virtual string Filename { get; set; }

        public virtual string Name { get; set; }

        public virtual string Contents { get; set; }

        public event EventHandler FileSaved;

        public virtual void CreateFile(string name)
        {
            this.Name = name;
            this.Contents = string.Empty;
        }

        public virtual string GetDefaultExtension()
        {
            return "*.txt";
        }

        public virtual IEnumerable<string> GetSupportedExtensions()
        {
            return new string[] { "*.txt" };
        }

        public virtual Task OpenFile(string filename, IFileSystem provider)
        {
            this.Contents = provider.ReadAllText(filename);
            this.Filename = filename;
            this.Name = Path.GetFileName(filename);
            return Task.CompletedTask;
        }

        public virtual async Task Save(IFileSystem provider)
        {
            await Save(Filename, provider);
        }

        public virtual Task Save(string filename, IFileSystem provider)
        {
            provider.WriteAllText(filename, Contents);
            this.Filename = filename;
            FileSaved?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        public virtual async Task<bool> IsOfType(GenericFile file)
        {
            for (int i = 0;i < file.Length;i++)
            {
                if (await file.ReadAsync(i) == 0) // Ensure there's no null characters
                {
                    return false;
                }
            }
            return true;
        }
    }
}
