using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// Helps with file access
    /// </summary>
    public static class IOHelper
    {
        /// <summary>
        /// A function that can select between instances of <see cref="FileTypeDetectionResult"/> with identical match chances
        /// </summary>
        /// <param name="matches">The matches between which to distinguish</param>
        /// <returns>The <see cref="FileTypeDetectionResult"/> that was selected.</returns>
        public delegate FileTypeDetectionResult DuplicateMatchSelector(IEnumerable<FileTypeDetectionResult> matches);

        /// <summary>
        /// An implementation of <see cref="DuplicateMatchSelector"/> that will pick the first match.
        /// This is not recommended.  It would be better to use a function that selected between matches using user-input.
        /// </summary>
        /// <param name="matches">The matches between which to distinguish</param>
        /// <returns>The <see cref="FileTypeDetectionResult"/> that was selected.</returns>
        public static FileTypeDetectionResult PickFirstDuplicateMatchSelector(IEnumerable<FileTypeDetectionResult> matches)
        {
            return matches.First();
        }

        /// <summary>
        /// Gets the types that implement <see cref="ICreatableFile"/>
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An enumerable of all registered types that implement <see cref="ICreatableFile"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public static IEnumerable<TypeInfo> GetCreateableFileTypes(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            return manager.GetRegisteredTypes<ICreatableFile>();
        }

        /// <summary>
        /// Gets the types that implement <see cref="IOpenableFile"/>
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An enumerable of all registered types that implement <see cref="IOpenableFile"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public static IEnumerable<TypeInfo> GetOpenableFileTypes(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            return manager.GetRegisteredTypes<IOpenableFile>();
        }

        [Obsolete("Use the overload with a PluginManager parameter")]
        public static ICreatableFile CreateNewFile(string newFileName, TypeInfo fileType)
        {
            return CreateNewFile(newFileName, fileType, new PluginManager());
        }

        /// <summary>
        /// Creates a new file
        /// </summary>
        /// <param name="newFileName">Name of the new file</param>
        /// <param name="fileType">Type of the new file.  Must implement <see cref="ICreatableFile"/></param>
        /// <returns>The newly created file</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileType"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fileType"/> does not implement <see cref="ICreatableFile"/> or if it cannot be instantiated (if it's an abstract class, interface, or lacks a default constructor).</exception>
        public static ICreatableFile CreateNewFile(string newFileName, TypeInfo fileType, PluginManager pluginManager)
        {
            if (fileType == null)
            {
                throw new ArgumentNullException(nameof(fileType));
            }

            if (!ReflectionHelpers.IsOfType(fileType, typeof(ICreatableFile).GetTypeInfo()))
            {
                throw new ArgumentException(string.Format(Properties.Resources.Reflection_ErrorInvalidType, nameof(ICreatableFile)), nameof(fileType));
            }

            if (!pluginManager.CanCreateInstance(fileType))
            {
                throw new ArgumentException(Properties.Resources.Reflection_ErrorNoDefaultConstructor, nameof(fileType));
            }

            var file = pluginManager.CreateInstance(fileType) as ICreatableFile;
            file.CreateFile(newFileName);
            return file;
        }

        /// <summary>
        /// Opens a file and returns an object that models it
        /// </summary>
        /// <param name="filename">Path of the file to open</param>
        /// <param name="fileType">Type of the object to model the file</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An object that represents the file that was opened</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filename"/>, <paramref name="fileType"/>, or <paramref name="manager"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if there is not a registered implementation of <see cref="IFileOpener"/> in the current plugin manager that supports <see cref="fileType"/>.</exception>
        public static async Task<object> OpenFile(string filename, TypeInfo fileType, PluginManager manager)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (fileType == null)
            {
                throw new ArgumentNullException(nameof(fileType));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var openers = manager.GetRegisteredObjects<IFileOpener>().Where(x => x.SupportsType(fileType));
            if (!openers.Any())
            {
                throw new ArgumentException(string.Format(Properties.Resources.IO_ErrorNoFileOpener, fileType.ToString()), nameof(fileType));
            }

            if (manager.CanCreateInstance(fileType))
            {
                return await openers.OrderBy(x => x.GetUsagePriority(fileType)).First().OpenFile(fileType, filename, manager.CurrentIOProvider);
            }
            else
            {
                throw new ArgumentException(Properties.Resources.Reflection_ErrorNoDefaultConstructor);
            }
        }

        /// <summary>
        /// Using the given file, auto-detects the file type and creates an instance of an appropriate object to model it.
        /// If no such class could be found, will return the original file
        /// </summary>
        /// <param name="file">The file for which to find a better class</param>
        /// <param name="duplicateFileTypeSelector">Resolves duplicate file type detections</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An object that represents the given file, or <paramref name="file"/> if no such class could be found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="file"/>, <paramref name="duplicateFileTypeSelector"/>, or <paramref name="manager"/> is null.</exception>
        public static async Task<object> OpenFile(GenericFile file, DuplicateMatchSelector duplicateFileTypeSelector, PluginManager manager)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (duplicateFileTypeSelector == null)
            {
                throw new ArgumentNullException(nameof(duplicateFileTypeSelector));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var type = await GetFileType(file, duplicateFileTypeSelector, manager);
            var fileOpeners = manager.GetRegisteredObjects<IFileOpener>().Where(x => x.SupportsType(type));
            var genericFileOpeners = manager.GetRegisteredObjects<IFileFromGenericFileOpener>().Where(x => x.SupportsType(type));
            if (type == null 
                ||
                !(fileOpeners.Any() || genericFileOpeners.Any())
                )
            {
                // Nothing can model the file
                // Re-open GenericFile so it's not readonly
                var newFile = new GenericFile();
                await newFile.OpenFile(file.Filename, manager.CurrentIOProvider);
                return newFile;
            }
            else
            {
                var openers = new List<IBaseFileOpener>();
                openers.AddRange(fileOpeners);
                openers.AddRange(genericFileOpeners);
                var fileOpener = openers.OrderByDescending(x => x.GetUsagePriority(type)).First();
                if (fileOpener is IFileOpener fromFileOpener)
                {
                    return await fromFileOpener.OpenFile(type, file.Filename, manager.CurrentIOProvider);
                }
                else if (fileOpener is IFileFromGenericFileOpener fromGenericFileOpener)
                {
                    return await fromGenericFileOpener.OpenFile(type, file);
                }
                else
                {
                    throw new Exception("Unsupported IBaseFileOpener type: " + fileOpener.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Opens a file, auto-detecting the correct type
        /// </summary>
        /// <param name="path">Path of the file to open</param>
        /// <param name="duplicateFileTypeSelector">Resolves duplicate file type detections</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An object that represents the file at the given path</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="file"/>, <paramref name="duplicateFileTypeSelector"/>, or <paramref name="manager"/> is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown if no file or directory could be found at the given path</exception>
        public static async Task<object> OpenFile(string path, DuplicateMatchSelector duplicateFileTypeSelector, PluginManager manager)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (duplicateFileTypeSelector == null)
            {
                throw new ArgumentNullException(nameof(duplicateFileTypeSelector));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (manager.CurrentIOProvider.FileExists(path))
            {
                using (var file = new GenericFile())
                {
                    file.IsReadOnly = true;
                    await file.OpenFile(path, manager.CurrentIOProvider);
                    return await OpenFile(file, duplicateFileTypeSelector, manager);
                }
            }
            else if (manager.CurrentIOProvider.DirectoryExists(path))
            {
                return OpenDirectory(path, duplicateFileTypeSelector, manager);
            }
            else
            {
                throw new FileNotFoundException(Properties.Resources.IO_FileNotFound, path);
            }
        }

        /// <summary>
        /// Sometimes a "file" actually exists as multiple files in a directory.  This method will open a "file" using the given directory.
        /// </summary>
        /// <param name="path">Path of the directory to open</param>
        /// <param name="duplicateFileTypeSelector">Resolves duplicate file type detections</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>An object representing the given directory, or null if no such object could be found</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="file"/>, <paramref name="duplicateDirectoryTypeSelector"/>, or <paramref name="manager"/> is null.</exception>
        public static async Task<object> OpenDirectory(string path, DuplicateMatchSelector duplicateDirectoryTypeSelector, PluginManager manager)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (duplicateDirectoryTypeSelector == null)
            {
                throw new ArgumentNullException(nameof(duplicateDirectoryTypeSelector));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var type = await GetDirectoryType(path, duplicateDirectoryTypeSelector, manager);
            var openers = manager.GetRegisteredObjects<IFileOpener>().Where(x => x.SupportsType(type));
            if (type == null || !openers.Any())
            {
                // Nothing can model the file
                return null;
            }
            else
            {
                return await openers.OrderBy(x => x.GetUsagePriority(type)).First().OpenFile(type, path, manager.CurrentIOProvider);
            }
        }

        /// <summary>
        /// Gets a type that can represent the given file
        /// </summary>
        /// <param name="file">File for which to detect the type</param>
        /// <param name="duplicateFileTypeSelector">Resolves duplicate file type detections</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>A type that can represent the given file</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="file"/>, <paramref name="duplicateFileTypeSelector"/>, or <paramref name="manager"/> is null.</exception>
        public static async Task<TypeInfo> GetFileType(GenericFile file, DuplicateMatchSelector duplicateFileTypeSelector, PluginManager manager)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (duplicateFileTypeSelector == null)
            {
                throw new ArgumentNullException(nameof(duplicateFileTypeSelector));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var resultSetTasks = new List<Task<IEnumerable<FileTypeDetectionResult>>>();
            foreach (var detector in manager.GetRegisteredObjects<IFileTypeDetector>())
            {
                // Start the file type detection
                var detectTask = Task.Run(async () => {
                    try
                    {
                        return await detector.DetectFileType(file, manager);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Encountered exception when using {detector.GetType().Name} to detect a file type: {ex.ToString()}");
                        return new FileTypeDetectionResult[] { };
                    }                   
                }); 

                // Add the task to a list of running detection tasks, so there is the option of running them asynchronously.
                resultSetTasks.Add(detectTask);

                // However, the file isn't necessarily thread-safe, so if it isn't, only one should be run at any one time
                if (!file.IsThreadSafe)
                {
                    try
                    {
                        await detectTask;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Encountered exception when using {detector.GetType().Name} to detect a file type: {ex.ToString()}");
                        continue;
                    }
                }
            }

            var matches = new List<FileTypeDetectionResult>();

            // Merge all results into one list
            foreach (var item in await Task.WhenAll(resultSetTasks))
            {
                matches.AddRange(item);
            }

            return GetCorrectFileTypeDetectionResult(matches, duplicateFileTypeSelector)?.FileType;
        }

        /// <summary>
        /// Gets a type that can represent the given directory
        /// </summary>
        /// <param name="path">Directory for which to detect the type</param>
        /// <param name="duplicateDirectoryTypeSelector">Resolves duplicate file type detections</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>A type that can represent the given file</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="path"/>, <paramref name="duplicateDirectoryTypeSelector"/>, or <paramref name="manager"/> is null.</exception>
        public static async Task<TypeInfo> GetDirectoryType(string path, DuplicateMatchSelector duplicateDirectoryTypeSelector, PluginManager manager)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (duplicateDirectoryTypeSelector == null)
            {
                throw new ArgumentNullException(nameof(duplicateDirectoryTypeSelector));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var resultSetTasks = new List<Task<IEnumerable<FileTypeDetectionResult>>>();
            foreach (var detector in manager.GetRegisteredObjects<IDirectoryTypeDetector>())
            {
                // Start the file type detection
                var detectTask = detector.DetectDirectoryType(path, manager);

                // Add the task to a list of running detection tasks, so there is the option of running them asynchronously.
                resultSetTasks.Add(detectTask);
            }

            var matches = new List<FileTypeDetectionResult>();

            // Merge all results into one list
            foreach (var item in await Task.WhenAll(resultSetTasks))
            {
                matches.AddRange(item);
            }

            return GetCorrectFileTypeDetectionResult(matches, duplicateDirectoryTypeSelector)?.FileType;
        }

        /// <summary>
        /// Gets the correct file type detection result
        /// </summary>
        /// <param name="results">The results from which to select</param>
        /// <param name="duplicateMatchSelector">A function that can select between duplicate types</param>
        /// <returns>The correct file type detection result, or null if there are no results</returns>
        private static FileTypeDetectionResult GetCorrectFileTypeDetectionResult(IEnumerable<FileTypeDetectionResult> results, DuplicateMatchSelector duplicateMatchSelector)
        {
            if (!results.Any())
            {
                return null;
            }
            else if (results.Count() == 1)
            {
                return results.First();
            }
            else
            {
                // Multiple matches exist.  Find the one with the highest chance of being the correct one.
                var maxChance = results.Max(x => x.MatchChance);
                var topMatches = results.Where(x => x.MatchChance == maxChance);
                if (!topMatches.Any())
                {
                    // Nothing matches the maximum.  Should be unreachable.
                    throw new Exception("Could not find results that match the maximum.  This exception should have been unreachable and likely indicates an error in implementation.");
                }
                else if (topMatches.Count() == 1)
                {
                    return topMatches.First();
                }
                else
                {
                    return duplicateMatchSelector(topMatches);
                }
            }
        }
    }
}
