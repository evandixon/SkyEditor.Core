﻿using SkyEditor.Core.IO;
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
    public class MemoryIOProvider : IIOProvider
    {

        public MemoryIOProvider()
        {
            Files = new ConcurrentDictionary<string, byte[]>();
            EnableInMemoryLoad = true;
            tempCounter = 0;
        }

        protected ConcurrentDictionary<string, byte[]> Files { get; set; }

        /// <summary>
        /// Controls the output of <see cref="CanLoadFileInMemory(long)"/>
        /// </summary>
        public bool EnableInMemoryLoad { get; set; }

        private int tempCounter;
        private object tempCounterLock = new object();

        #region IIO Provider Implementation

        public virtual long GetFileLength(string filename)
        {
            var filenameLower = filename.ToLowerInvariant();
            return Files.First(x => x.Key.ToLowerInvariant() == filenameLower && x.Value != null).Value.Length;
        }

        public virtual bool FileExists(string filename)
        {
            var filenameLower = filename.ToLowerInvariant();
            return Files.Any(x => x.Key.ToLower() == filenameLower && x.Value != null);
        }

        public virtual bool DirectoryExists(string path)
        {
            var dirNameLower = path.ToLowerInvariant();
            return Files.Any(x => x.Key.ToLower() == dirNameLower && x.Value == null);
        }

        public virtual void CreateDirectory(string path)
        {
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

        /// <summary>
        /// Gets a regular expression for the given search pattern for use with <see cref="GetFiles(string, string, bool)"/>.  Do not provide asterisks.
        /// </summary>
        private StringBuilder GetFileSearchRegexQuestionMarkOnly(string searchPattern)
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
        protected string GetFileSearchRegex(string searchPattern)
        {
            var asteriskParts = searchPattern.Split('*');
            var regexString = new StringBuilder(@"(.*)\/");

            foreach (var part in asteriskParts)
            {
                regexString.Append(GetFileSearchRegexQuestionMarkOnly(part));
                if (part != asteriskParts[asteriskParts.Length - 1])
                {
                    regexString.Append(".*");
                }
            }

            return regexString.ToString();
        }

        public virtual string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly)
        {
            var searchPatternRegex = new Regex(searchPattern);
            return Files.Where(x => searchPatternRegex.IsMatch(x.Key) && x.Value != null).Select(x => x.Key).ToArray();
        }

        public string[] GetDirectories(string path, bool topDirectoryOnly)
        {
            var pathLower = path.ToLowerInvariant() + "/";
            return Files.Where(x => x.Key.ToLowerInvariant().StartsWith(pathLower) && x.Value == null).Select(x => x.Key).ToArray();
        }

        public byte[] ReadAllBytes(string filename)
        {
            var filenameLower = filename.ToLower();
            return Files.First(x => x.Key.ToLowerInvariant() == filenameLower && x.Value != null).Value;
        }

        public string ReadAllText(string filename)
        {
            var bytes = ReadAllBytes(filename);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public void WriteAllBytes(string filename, byte[] data)
        {
            Files[filename.Replace(@"\", @"/")] = data;
        }

        public void WriteAllText(string filename, string data)
        {
            WriteAllBytes(filename, Encoding.UTF8.GetBytes(data));
        }

        public void CopyFile(string sourceFilename, string destinationFilename)
        {
            WriteAllBytes(destinationFilename, ReadAllBytes(sourceFilename));
        }

        public void DeleteFile(string filename)
        {
            byte[] dummy;
            var filenameLower = filename.ToLowerInvariant();
            foreach (var match in Files.Where(x => x.Key.ToLowerInvariant() == filenameLower && x.Value != null).ToList())
            {
                Files.TryRemove(match.Key, out dummy);
            }
        }

        public void DeleteDirectory(string path)
        {
            byte[] dummy;
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
                while (filename != null || FileExists(filename))
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
            return new MemoryStream(ReadAllBytes(filename), true);
        }

        public Stream OpenFileReadOnly(string filename)
        {
            return new MemoryStream(ReadAllBytes(filename), false);
        }

        public Stream OpenFileWriteOnly(string filename)
        {
            return new MemoryStream(ReadAllBytes(filename), true);
        }

        #endregion
    }
}