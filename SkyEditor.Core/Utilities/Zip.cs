using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Utilities
{
    public static class Zip
    {
        /// <summary>
        /// Unzips the given zip file to the given directory
        /// </summary>
        /// <param name="zipFilename">Path of the file to unzip</param>
        /// <param name="outputDir">Directory in which to unzip the files</param>
        /// <param name="provider">I/O provider containing the zip file and the output directory</param>
        public static async Task UnzipDir(string zipFilename, string outputDir, IIOProvider provider)
        {
            using (var archive = provider.OpenFileReadOnly(zipFilename))
            {
                if (!provider.DirectoryExists(outputDir))
                {
                    provider.CreateDirectory(outputDir);
                }

                using (var zip = new ZipArchive(archive, ZipArchiveMode.Read))
                {
                    foreach (var item in zip.Entries)
                    {
                        using (var zipEntry = item.Open())
                        {
                            if (!provider.DirectoryExists(Path.GetDirectoryName(Path.Combine(outputDir, item.FullName))))
                            {
                                provider.CreateDirectory(Path.GetDirectoryName(Path.Combine(outputDir, item.FullName)));
                            }

                            using (var file = provider.OpenFileWriteOnly(Path.Combine(outputDir, item.FullName)))
                            {
                                await zipEntry.CopyToAsync(file);
                            }
                        }
                    }
                }                    
            }                
        }

        /// <summary>
        /// Zips the given directory
        /// </summary>
        /// <param name="inputDir">Directory to zip</param>
        /// <param name="zipFilename">Path of the target zip file</param>
        /// <param name="provider">I/O provider containing the zip file and the input directory</param>
        public static async Task ZipDir(string inputDir, string zipFilename, IIOProvider provider)
        {
            using (var archive = provider.OpenFile(zipFilename))
            {
                using (var zip = new ZipArchive(archive, ZipArchiveMode.Create))
                {
                    foreach (var filename in provider.GetFiles(inputDir, "*", false))
                    {
                        using (var file = provider.OpenFileReadOnly(filename))
                        {
                            var entry = zip.CreateEntry(FileSystem.MakeRelativePath(filename, inputDir), CompressionLevel.Optimal);
                            using (var entryStream = entry.Open())
                            {
                                await file.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
            }
        }
    }
}
