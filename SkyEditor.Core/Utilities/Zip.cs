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
        public static async Task Unzip(string zipFilename, string outputDir, IIOProvider provider)
        {
            using (var archive = provider.OpenFileReadOnly(zipFilename))
            {
                if (!provider.DirectoryExists(outputDir))
                {
                    provider.CreateDirectory(outputDir);
                }

                var zip = new ZipArchive(archive);
                foreach (var item in zip.Entries)
                {
                    using (var zipEntry = item.Open())
                    {
                        using (var file = provider.OpenFileWriteOnly(Path.Combine(outputDir, item.FullName)))
                        {
                            await zipEntry.CopyToAsync(file);
                        }
                    }
                }
            }
                
        }
    }
}
