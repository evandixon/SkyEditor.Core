using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    public class TextFile : ICreatableFile, IOpenableFile, ISavableAs, IDetectableFileType
    {
        public string Filename { get; set; }

        public string Name { get; set; }

        public string Contents { get; set; }

        public event EventHandler FileSaved;

        public void CreateFile(string name)
        {
            this.Name = name;
        }

        public string GetDefaultExtension()
        {
            return "*.txt";
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return new string[] { "*.txt" };
        }

        public Task OpenFile(string filename, IIOProvider provider)
        {
            this.Contents = provider.ReadAllText(filename);
            this.Filename = filename;
            this.Name = Path.GetFileName(filename);
            return Task.CompletedTask;
        }

        public async Task Save(IIOProvider provider)
        {
            await Save(Filename, provider);
        }

        public Task Save(string filename, IIOProvider provider)
        {
            provider.WriteAllText(filename, Contents);
            this.Filename = filename;
            FileSaved?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        public async Task<bool> IsOfType(GenericFile file)
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
