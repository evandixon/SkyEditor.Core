﻿using SkyEditor.Core.Projects;
using SkyEditor.Core.UI;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace SkyEditor.Core
{
    public class IOUIManager : IDisposable, INotifyPropertyChanged/*, IReportProgress*/
    {

        public IOUIManager(PluginManager manager)
        {
            throw new NotImplementedException();
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

        private ObservableCollection<ActionMenuItem> _rootMenuItems;
        #endregion

        public IEnumerable<object> GetViewModelsForModel(object dummy)
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
