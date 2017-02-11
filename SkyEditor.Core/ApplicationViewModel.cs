﻿using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using SkyEditor.Core.Settings;
using SkyEditor.Core.UI;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    public class ApplicationViewModel : IDisposable, INotifyPropertyChanged, IReportProgress
    {

        public ApplicationViewModel(PluginManager manager)
        {
            // Set main properties
            this.CurrentPluginManager = manager;
            this.CurrentSolution = null;
            this.OpenFiles = new ObservableCollection<FileViewModel>();
            this.AnchorableViewModels = new ObservableCollection<AnchorableViewModel>();            

            // Set Progress Properties
            this._message = Properties.Resources.UI_Ready;
            this._progress = 0;
            this._isIndeterminate = false;
            this._isCompleted = true;
        }

        #region Events
        /// <summary>
        /// Raised when the background loading operations have completed
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Raised when a file is opened
        /// </summary>
        public event EventHandler<FileOpenedEventArguments> FileOpened;

        /// <summary>
        /// Raised when a file is closed
        /// </summary>
        public event EventHandler<FileClosedEventArgs> FileClosed;

        /// <summary>
        /// Raised when a property has been changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raised when background loading has progressed
        /// </summary>
        public event EventHandler<ProgressReportedEventArgs> ProgressChanged;

        /// <summary>
        /// Raised when the current solution has changed
        /// </summary>
        public event EventHandler SolutionChanged;

        /// <summary>
        /// Raised when the currently selected project has changed
        /// </summary>
        public event EventHandler CurrentProjectChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager CurrentPluginManager { get; set; }

        /// <summary>
        /// The view models for anchorable views
        /// </summary>
        public ObservableCollection<AnchorableViewModel> AnchorableViewModels { get; private set; }


        /// <summary>
        /// The files that are currently open
        /// </summary>
        public ObservableCollection<FileViewModel> OpenFiles { get; private set; }

        /// <summary>
        /// The currently-selected file
        /// </summary>
        public FileViewModel SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFile)));
                }
            }
        }
        private FileViewModel _selectedFile;

        /// <summary>
        /// The currently selected view model (anchorable or file)
        /// </summary>
        public object ActiveViewModel
        {
            get
            {
                return _activeContent;
            }
            set
            {
                // Only update if something changed
                if (_activeContent != value)
                {
                    _activeContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveViewModel)));
                }

                // If active content is a file, update the active file
                if (value is FileViewModel)
                {
                    SelectedFile = value as FileViewModel;
                }
            }
        }
        private object _activeContent;

        /// <summary>
        /// The current solution
        /// </summary>
        public Solution CurrentSolution
        {
            get
            {
                return _currentSolution;
            }
            set
            {
                if (_currentSolution != value)
                {
                    if (_currentSolution != null)
                    {
                        _currentSolution.Dispose();
                    }

                    _currentSolution = value;

                    SolutionChanged?.Invoke(this, new EventArgs());
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSolution)));
                }
            }
        }
        private Solution _currentSolution;

        /// <summary>
        /// 
        /// </summary>
        public Project CurrentProject
        {
            get
            {
                return _currentProject;
            }
            set
            {
                if (_currentProject != value)
                {
                    _currentProject = value;
                    CurrentProjectChanged?.Invoke(this, new EventArgs());
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProject)));
                }
            }
        }
        private Project _currentProject;

        private List<IReportProgress> RunningProgressReportables { get; set; }

        private ObservableCollection<ActionMenuItem> _rootMenuItems;

        // IReportProgress properties

        /// <summary>
        /// The progress of the current loading operation
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
                }
            }
        }
        private float _progress;

        /// <summary>
        /// A user-friendly string identifying what the current loading operation is doing
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }
        private string _message;

        /// <summary>
        /// Whether or not the current loading progress (<see cref="Progress"/>) can be accurately determined
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIndeterminate)));
                }
            }
        }
        private bool _isIndeterminate;

        /// <summary>
        /// Whether or not the current load operation is complete
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                }
            }
        }
        private bool _isCompleted;

        /// <summary>
        /// The current <see cref="ConsoleManager"/> for the application.  This is the class that handles parsing and executing commands from the console.
        /// </summary>
        public ConsoleManager CurrentConsoleManager
        {
            get
            {
                if (_consoleManager == null)
                {
                    _consoleManager = new ConsoleManager(this);
                }
                return _consoleManager;
            }
        }
        private ConsoleManager _consoleManager;

        #endregion

        #region Task Watching
        private class TaskProgressReporterWrapper : IReportProgress
        {
            public TaskProgressReporterWrapper(Task task)
            {
                IsCompleted = task.IsCompleted;
                IsIndeterminate = true;
                Message = Properties.Resources.UI_LoadingGeneric;
                Progress = 0;
                InnerTask = task;
            }

            public TaskProgressReporterWrapper(Task task, string loadingMessage)
            {
                IsCompleted = task.IsCompleted;
                IsIndeterminate = true;
                Message = loadingMessage;
                Progress = 0;
                InnerTask = task;
            }

            public Task InnerTask;

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
                    }
                }
            }
            private float _progress;

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
                    }
                }
            }
            private string _message;

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
                    }
                }
            }
            private bool _isIndeterminate;

            public bool IsCompleted { get; set; }

            public event EventHandler<ProgressReportedEventArgs> ProgressChanged;
            public event EventHandler Completed;

            public async void Start()
            {
                await InnerTask;
                IsCompleted = true;
                Completed?.Invoke(this, new EventArgs());
            }
        }

        private object _loadingStatusLock = new object();
        private object _loadingReportablesLock = new object();

        /// <summary>
        /// Adds the given <paramref name="task"/> to the list of currently loading tasks.
        /// </summary>
        /// <param name="task"><see cref="Task"/> to add to the loading list.</param>
        /// <remarks>This overload will never show determinate progress.</remarks>    
        public void ShowLoading(Task task)
        {
            var wrapper = new TaskProgressReporterWrapper(task);
            wrapper.Start();
            ShowLoading(wrapper);
        }

        /// <summary>
        /// Adds the given <paramref name="task"/> to the list of currently loading tasks.
        /// </summary>
        /// <param name="task"><see cref="Task"/> to add to the loading list.</param>
        /// <param name="loadingMessage">User-friendly loading message.  Usually what the task is doing.</param>
        /// <remarks>This overload will never show determinate progress.</remarks>
        public void ShowLoading(Task task, string loadingMessage)
        {
            var wrapper = new TaskProgressReporterWrapper(task, loadingMessage);
            wrapper.Start();
            ShowLoading(wrapper);
        }

        /// <summary>
        /// Adds the given <paramref name="task"/> to the list of currently loading tasks.
        /// </summary>
        /// <param name="task"><see cref="IReportProgress"/> to add to the loading list.</param>
        public void ShowLoading(IReportProgress task)
        {
            task.Completed += OnLoadingTaskCompleted;
            task.ProgressChanged += OnLoadingTaskProgressed;

            IsCompleted = false;

            lock (_loadingReportablesLock)
            {
                RunningProgressReportables.Add(task);
            }

            UpdateLoadingStatus();
        }

        private void OnLoadingTaskCompleted(object sender, EventArgs e)
        {
            CleanupCompletedTasks();
        }

        private void OnLoadingTaskProgressed(object sender, ProgressReportedEventArgs e)
        {
            UpdateLoadingStatus();
        }

        private void UpdateLoadingStatus()
        {
            lock (_loadingStatusLock)
            {
                lock (_loadingReportablesLock)
                {
                    // Update progress and determinancy
                    if (RunningProgressReportables.Any(x => x.IsIndeterminate))
                    {
                        IsIndeterminate = true;
                    }
                    else
                    {
                        IsIndeterminate = false;

                        if (RunningProgressReportables.Count == 1)
                        {
                            Progress = RunningProgressReportables.First().Progress;
                        }
                        else if (RunningProgressReportables.Count == 0)
                        {
                            // Should be unreachable
                            Progress = 0;
                        }
                        else
                        {
                            RunningProgressReportables.Select(x => x.Progress).Aggregate((x, y) => x * y);
                        }

                        // Update message
                        if (RunningProgressReportables.Count > 1)
                        {
                            Message = Properties.Resources.UI_LoadingGeneric;
                        }
                        else if (RunningProgressReportables.Count == 0)
                        {
                            Message = Properties.Resources.UI_Ready;
                        }
                        else
                        {
                            Message = RunningProgressReportables.First().Message;
                        }
                    }
                }
            }

            ProgressChanged?.Invoke(this, new ProgressReportedEventArgs { IsIndeterminate = this.IsIndeterminate, Message = this.Message, Progress = this.Progress });
        }

        private void CleanupCompletedTasks()
        {
            lock (_loadingReportablesLock)
            {
                foreach (var item in RunningProgressReportables.Where(x => x.IsCompleted).ToList())
                {
                    item.Completed -= OnLoadingTaskCompleted;
                    item.ProgressChanged -= OnLoadingTaskProgressed;
                    RunningProgressReportables.Remove(item);
                }

                if (RunningProgressReportables.Count == 0)
                {
                    IsCompleted = true;
                    Completed?.Invoke(this, new EventArgs());
                }
            }
        }
        #endregion

        #region Functions

        #region I/O Filters
        /// <summary>
        /// Gets the IO filter string for use with an OpenFileDialog or a SaveFileDialog.
        /// </summary>
        /// <param name="filters">A collection containing the extensions to put in the string.</param>
        /// <param name="addSupportedFilesEntry">Whether or not to add a "Supported Files" entry to the filter.</param>
        /// <param name="allowAllFiles">Whether or not to add an "All Files" entry to the filters.</param>
        /// <returns>A string that can be used directly with the filter of an OpenFileDialog or a SaveFileDialog.</returns>
        public string GetIOFilter(ICollection<string> filters, bool addSupportedFilesEntry, bool allowAllFiles)
        {
            // Register any unregistered filters
            foreach (var item in (from f in filters where !CurrentPluginManager.IOFilters.ContainsKey(f) select f))
            {
                CurrentPluginManager.IOFilters.Add(item, string.Format(Properties.Resources.UI_UnknownFileRegisterTemplate, item.Trim('*').Trim('.').ToUpper()));
            }

            // Generate the IO Filter string
            var fullFilter = new StringBuilder();
            var usableFilters = CurrentPluginManager.IOFilters.Where(x => filters.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);

            if (addSupportedFilesEntry)
            {
                fullFilter.Append(Properties.Resources.UI_SupportedFiles + "|" +
                    string.Join(";", usableFilters.Select(x => "*." + x.Key.Trim('*').Trim('.')) + "|"));
            }

            fullFilter.Append(string.Join("|", from i in usableFilters select string.Format("{0} ({1})|{1}", i.Value, "*." + i.Key.Trim('*').Trim('.'))));

            if (allowAllFiles)
            {
                fullFilter.Append("|" + Properties.Resources.UI_AllFiles + " (*.*)|*.*");
            }

            return fullFilter.ToString();
        }

        /// <summary>
        /// Gets the IO filter string for use with an OpenFileDialog or a SaveFileDialog.
        /// </summary>
        /// <returns>A string that can be used directly with the filter of an OpenFileDialog or a SaveFileDialog.</returns>
        public string GetIOFilter()
        {
            return GetIOFilter(CurrentPluginManager.IOFilters.Keys, true, true);
        }

        /// <summary>
        /// Gets the IO filter string for use with an OpenFileDialog or a SaveFileDialog.
        /// </summary>
        /// <param name="filters">A collection containing the extensions to put in the string.</param>
        /// <returns>A string that can be used directly with the filter of an OpenFileDialog or a SaveFileDialog.</returns>
        public string GetIOFilter(ICollection<string> filters)
        {
            return GetIOFilter(filters, true, true);
        }

        #endregion

        #region File Open/Close
        /// <summary>
        /// Creates a new <see cref="FileViewModel"/> wrapper for the given model.
        /// </summary>
        /// <param name="model">Model for which to create the <see cref="FileViewModel"/> wrapper.</param>
        /// <returns>A new <see cref="FileViewModel"/> wrapper.</returns>
        protected virtual FileViewModel CreateViewModel(object model)
        {
            FileViewModel output = new FileViewModel();
            output.Model = model;
            return output;
        }

        /// <summary>
        /// Opens the given file
        /// </summary>
        /// <param name="model">The model to open</param>
        /// <param name="disposeOnClose">True to call the file's dispose method (if IDisposable) when closed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        public void OpenFile(object model, bool disposeOnClose)
        {
            if (model == null)
            {
                throw (new ArgumentNullException(nameof(model)));
            }

            if (!OpenFiles.Any(x => ReferenceEquals(x.Model, model)))
            {
                var wrapper = CreateViewModel(model);
                wrapper.DisposeOnClose = disposeOnClose;
                OpenFiles.Add(wrapper);
                FileOpened?.Invoke(this, new FileOpenedEventArguments { File = model, FileViewModel = wrapper, DisposeOnExit = disposeOnClose });
            }
        }

        /// <summary>
        /// Opens the given file
        /// </summary>
        /// <param name="model">File to open</param>
        /// <param name="parentProject">Project the file belongs to.  If the file does not belong to a project, don't use this overload.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> or <paramref name="parentProject"/> is null.</exception>
        public void OpenFile(object model, Project parentProject)
        {
            if (ReferenceEquals(model, null))
            {
                throw (new ArgumentNullException(nameof(model)));
            }
            if (ReferenceEquals(parentProject, null))
            {
                throw (new ArgumentNullException(nameof(parentProject)));
            }

            if (!OpenFiles.Any(x => ReferenceEquals(x.Model, model)))
            {
                var wrapper = CreateViewModel(model);
                wrapper.DisposeOnClose = false;
                wrapper.ParentProject = parentProject;
                OpenFiles.Add(wrapper);
                FileOpened?.Invoke(this, new FileOpenedEventArguments { File = model, FileViewModel = wrapper, DisposeOnExit = false, ParentProject = parentProject });
            }
        }

        /// <summary>
        /// Opens a file from the given filename.
        /// </summary>
        /// <param name="filename">Full path of the file to open.</param>
        /// <param name="autoDetectSelector">Delegate function used to resolve duplicate auto-detection results.</param>
        /// <remarks>This overload is intended to open files on disk that are not associated with a project, automatically determining the file type.
        /// To open a project file, use <see cref="OpenFile(Object, Project)"/>.
        /// To open a file that is not necessarily on disk, use <see cref="OpenFile(Object, Boolean)"/>.
        /// To open a file using a specific type as the model, use <see cref="OpenFile(String, TypeInfo)"/>.
        ///
        /// When the file is closed, the underlying model will be disposed.</remarks>
        public async Task OpenFile(string filename, IOHelper.DuplicateMatchSelector autoDetectSelector)
        {
            var model = await IOHelper.OpenFile(filename, autoDetectSelector, CurrentPluginManager);

            if (!OpenFiles.Any(x => ReferenceEquals(x.Model, model)))
            {
                var wrapper = CreateViewModel(model);
                wrapper.Filename = filename;
                wrapper.DisposeOnClose = true;
                OpenFiles.Add(wrapper);
                FileOpened?.Invoke(this, new FileOpenedEventArguments { File = model, FileViewModel = wrapper, DisposeOnExit = true });
            }
        }

        /// <summary>
        /// Opens a file from the given filename.
        /// </summary>
        /// <param name="filename">Full path of the file to open.</param>
        /// <param name="modelType">Type of the model of the file.</param>
        /// <remarks>This overload is intended to open files on disk, using a specific file type, that are not associated with a project.
        /// To open a project file, use <see cref="OpenFile(Object, Project)"/>.
        /// To open a file that is not necessarily on disk, use <see cref="OpenFile(Object, Boolean)"/>.
        /// To open a file, auto-detecting the file type, use <see cref="OpenFile(String, IOHelper.DuplicateMatchSelector)"/>.
        ///
        /// When the file is closed, the underlying model will be disposed.</remarks>
        public async Task OpenFile(string filename, TypeInfo modelType)
        {
            var model = await IOHelper.OpenFile(filename, modelType, CurrentPluginManager);

            if (!OpenFiles.Any(x => ReferenceEquals(x.Model, model)))
            {
                var wrapper = CreateViewModel(model);
                wrapper.Filename = filename;
                wrapper.DisposeOnClose = true;
                OpenFiles.Add(wrapper);
                FileOpened?.Invoke(this, new FileOpenedEventArguments { File = model, FileViewModel = wrapper, DisposeOnExit = true });
            }
        }

        /// <summary>
        /// Closes the file
        /// </summary>
        /// <param name="File">File to close</param>
        public void CloseFile(FileViewModel file)
        {
            if (file != null)
            {
                for (var i = OpenFiles.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(OpenFiles[i], file))
                    {
                        OpenFiles[i].Dispose();
                        OpenFiles.RemoveAt(i);
                    }
                }

                if (ReferenceEquals(file, SelectedFile))
                {
                    SelectedFile = null;
                }

                FileClosed?.Invoke(this, new FileClosedEventArgs { File = file.Model });
            }
        }
        #endregion

        #region UI Components
        /// <summary>
        /// Gets the top-level menu items.  Children are properly referenced.
        /// </summary>
        /// <returns>The top-level menu items</returns>
        public async Task<ObservableCollection<ActionMenuItem>> GetRootMenuItems()
        {
            if (ReferenceEquals(_rootMenuItems, null))
            {
                _rootMenuItems = new ObservableCollection<ActionMenuItem>();
                //Generate the menu items
                foreach (var item in UIHelper.GenerateLogicalMenuItems(await UIHelper.GetMenuItemInfo(CurrentPluginManager, CurrentPluginManager.CurrentSettingsProvider.GetIsDevMode()), this, null))
                {
                    _rootMenuItems.Add(item);
                }
                //Update their visibility now that all of them have been created
                //Doing this before they're all created will cause unintended behavior
                foreach (var item in _rootMenuItems)
                {
                    await UpdateMenuItemVisibility(item);
                }
            }
            return _rootMenuItems;
        }

        /// <summary>
        /// Gets the <see cref="FileViewModel"/> wrapping <paramref name="model"/>, provided it exists in <see cref="OpenFiles"/>.
        /// </summary>
        /// <param name="model">The model for which to find the <see cref="FileViewModel"/>.</param>
        /// <returns>The <see cref="FileViewModel"/> wrapping <paramref name="model"/>, <paramref name="model"/> if it is itself a <see cref="FileViewModel"/>, or null if it does not exist.</returns>
        public FileViewModel GetFileViewModelForModel(object model)
        {
            var fvm = OpenFiles.FirstOrDefault(x => ReferenceEquals(x.Model, model));
            if (fvm == null)
            {
                return model as FileViewModel; // Return model or null
            }
            else
            {
                return fvm;
            }
        }

        /// <summary>
        /// Gets the current view models for the model, creating them if necessary.
        /// </summary>
        /// <param name="model">Model for which to get the view models.</param>
        /// <returns>An IEnumerable of view models that support the given model, or null if the model is not an open file.</returns>
        public IEnumerable<GenericViewModel> GetViewModelsForModel(object model)
        {
            var file = GetFileViewModelForModel(model);
            if (file != null)
            {
                // The file is open
                return file.GetViewModels(this);
            }
            else if (model is FileViewModel)
            {
                // The model provided is a file view model, which we can still work with
                return (model as FileViewModel).GetViewModels(this);
            }
            else
            {
                // The file is not open
                return null;
            }
        }

        /// <summary>
        /// Gets the possible targets for a menu action.
        /// </summary>
        private IEnumerable<object> GetMenuActionTargets()
        {
            List<object> output = new List<object>();

            if (CurrentSolution != null)
            {
                output.Add(CurrentSolution);
            }

            if (CurrentProject != null)
            {
                output.Add(CurrentProject);
            }

            if (SelectedFile != null)
            {
                output.Add(SelectedFile);
                output.Add(SelectedFile.Model);

                if (SelectedFile.Model is GenericViewModel && (SelectedFile.Model as GenericViewModel).Model != null)
                {
                    output.Add((SelectedFile.Model as GenericViewModel).Model);
                }
            }

            return output;
        }

        /// <summary>
        /// Gets the targets for the given menu action
        /// </summary>
        /// <param name="action">The action for which to retrieve the targets</param>
        public async Task<IEnumerable<object>> GetMenuActionTargets(MenuAction action)
        {
            List<object> targets = new List<object>();

            // Add the current project to the targets if supported
            if (CurrentSolution != null && await action.SupportsObject(CurrentSolution))
            {
                targets.Add(CurrentSolution);
            }

            // Add the current project if supported
            if (CurrentProject != null && await action.SupportsObject(CurrentProject))
            {
                targets.Add(CurrentProject);
            }

            // Add the selected file if supported
            if (SelectedFile != null)
            {
                // Add the file's view model if supported
                if (await action.SupportsObject(SelectedFile))
                {
                    targets.Add(SelectedFile);
                }

                // Add the model if supported
                if (await action.SupportsObject(SelectedFile.Model))
                {
                    targets.Add(SelectedFile.Model);
                }

                // Add a view model for the current file if available
                foreach (var item in SelectedFile.GetViewModels(this))
                {
                    if (await action.SupportsObject(item))
                    {
                        targets.Add(item);
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// Updates the visibility for the given menu item and its children, and returns the updated visibility
        /// </summary>
        /// <param name="menuItem">The menu items for which to update the visibility</param>
        private async Task<bool> UpdateMenuItemVisibility(ActionMenuItem menuItem)
        {
            var possibleTargets = GetMenuActionTargets(); // Note: Excludes view models for the selected file

            // Default to not visible
            var isVisible = false;

            if (menuItem.Actions != null)
            {
                // Visibility is determined by every available action
                // If any one of those actions is applicable, then this menu item is visible
                foreach (var item in menuItem.Actions)
                {
                    if (!isVisible)
                    {
                        if (item.AlwaysVisible)
                        {
                            // Then this action is always visible
                            isVisible = true;

                            // And don't bother checking the rest
                            break;
                        }
                        else
                        {
                            foreach (var target in possibleTargets)
                            {
                                // Check to see if this target is supported
                                if (await item.SupportsObject(target))
                                {
                                    // If it is, then this menu item should be visible
                                    isVisible = true;

                                    // And don't bother checking the rest
                                    break;
                                }
                            }

                            if (!isVisible && SelectedFile?.Model != null)
                            {
                                // Check to see if the action supports any view models
                                // If there are any view models that support the selected file,
                                isVisible = false;
                                foreach (var vm in SelectedFile.GetViewModels(this))
                                {
                                    if (await item.SupportsObject(vm))
                                    {
                                        isVisible = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Then this menu item is visible, and don't bother checking the rest
                        break;
                    }
                }
            }

            // Update children
            foreach (var item in menuItem.Children)
            {
                if (await UpdateMenuItemVisibility(item))
                {
                    isVisible = true;
                }
            }

            // Set the visibility to the value we calculated
            menuItem.IsVisible = isVisible;

            // Set this item to visible if there's a visible
            return isVisible;
        }

        /// <summary>
        /// Adds the given anchorable ViewModel to the list of open anchorable ViewModels
        /// </summary>
        /// <param name="model">The anchorable view model to add</param>
        public void ShowAnchorable(AnchorableViewModel anchorableViewModel)
        {
            var targetType = anchorableViewModel.GetType().GetTypeInfo();
            if (!AnchorableViewModels.Any(x => ReflectionHelpers.IsOfType(x, targetType)))
            {
                anchorableViewModel.CurrentApplicationViewModel = this;
                AnchorableViewModels.Add(anchorableViewModel);
            }
        }
        #endregion

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        //public float Progress => throw new NotImplementedException();

        //public string Message => throw new NotImplementedException();

        //public bool IsIndeterminate => throw new NotImplementedException();

        //public bool IsCompleted => throw new NotImplementedException();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IOUIManager() {
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
}