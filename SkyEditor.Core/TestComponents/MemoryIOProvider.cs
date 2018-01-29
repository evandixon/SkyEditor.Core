using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace SkyEditor.Core.TestComponents
{
    /// <summary>
    /// Implementation of <see cref="IIOProvider"/> that stores virtual files in memory.
    /// </summary>
    public class MemoryIOProvider : IMountableIOProvider
    {

        /// <summary>
        /// Gets a regular expression for the given search pattern for use with <see cref="GetFiles(string, string, bool)"/>.  Do not provide asterisks.
        /// </summary>
        private static StringBuilder GetFileSearchRegexQuestionMarkOnly(string searchPattern)
        {
            var parts = searchPattern.Split('?');
            var regexString = new StringBuilder();
            foreach (var item in parts)
            {
                regexString.Append(Regex.Escape(item));
                if (item != parts[parts.Length - 1])
                {
                    regexString.Append(".?");
                }
            }
            return regexString;
        }

        /// <summary>
        /// Gets a regular expression for the given search pattern for use with <see cref="GetFiles(string, string, bool)"/>.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string GetFileSearchRegex(string searchPattern)
        {
            var asteriskParts = searchPattern.Split('*');
            var regexString = new StringBuilder(@"(.*)\/");

            foreach (var part in asteriskParts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    // Asterisk
                    regexString.Append(".*");
                }
                else
                {
                    regexString.Append(GetFileSearchRegexQuestionMarkOnly(part));
                }
            }

            return regexString.ToString();
        }

        public MemoryIOProvider()
        {
            Files = new ConcurrentDictionary<string, byte[]>();
            tempCounter = 0;
            ResetWorkingDirectory();
            CreateDirectory("/");
        }

        protected ConcurrentDictionary<string, byte[]> Files { get; set; }

        private int tempCounter;
        private object tempCounterLock = new object();


        public string WorkingDirectory
        {
            get
            {
                return _workingDirectory;
            }
            set
            {
                _workingDirectory = FixPath(value);
            }
        }
        private string _workingDirectory;

        public void ResetWorkingDirectory()
        {
            _workingDirectory = "/";
        }

        /// <summary>
        /// Standardizes the path, making it absolute if not already
        /// </summary>
        /// <param name="path">The path to standardize.  Can be relative to the working directory (<see cref="WorkingDirectory"/>) or absolute</param>
        /// <returns>The standardized absolute path</returns>
        protected string FixPath(string path)
        { 
            var mountParts = path.Split(new[] { ':' }, 2);
            path = (mountParts.Length > 1 ? mountParts[1] : mountParts[0]);

            var fixedPath = WorkingDirectory;
            foreach (var part in path.Replace('\\', '/').Split('/'))
            {
                if (part == ".")
                {
                    // Do nothing
                }
                else if (part == "..")
                {
                    fixedPath = Path.GetDirectoryName(fixedPath);
                }
                else
                {
                    fixedPath = Path.Combine(fixedPath, part);
                }
            }         
            
            return fixedPath.Replace('\\', '/');
        }

        #region IIO Provider Implementation

        public virtual long GetFileLength(string filename)
        {
            var filenameLower = FixPath(filename.ToLowerInvariant());
            return Files.First(x => x.Key.ToLowerInvariant() == filenameLower && x.Value != null).Value.Length;
        }

        public virtual bool FileExists(string filename)
        {
            var filenameLower = FixPath(filename.ToLowerInvariant());
            return Files.Any(x => x.Key.ToLower() == filenameLower && x.Value != null);
        }

        public virtual bool DirectoryExists(string path)
        {
            var dirNameLower = FixPath(path.ToLowerInvariant());
            return Files.Any(x => x.Key.ToLower() == dirNameLower && x.Value == null);
        }

        public virtual void CreateDirectory(string path)
        {
            path = FixPath(path);
            if (!string.IsNullOrEmpty(path))
            {
                // Create the parent directory
                var parentPath = Path.GetDirectoryName(path)?.Replace(@"\", @"/");
                if (!string.IsNullOrEmpty(parentPath) && !DirectoryExists(parentPath))
                {
                    CreateDirectory(parentPath);
                }

                // Create the directory
                Files[path] = null;
            }
        }

        public virtual string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly)
        {
            path = FixPath(path).ToLowerInvariant().TrimEnd('/') + "/";
            var searchPatternRegex = new Regex(GetFileSearchRegex(searchPattern));
            var filesInPath = Files.Where(x => x.Key.ToLowerInvariant().StartsWith(path));
            if (topDirectoryOnly)
            {
                var slashCount = path.Count(x => x == '/');
                filesInPath = filesInPath.Where(x => x.Key.Count(y => y == '/') == slashCount);
            }
            return filesInPath.Where(x => searchPatternRegex.IsMatch(x.Key) && x.Value != null).Select(x => x.Key).ToArray();
        }

        public string[] GetDirectories(string path, bool topDirectoryOnly)
        {
            var pathLower = FixPath(path.ToLowerInvariant().TrimEnd('/'));
            return Files.Where(x => x.Key.ToLowerInvariant().StartsWith(pathLower) && x.Value == null).Select(x => x.Key).ToArray();
        }

        public byte[] ReadAllBytes(string filename)
        {
            var filenameLower = FixPath(filename.ToLower());
            return Files.First(x => x.Key.ToLowerInvariant() == filenameLower && x.Value != null).Value;
        }

        public string ReadAllText(string filename)
        {
            var bytes = ReadAllBytes(FixPath(filename));
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public void WriteAllBytes(string filename, byte[] data)
        {
            Files[FixPath(filename)] = data;
        }

        public void WriteAllText(string filename, string data)
        {
            WriteAllBytes(FixPath(filename), Encoding.UTF8.GetBytes(data));
        }

        public void CopyFile(string sourceFilename, string destinationFilename)
        {
            WriteAllBytes(FixPath(destinationFilename), ReadAllBytes(FixPath(sourceFilename)));
        }

        public void DeleteFile(string filename)
        {
            byte[] dummy;
            var filenameLower = FixPath(filename.ToLowerInvariant());
            foreach (var match in Files.Where(x => x.Key.ToLowerInvariant() == filenameLower && x.Value != null).ToList())
            {
                Files.TryRemove(match.Key, out dummy);
            }
        }

        public void DeleteDirectory(string path)
        {
            byte[] dummy;
            path = FixPath(path);
            if (DirectoryExists(path))
            {
                // Delete child directories
                var pathStart = path.ToLowerInvariant() + "/";
                foreach (var item in Files.Where(x => x.Key.ToLowerInvariant().StartsWith(pathStart)).ToList())
                {
                    Files.TryRemove(item.Key, out dummy);
                }

                // Delete the directory
                Files.TryRemove(Files.FirstOrDefault(x => x.Key.ToLowerInvariant() == path.ToLowerInvariant() && x.Value == null).Key, out dummy);
            }
        }

        public string GetTempFilename()
        {
            string filename = null;
            lock(tempCounterLock)
            {
                while (filename == null || FileExists(filename))
                {
                    filename = "/temp/" + tempCounter++.ToString();
                }                
            }
            WriteAllBytes(filename, Array.Empty<byte>());
            return filename;
        }

        public string GetTempDirectory()
        {
            string dirname = null;
            lock (tempCounterLock)
            {
                while (dirname != null || FileExists(dirname))
                {
                    dirname = "/temp/" + tempCounter++.ToString();
                }
            }
            CreateDirectory(dirname);
            return dirname;
        }

        public Stream OpenFile(string filename)
        {
            if (!FileExists(filename))
            {
                WriteAllBytes(filename, new byte[] { });
            }
            return new MemoryStream(ReadAllBytes(FixPath(filename)), true);
        }

        public Stream OpenFileReadOnly(string filename)
        {
            if (!FileExists(filename))
            {
                WriteAllBytes(filename, new byte[] { });
            }
            return new MemoryStream(ReadAllBytes(FixPath(filename)), false);
        }

        public Stream OpenFileWriteOnly(string filename)
        {
            if (!FileExists(filename))
            {
                WriteAllBytes(filename, new byte[] { });
            }
            return new MemoryStream(ReadAllBytes(FixPath(filename)), true);
        }

        #endregion
    }
}
