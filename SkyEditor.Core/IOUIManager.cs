using SkyEditor.Core.Projects;
using SkyEditor.Core.UI;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    public class IOUIManager : IDisposable, INotifyPropertyChanged, IReportProgress
    {

        public IOUIManager(PluginManager manager)
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
        /// Filters used in Open and Save dialogs.  Key: Extension, Value: Friendly name
        /// </summary>
        public Dictionary<string, string> IOFilters { get; private set; }

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

            public float Progress { get; set; }

            public string Message { get; set; }

            public bool IsIndeterminate { get; set; }

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

        public IEnumerable<GenericViewModel> GetViewModelsForModel(object dummy)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<object>> GetMenuActionTargets(MenuAction action)
        {
            throw new NotImplementedException();
        }

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
