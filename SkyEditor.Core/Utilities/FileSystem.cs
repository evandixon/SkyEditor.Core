using SkyEditor.Core.IO;
using SkyEditor.IO.FileSystem;
using SkyEditor.Utilities.AsyncFor;
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
        public static async Task CopyDirectory(string sourceDirectory, string destinationDirectory, IFileSystem provider)
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
        /// Ensures a directory exists and is empty
        /// </summary>
        /// <param name="directoryName">Path of the directory</param>
        /// <param name="provider">I/O provider containing the directory</param>
        public static async Task EnsureDirectoryExistsEmpty(string directoryName, IFileSystem provider)
        {

            // Delete the main directory (to delete all child directories)
            if (provider.DirectoryExists(directoryName))
            {
                provider.DeleteDirectory(directoryName);
            }            

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

        /// <summary>
        /// Makes the given path a relative path
        /// </summary>
        /// <param name="targetPath">The path to make relative</param>
        /// <param name="relativeToPath">The path to which <paramref name="targetPath"/> is relative.  Must be a directory</param>
        /// <returns><paramref name="targetPath"/>, except relative to <paramref name="relativeToPath"/></returns>
        public static string MakeRelativePath(string targetPath, string relativeToPath)
        {
            var otherPathString = relativeToPath.Replace('\\', '/');
            if (!otherPathString.EndsWith("/"))
            {
                otherPathString += "/";
            }
            var absolutePath = new Uri("file://" + targetPath.Replace('\\','/'));
            var otherPath = new Uri("file://" + otherPathString);

            return Uri.UnescapeDataString(otherPath.MakeRelativeUri(absolutePath).OriginalString);
        }
    }
}
