using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Projects
{
    /// <summary>
    /// Defines the common functionality of both projects and solutions
    /// </summary>
    public abstract class ProjectBase : INotifyPropertyChanged, INotifyModified, IReportProgress, IOnDisk, ISavable, IDisposable
    {
        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <typeparam name="T">Type of the project</typeparam>
        /// <param name="parentPath">Directory in which the project directory will be created</param>
        /// <param name="projectName">Name of the project</param>
        /// <param name="projectType">Type of the project</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The newly created project</returns>
        public static T CreateProject<T>(string parentPath, string projectName, Type projectType, PluginManager manager) where T : ProjectBase
        {
            // Create the instance
            var output = ReflectionHelpers.CreateInstance(projectType) as T;

            // Get the filename
            var filename = Path.Combine(parentPath, projectName, projectName + "." + output.ProjectFileExtension);
            // Create the directory if it doesn't exist
            if (!manager.CurrentIOProvider.DirectoryExists(Path.GetDirectoryName(filename)))
            {
                manager.CurrentIOProvider.CreateDirectory(Path.GetDirectoryName(filename));
            }

            // Set the properties
            output.Filename = filename;
            output.CurrentPluginManager = manager;
            output.Name = projectName;
            output.Settings = new SettingsProvider(manager);
            return output;
        }

        /// <summary>
        /// Opens a project
        /// </summary>
        /// <typeparam name="T">Type of the project to load</typeparam>
        /// <param name="filename">Path of the project file</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The newly-opened project</returns>
        public static async Task<T> OpenProjectFile<T>(string filename, PluginManager manager) where T : ProjectBase
        {
            // Open the file
            var file = Json.DeserializeFromFile<ProjectFile>(filename, manager.CurrentIOProvider);

            // Get the type
            var projectType = ReflectionHelpers.GetTypeByName(file.AssemblyQualifiedTypeName, manager);
            if (projectType == null)
            {
                // Can't find the type.  Use a dummy one so basic loading can continue.
                projectType = typeof(UnsupportedProject).GetTypeInfo();
            }

            // Create the project & load basic info
            var output = ReflectionHelpers.CreateInstance(projectType) as T;
            output.Filename = filename;
            output.CurrentPluginManager = manager;
            output.Name = file.Name;
            output.Settings = SettingsProvider.Deserialize(file.InternalSettings, manager);

            // Load items
            var itemLoadTasks = new Dictionary<string, Task<IOnDisk>>();
            foreach (var item in file.Items)
            {
                if (item.Value == null)
                {
                    // Directory
                    output.CreateDirectory(item.Key);
                }
                else
                {
                    // Item
                    itemLoadTasks.Add(item.Key, output.LoadProjectItem(item.Value));
                }
            }

            // Add the items
            foreach (var item in itemLoadTasks)
            {
                output.AddItem(item.Key, await item.Value);
            }

            return output;
        }

        #region Child Classes

        protected class ItemValue : IOnDisk
        {
            public string AssemblyQualifiedTypeName { get; set; }
            public string Filename { get; set; }
        }

        protected class ProjectFile
        {
            public ProjectFile()
            {
                Items = new Dictionary<string, ItemValue>();
            }
            public const string CurrentVersion = "v2";
            public string FileFormat { get; set; }
            public string AssemblyQualifiedTypeName { get; set; }
            public string Name { get; set; }
            public Dictionary<string, ItemValue> Items { get; set; }
            public string InternalSettings { get; set; }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the project completes a build
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Raised when a directory is created
        /// </summary>
        public event EventHandler<DirectoryCreatedEventArgs> DirectoryCreated;

        /// <summary>
        /// Raised when a directory is deleted
        /// </summary>
        public event EventHandler<DirectoryDeletedEventArgs> DirectoryDeleted;

        /// <summary>
        /// Raised when an item is added
        /// </summary>
        public event EventHandler<ItemAddedEventArgs> ItemAdded;

        /// <summary>
        /// Raised when an item is removed
        /// </summary>
        public event EventHandler<ItemRemovedEventArgs> ItemRemoved;

        /// <summary>
        /// Raised when the project is modified
        /// </summary>
        public event EventHandler Modified;

        /// <summary>
        /// Raised when the build progresses
        /// </summary>
        public event EventHandler<ProgressReportedEventArgs> ProgressChanged;

        /// <summary>
        /// Raised when the project file has been saved
        /// </summary>
        public event EventHandler FileSaved;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager CurrentPluginManager { get; set; }

        /// <summary>
        /// Path of the project file
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Name of the project
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The default extension used for the project file
        /// </summary>
        public abstract string ProjectFileExtension { get; }

        /// <summary>
        /// Settings associated with the project
        /// </summary>
        public SettingsProvider Settings { get; set; }

        /// <summary>
        /// Matches logical paths to items
        /// </summary>
        /// <remarks>Key: logical path; Value: Item (Projects for Solutions, Files for Projects).
        /// If the value is null, the path is an empty directory.
        ///
        /// Example Paths (In form: "{Path}"/{Value})
        /// ""/null - Represents the root directory
        /// "/Test"/null - directory
        /// "/Test/Ing"/null - directory
        /// "/Test/File"/[GenericFile] - File of type GenericFile, named "File", in directory "Test"</remarks>
        private Dictionary<string, IOnDisk> Items { get; set; }

        /// <summary>
        /// Whether or not the project has unsaved changes
        /// </summary>
        public bool HasUnsavedChanges
        {
            get
            {
                return _unsavedChanges;
            }
            set
            {
                _unsavedChanges = value;
                if (value)
                {
                    Modified?.Invoke(this, new EventArgs());
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUnsavedChanges)));
                }
            }
        }
        bool _unsavedChanges;

        // Build-related properties

        /// <summary>
        /// The progress of the current build
        /// </summary>
        public float Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    ProgressChanged?.Invoke(this, new ProgressReportedEventArgs { IsIndeterminate = IsIndeterminate, Message = Message, Progress = Progress });
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
                }
            }
        }
        private float _progress;

        /// <summary>
        /// What the current build is doing
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    ProgressChanged?.Invoke(this, new ProgressReportedEventArgs { IsIndeterminate = IsIndeterminate, Message = Message, Progress = Progress });
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }
        private string _message;

        /// <summary>
        /// Whether or not the exact build progress can currently be determined
        /// </summary>
        public bool IsIndeterminate
        {
            get
            {
                return _isIndeterminate;
            }
            set
            {
                if (_isIndeterminate != value)
                {
                    _isIndeterminate = value;
                    ProgressChanged?.Invoke(this, new ProgressReportedEventArgs { IsIndeterminate = IsIndeterminate, Message = Message, Progress = Progress });
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIndeterminate)));
                }
            }
        }
        private bool _isIndeterminate;

        /// <summary>
        /// Whether or not the current build is complete
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    Completed?.Invoke(this, new EventArgs());
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                }
            }
        }
        private bool _isCompleted;

        /// <summary>
        /// The status of the current build
        /// </summary>
        public BuildStatus CurrentBuildStatus
        {
            get
            {
                return _currentBuildStatus;
            }
            set
            {
                _currentBuildStatus = value;

                if (IsBuildCompleted)
                {
                    Completed?.Invoke(this, new EventArgs());
                }
            }
        }
        private BuildStatus _currentBuildStatus;

        /// <summary>
        /// Whether or not the build has completed
        /// </summary>
        /// <remarks>
        /// In this context, a completed build means the <see cref="CurrentBuildStatus"/> is <see cref="BuildStatus.Done"/> or <see cref="BuildStatus.Canceled"/>.
        /// </remarks>
        public bool IsBuildCompleted
        {
            get
            {
                return CurrentBuildStatus == BuildStatus.Done || CurrentBuildStatus == BuildStatus.Canceled;
            }
        }

        /// <summary>
        /// Whether or not the build has completed
        /// </summary>
        /// <remarks>
        /// In this context, a completed build means the <see cref="CurrentBuildStatus"/> is <see cref="BuildStatus.Building"/> or <see cref="BuildStatus.Canceling"/>.
        /// </remarks>
        public bool IsBuilding
        {
            get
            {
                return CurrentBuildStatus == BuildStatus.Building || CurrentBuildStatus == BuildStatus.Canceling;
            }
        }

        /// <summary>
        /// Whether or not a build cancellation has been requested
        /// </summary>
        public bool IsCancelRequested
        {
            get
            {
                return CurrentBuildStatus == BuildStatus.Canceling;
            }
        }

        /// <summary>
        /// Whether or not a build can be started
        /// </summary>
        public virtual bool CanBuild
        {
            get
            {
                return !IsBuilding;
            }
        }

        #endregion

        #region Functions

        #region Project Open/Save

        /// <summary>
        /// Creates an instance of the current project item type from the data stored in the project file
        /// </summary>
        /// <param name="item">The data needed to load the item</param>
        protected abstract Task<IOnDisk> LoadProjectItem(ItemValue item);

        /// <summary>
        /// Saves the project to the current file
        /// </summary>
        /// <param name="provider">Instance of the current IO provider</param>
        public Task Save(IIOProvider provider)
        {
            var file = new ProjectFile();
            file.FileFormat = ProjectFile.CurrentVersion;
            file.AssemblyQualifiedTypeName = GetType().AssemblyQualifiedName;
            file.Name = this.Name;
            file.InternalSettings = this.Settings.Serialize();
            file.Items = new Dictionary<string, ItemValue>();

            // Create the item dictionary for the file
            foreach (var item in Items)
            {
                if (item.Value == null)
                {
                    // Directory
                    file.Items.Add(FixPath(item.Key), null);
                }
                else
                {
                    var absolutePath = new Uri(item.Value.Filename);
                    var otherPath = new Uri(Path.GetDirectoryName(Filename));
                    // Item
                    file.Items.Add(FixPath(item.Key),
                               new ItemValue
                               {
                                   Filename = absolutePath.MakeRelativeUri(otherPath).LocalPath,
                                   AssemblyQualifiedTypeName = item.Value.GetType().AssemblyQualifiedName
                               });
                }
            }

            Json.SerializeToFile(this.Filename, file, provider);
            FileSaved?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        #endregion

        #region Building

        /// <summary>
        /// Performs any lengthy initialization that must be done when the project is created
        /// </summary>
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs routine initialization when the project is loaded.
        /// </summary>
        /// <remarks>
        /// This function has two indended uses: loading small files into memory if needed and verifying correct initialization.  Ideally, <see cref="Initialize"/> will already have run, but circumstances (like previous exceptions, the user closing the application, or important files being deleted) may result in it being incomplete.  This function should fix incomplete initialization if that is the case.
        /// </remarks>
        public virtual Task Load()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds the project
        /// </summary>
        public virtual Task Build()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Cancels an in-progress build
        /// </summary>
        public virtual void CancelBuild()
        {
            if (IsBuilding)
            {
                CurrentBuildStatus = BuildStatus.Canceling;
            }
        }

        #endregion

        #region Logical Filesystem

        /// <summary>
        /// Standardizes a path
        /// </summary>
        /// <param name="path">The path to standardize</param>
        /// <returns>A standardized path</returns>
        protected string FixPath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }

        /// <summary>
        /// Gets paths and objects in the logical filesystem.
        /// </summary>
        /// <param name="path">Path of child items.</param>
        /// <param name="recursive">Whether or not to search child directories.</param>
        /// <param name="getDirectories">Whether to get files or directories.</param>
        /// <returns>An instance of <see cref="IEnumerable<KeyValuePair<string, object>>"/>, where each key is the full path and each value is the corresponding object, or null if the path is a directory.</returns>
        private IEnumerable<KeyValuePair<string, IOnDisk>> GetItemsInternal(string path, bool recursive, bool getDirectories)
        {
            var fixedPath = FixPath(path).ToLowerInvariant() + "/";

            // Given directory structure of:
            // /Test
            // /Test/In
            // /Test/Ing
            // /Blarg/Test
            // /Test/Ing/Test
            // 
            // And an path of "/Test"...

            var recursiveSelect = Items.Where(x => x.Key.ToLowerInvariant().StartsWith(fixedPath) && // Check the path
                                  ((getDirectories && x.Value == null) || (!getDirectories && x.Value != null))); // Select the correct type (directory vs item; distinction is directories have a null value

            if (recursive) // Should return /Test/Ing and /Test/Ing/Test
            {

                return recursiveSelect.OrderBy(x => x.Key, new DirectoryStructureComparer());
            }
            else // Should return /Test/Ing only
            {
                // recursiveSelect currently contains /Test/Ing and /Test/Ing/Test
                // fixedPath is /Test/
                // Eliminate anything with more slashes than fixedPath
                var currentSlashCount = fixedPath.Where(x => x == '/').Count();
                return recursiveSelect.Where(x => x.Key.Where(c => c == '/').Count() > currentSlashCount)
                                      .OrderBy(x => x.Key, new DirectoryStructureComparer());
            }
        }

        #region Items
        /// <summary>
        /// Gets the items from the given path
        /// </summary>
        /// <param name="path">Path from which to get the items</param>
        /// <param name="recursive">Whether or not to search all directories or only the given path</param>
        /// <returns>The keys and values of all the items in the given path</returns>
        public Dictionary<string, IOnDisk> GetItems(string path, bool recursive)
        {
            return GetItemsInternal(path, recursive, false).ToDictionary(x => x.Key, y => y.Value);
        }

        /// <summary>
        /// Determines whether or not an item exists at the given path
        /// </summary>
        /// <param name="path">The path of the desired item</param>
        /// <returns>A boolean indicating whether or not an item exists at the given path</returns>
        public bool ItemExists(string path)
        {
            var fixedPath = FixPath(path).ToLowerInvariant();
            return Items.Any(x => x.Key.ToLowerInvariant() == fixedPath && x.Value != null);
        }

        /// <summary>
        /// Gets the item at the given path
        /// </summary>
        /// <param name="path">The path of the desired item</param>
        /// <returns>The item at the given path</returns>
        protected object GetItem(string path)
        {
            var fixedPath = FixPath(path).ToLowerInvariant();
            return Items.FirstOrDefault(x => x.Key.ToLowerInvariant() == fixedPath && x.Value != null).Value;
        }

        /// <summary>
        /// Adds the given item to the solution
        /// </summary>
        /// <param name="path">New path of the item</param>
        /// <param name="item">The item to add</param>
        protected void AddItem(string path, IOnDisk item)
        {
            if (ItemExists(path))
            {
                throw new ArgumentException(Properties.Resources.Project_ItemExistsAtPath, nameof(path));
            }

            var parentDir = FixPath(Path.GetDirectoryName(path));
            if (!string.IsNullOrEmpty(parentDir) && DirectoryExists(parentDir))
            {
                CreateDirectory(parentDir);
            }

            var fixedPath = FixPath(path);
            Items.Add(fixedPath, item);
            ItemAdded?.Invoke(this, new ItemAddedEventArgs(fixedPath));
        }

        /// <summary>
        /// Deletes a directory or item at the given path if it exists
        /// </summary>
        /// <param name="path">Path of the directory or item to delete</param>
        /// <returns>A boolean indicating whether or not the item was deleted</returns>
        protected bool DeleteItem(string path)
        {
            var fixedPathLower = FixPath(path).ToLower();

            var toRemove = Items.Where(x => x.Key.ToLowerInvariant() == fixedPathLower).Select(x => x.Key).FirstOrDefault();
            if (toRemove != null)
            {
                Items.Remove(toRemove);
                ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(toRemove));
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Directories

        /// <summary>
        /// Determines whether or not the given directory exists
        /// </summary>
        /// <param name="path">The directory for which to check</param>
        /// <returns>A boolean indicating whether or not the directory exists</returns>
        public bool DirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return true; // Root directory("") should always exist

            var fixedPath = FixPath(path).ToLowerInvariant();
            return Items.Any(x => x.Key.ToLowerInvariant() == fixedPath && x.Value == null);
        }

        /// <summary>
        /// Gets the directories inside the given directory
        /// </summary>
        /// <param name="path">The directory in which to check</param>
        /// <param name="recursive">Whether or not to search the top directory only</param>
        /// <returns>The directories found inside the given directory</returns>
        public IEnumerable<string> GetDirectories(string path, bool recursive)
        {
            return GetItemsInternal(path, recursive, true).Select(x => x.Key);
        }

        /// <summary>
        /// Determines whether or not a directory can be created inside the given path
        /// </summary>
        /// <param name="parentPath">Path of the directory inside which to create the new directory</param>
        /// <returns>A boolean indicating whether or not a directory can be created inside the given directory</returns>
        public virtual bool CanCreateDirectory(string parentPath)
        {
            return DirectoryExists(parentPath);
        }

        /// <summary>
        /// Creates a directory if it does not exist
        /// </summary>
        /// <param name="path">Path of the new directory</param>
        public void CreateDirectory(string path)
        {
            if (CanCreateDirectory(path))
            {
                var fixedPath = FixPath(path);

                // Ensure parent directory exists
                if (!string.IsNullOrEmpty(path)) // But only if it isn't the root
                {
                    CreateDirectory(Path.GetDirectoryName(path));
                }

                // Create the directory
                if (!DirectoryExists(fixedPath))
                {
                    Items.Add(fixedPath, null);
                    DirectoryCreated?.Invoke(this, new DirectoryCreatedEventArgs(fixedPath));
                }
            }
        }

        /// <summary>
        /// Determines whether or not the directory at the given path can be deleted
        /// </summary>
        /// <param name="directoryPath">Path of the directory to delete</param>
        /// <returns>A boolean indicating whether or not the given directory can be deleted</returns>
        public virtual bool CanDeleteDirectory(string directoryPath)
        {
            return DirectoryExists(directoryPath);
        }

        /// <summary>
        /// Deletes the directory with the given path, along with all of its contents
        /// </summary>
        /// <param name="path">The directory to delete</param>
        public void DeleteDirectory(string path)
        {
            if (CanDeleteDirectory(path))
            {
                // Delete items
                foreach (var item in GetItems(path, true).ToArray())
                {
                    DeleteItem(item.Key);
                }

                // Delete child directories
                foreach (var item in GetDirectories(path, true).ToArray())
                {
                    DeleteDirectory(item);
                }

                // Delete the directory
                if (DeleteItem(path))
                {
                    DirectoryDeleted?.Invoke(this, new DirectoryDeletedEventArgs(path));
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach (var item in Items)
                    {
                        if (item.Value is IDisposable)
                        {
                            (item.Value as IDisposable).Dispose();
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProjectBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public abstract class ProjectBase<T> : ProjectBase where T : class, IOnDisk
    {
        protected new T GetItem(string path)
        {
            return base.GetItem(path) as T;
        }

        protected void AddItem(string path, T item)
        {
            base.AddItem(path, item);
        }
    }
}
