using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Implementation of <see cref="IIOProvider"/> that wraps <see cref="System.IO.File"/> and <see cref="System.IO.Directory"/> (i.e. the physical file system).
    /// </summary>
    public class PhysicalIOProvider : IIOProvider
    {

        public PhysicalIOProvider()
        {
        }

        public virtual void CopyFile(string sourceFilename, string destinationFilename)
        {
            File.Copy(sourceFilename, destinationFilename, true);
        }

        public virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public virtual void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public virtual void DeleteFile(string filename)
        {
            File.Delete(filename);
        }

        public virtual void WriteAllBytes(string filename, byte[] data)
        {
            File.WriteAllBytes(filename, data);
        }

        public virtual void WriteAllText(string filename, string data)
        {
            File.WriteAllText(filename, data);
        }

        public virtual bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public virtual bool FileExists(string Filename)
        {
            return File.Exists(Filename);
        }

        public virtual string[] GetDirectories(string path, bool topDirectoryOnly)
        {
            if (topDirectoryOnly)
            {
                return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            }
            else
            {
                return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            }
        }

        public virtual long GetFileLength(string filename)
        {
            return (new FileInfo(filename)).Length;
        }

        public virtual string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly)
        {
            if (topDirectoryOnly)
            {
                return Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            }
            else
            {
                return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
            }
        }

        public virtual string GetTempDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "SkyEditor", Guid.NewGuid().ToString());
            if (!DirectoryExists(System.Convert.ToString(tempDir)))
            {
                CreateDirectory(System.Convert.ToString(tempDir));
            }
            return tempDir;
        }

        public virtual string GetTempFilename()
        {
            return Path.GetTempFileName();
        }

        public virtual Stream OpenFile(string filename)
        {
            return File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        public virtual Stream OpenFileReadOnly(string filename)
        {
            return File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        }

        public virtual Stream OpenFileWriteOnly(string filename)
        {
            return File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public virtual byte[] ReadAllBytes(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        public virtual string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }
    }
}
