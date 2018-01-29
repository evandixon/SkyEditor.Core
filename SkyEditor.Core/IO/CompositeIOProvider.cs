using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// An I/O provider with support for mounting other I/O providers
    /// </summary>
    public class CompositeIOProvider : IIOProvider
    {
        public CompositeIOProvider() : this(new PhysicalIOProvider())
        {            
        }

        public CompositeIOProvider(IIOProvider defaultIOProvider)
        {
            DefaultIOProvider = defaultIOProvider;
            MountedProviders = new Dictionary<string, IIOProvider>();
            ResetWorkingDirectory();
        }

        public string WorkingDirectory
        {
            get
            {
                return _workingDirectory;
            }
            set
            {
                var provider = GetIOProvider(value);
                provider.WorkingDirectory = value;
                _workingDirectory = value;
            }
        }
        private string _workingDirectory;

        public void ResetWorkingDirectory()
        {
            DefaultIOProvider.ResetWorkingDirectory();
            WorkingDirectory = DefaultIOProvider.WorkingDirectory;
        }

        protected IIOProvider DefaultIOProvider { get; set; }
        
        protected Dictionary<string, IIOProvider> MountedProviders { get; set; }

        protected string FixPath(string path)
        {
            if (path.Contains(":"))
            {
                // If this is a mount, defer path fixing to the mounted I/O provider
                return path;
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return Path.Combine(WorkingDirectory, path);
            }
        }
        
        protected virtual IIOProvider GetIOProvider(string path)
        {
            var mount = FixPath(path).Split(new[] { ':' }, 2)[0];
            if (MountedProviders.ContainsKey(mount.ToLower()))
            {
                return MountedProviders[mount.ToLower()];
            }
            else
            {
                return DefaultIOProvider;
            }
        }

        public void MountProvider(string name, IIOProvider provider)
        {
            var validationRegex = new Regex("[a-zA-Z0-9]");
            if (!validationRegex.IsMatch(name))
            {
                throw new ArgumentException("Mount name must only contain letters and numbers.", nameof(name));
            }
            MountedProviders.Add(name.ToLower(), provider);
        }

        public void CopyFile(string sourceFilename, string destinationFilename)
        {
            var sourceProvider = GetIOProvider(sourceFilename);
            var destProvider = GetIOProvider(destinationFilename);
            if (sourceProvider == destProvider)
            {
                sourceProvider.CopyFile(sourceFilename, destinationFilename);
            }
            else
            {
                if (!destProvider.DirectoryExists(Path.GetDirectoryName(destinationFilename)))
                {
                    destProvider.CreateDirectory(Path.GetDirectoryName(destinationFilename));
                }

                using (var sourceStream = sourceProvider.OpenFileReadOnly(sourceFilename))
                using (var destStream = destProvider.OpenFileWriteOnly(destinationFilename))
                {
                    sourceStream.CopyTo(destStream);
                    destStream.Flush();
                }
            }
        }

        public void CreateDirectory(string path)
        {
            GetIOProvider(path).CreateDirectory(FixPath(path));
        }

        public void DeleteDirectory(string path)
        {
            GetIOProvider(path).DeleteDirectory(FixPath(path));
        }

        public void DeleteFile(string filename)
        {
            GetIOProvider(filename).DeleteFile(FixPath(filename));
        }

        public bool DirectoryExists(string path)
        {
            return GetIOProvider(path).DirectoryExists(FixPath(path));
        }

        public bool FileExists(string filename)
        {
            return GetIOProvider(filename).FileExists(FixPath(filename));
        }

        public string[] GetDirectories(string path, bool topDirectoryOnly)
        {
            return GetIOProvider(path).GetDirectories(FixPath(path), topDirectoryOnly);
        }

        public long GetFileLength(string filename)
        {
            return GetIOProvider(filename).GetFileLength(FixPath(filename));
        }

        public string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly)
        {
            return GetIOProvider(path).GetFiles(FixPath(path), searchPattern, topDirectoryOnly);
        }

        public string GetTempDirectory()
        {
            return DefaultIOProvider.GetTempDirectory();
        }

        public string GetTempFilename()
        {
            return DefaultIOProvider.GetTempFilename();
        }

        public Stream OpenFile(string filename)
        {
            return GetIOProvider(filename).OpenFile(FixPath(filename));
        }

        public Stream OpenFileReadOnly(string filename)
        {
            return GetIOProvider(filename).OpenFileReadOnly(FixPath(filename));
        }

        public Stream OpenFileWriteOnly(string filename)
        {
            return GetIOProvider(filename).OpenFileWriteOnly(FixPath(filename));
        }

        public byte[] ReadAllBytes(string filename)
        {
            return GetIOProvider(filename).ReadAllBytes(FixPath(filename));
        }

        public string ReadAllText(string filename)
        {
            return GetIOProvider(filename).ReadAllText(FixPath(filename));
        }

        public void WriteAllBytes(string filename, byte[] data)
        {
            GetIOProvider(filename).WriteAllBytes(FixPath(filename), data);
        }

        public void WriteAllText(string filename, string data)
        {
            GetIOProvider(filename).WriteAllText(FixPath(filename), data);
        }
    }
}
