using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Utilities
{
    public static class FileSystem
    {
        /// <summary>
        /// Asynchronously copies a directory
        /// </summary>
        /// <param name="sourceDirectory">The directory to copy</param>
        /// <param name="destinationDirectory">The new destination for the source directory</param>
        public static async Task CopyDirectory(string sourceDirectory, string destinationDirectory, IIOProvider provider)
        {
            // Get the files/directories to copy
            var files = provider.GetFiles(sourceDirectory, "*", false);

            //Create all required directories
            foreach (var item in files)
            {
                var dest = item.Replace(sourceDirectory, destinationDirectory);
                if (!provider.DirectoryExists(Path.GetDirectoryName(dest)))
                {
                    provider.CreateDirectory(Path.GetDirectoryName(dest));
                }
            }

            AsyncFor f = new AsyncFor();
            f.RunSynchronously = false;
            await f.RunForEach(files, path =>
            {
                string dest = path.Replace(sourceDirectory, destinationDirectory);
                provider.CopyFile(path, dest);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Ensures a directory is empty
        /// </summary>
        /// <param name="directoryName">Path of the directory</param>
        /// <param name="provider">I/O provider containing the directory</param>
        public static async Task EnsureDirectoryEmpty(string directoryName, IIOProvider provider)
        {

            // Delete the main directory (to delete all child directories)
            provider.DeleteDirectory(directoryName);

            // Wait until it is fully deleted (because it seems IO.Directory.Delete is asynchronous, and can't be awaited directly)
            await Task.Run(new Action(() =>
            {
                while (provider.DirectoryExists(directoryName))
                {
                    // Block
                }
            })).ConfigureAwait(false);

            // Recreate the main directory
            provider.CreateDirectory(directoryName);
        }
    }
}
