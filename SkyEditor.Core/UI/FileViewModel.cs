﻿using SkyEditor.Core.IO;
using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.Core.Projects;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// The view model for any model, especially models that represent open files
    /// </summary>
    public class FileViewModel : IDisposable, INotifyPropertyChanged, INotifyModified
    {       

        public FileViewModel(PluginManager pluginManager)
        {
            IsFileModified = false;
            CloseCommand = new RelayCommand(new Action<object>(CloseAction));
            CurrentPluginManager = pluginManager;
        }

        public FileViewModel(object model, PluginManager pluginManager) : this(pluginManager)
        {
            this.Model = model;
        }

        #region Events
        public event EventHandler CloseCommandExecuted;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Modified;
        public event EventHandler MenuItemRefreshRequested;

        private void OnMenuItemRefreshRequested(object sender, EventArgs e)
        {
            MenuItemRefreshRequested?.Invoke(sender, e);
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion

        #region Event Handlers
        private void File_OnSaved(object sender, EventArgs e)
        {
            IsFileModified = false;
        }

        private void File_OnModified(object sender, EventArgs e)
        {
            IsFileModified = true;
        }
        #endregion

        #region Properties

        /// <summary>
        /// The path of the file, or null if the file is not on disk
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Whether or not the file has been modified since it was last opened or saved
        /// </summary>
        public bool IsFileModified
        {
            get
            {
                return _isFileModified;
            }
            set
            {
                if (_isFileModified != value)
                {
                    _isFileModified = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFileModified)));

                    // Title is dependant on this property, so notify that it has changed too
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));

                    if (_isFileModified)
                    {
                        Modified?.Invoke(this, new EventArgs());
                    }
                }
            }
        }
        private bool _isFileModified;

        /// <summary>
        /// The display title of the file
        /// </summary>
        public string Title
        {
            get
            {
                // Get the display name
                string output;
                if (!String.IsNullOrEmpty(Filename))
                {
                    output = Path.GetFileName(Filename);
                }
                else if (Model is INamed)
                {
                    output = (Model as INamed).Name;
                }
                else if (Model is IOnDisk)
                {
                    output = Path.GetFileName((Model as IOnDisk).Filename);
                }
                else if (Model != null)
                {
                    output = ReflectionHelpers.GetTypeFriendlyName(Model.GetType());
                }
                else
                {
                    output = Properties.Resources.UI_FileViewModel_NullTitle;
                }

                // Indicate if the file has been modified
                if (IsFileModified)
                {
                    return "* " + output;
                }
                else
                {
                    return output;
                }
            }
        }

        /// <summary>
        /// The underlying model the view model represents
        /// </summary>
        public object Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (!ReferenceEquals(_model, value))
                {
                    // Cleanup existing event handlers
                    if (_model is ISavable)
                    {
                        (_model as ISavable).FileSaved -= File_OnSaved;
                    }
                    if (_model is INotifyModified)
                    {
                        (_model as INotifyModified).Modified -= File_OnModified;
                    }

                    // Pre-set logic
                    ResetViewModels();
                    var originallyNull = (_model == null);

                    // Set value
                    _model = value;

                    // Post-set logic
                    if (originallyNull)
                    {
                        // If we're loading the file, it hasn't been modified
                        IsFileModified = false;
                    }
                    else
                    {
                        // If the file is being replaced, it has been modified
                        IsFileModified = true;
                    }

                    // Add new event handlers
                    if (_model is ISavable)
                    {
                        (_model as ISavable).FileSaved += File_OnSaved;
                    }
                    if (_model is INotifyModified)
                    {
                        (_model as INotifyModified).Modified += File_OnModified;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
                }

            }
        }
        private object _model;

        /// <summary>
        /// The view models for the current model
        /// </summary>
        private List<GenericViewModel> ViewModels { get; set; }

        /// <summary>
        /// The command used to close the file
        /// </summary>
        public RelayCommand CloseCommand { get; }

        /// <summary>
        /// Whether or not to dispose the underlying model when the view model is closed
        /// </summary>
        public bool DisposeOnClose { get; set; }

        /// <summary>
        /// The project to which the file belongs, or null if the file is independant
        /// </summary>
        public Project ParentProject { get; set; }

        private PluginManager CurrentPluginManager { get; }

        #endregion

        #region Functions

        #region View Models
        /// <summary>
        /// Clears and disposes existing view models
        /// </summary>
        private void ResetViewModels()
        {
            if (ViewModels != null)
            {
                // Cleanup
                foreach (var item in ViewModels)
                {
                    // Remove handlers first
                    if (item is ISavable)
                    {
                        (item as ISavable).FileSaved -= File_OnSaved;
                    }
                    if (item is INotifyModified)
                    {
                        (item as INotifyModified).Modified -= File_OnSaved;
                    }
                    item.MenuItemRefreshRequested -= OnMenuItemRefreshRequested;

                    // Dispose of view models if applicable
                    (item as IDisposable)?.Dispose();
                }

                // Reset
                ViewModels.Clear();
                ViewModels = null;
            }
        }

        /// <summary>
        /// Tells the view models to update the underlying model
        /// </summary>
        private void ForceViewModelRefresh()
        {
            foreach (var item in ViewModels ?? Enumerable.Empty<GenericViewModel>())
            {
                item.UpdateModel(Model);
            }
        }

        /// <summary>
        /// Gets the current view models for the given file, creating them if necessary
        /// </summary>
        /// <param name="appViewModel">Instance of the current application ViewModel</param>
        /// <returns>An IEnumerable of view models that support the given file's model</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public IEnumerable<GenericViewModel> GetViewModels(ApplicationViewModel appViewModel)
        {
            if (appViewModel == null)
            {
                throw new ArgumentNullException(nameof(appViewModel));
            }

            // Initialize if null
            if (ViewModels == null)
            {
                ViewModels = new List<GenericViewModel>();

                // Search for potential view models
                var allViewModels = CurrentPluginManager.GetRegisteredObjects<GenericViewModel>();
                foreach (var item in allViewModels)
                {
                    // Set the plugin manager if needed
                    if (item.CurrentApplicationViewModel == null)
                    {
                        item.SetApplicationViewModel(appViewModel);
                    }

                    // Check to see if this would be a valid view model
                    if (item.SupportsObject(Model))
                    {
                        // Create the view model
                        var vm = CurrentPluginManager.CreateNewInstance(item) as GenericViewModel;
                        vm.SetApplicationViewModel(appViewModel);
                        vm.SetModel(Model);
                        ViewModels.Add(vm);

                        RegisterViewModelEventHandlers(vm);
                    }
                }
            }

            return ViewModels;
        }

        /// <summary>
        /// Gets the view model with the given type, creating it if necessary
        /// </summary>
        /// <typeparam name="T">Type of the desired view model</typeparam>
        /// <param name="appViewModel">Instance of the current application ViewModel</param>
        /// <returns>The requested view model, or null if it does not exist</returns>
        public T GetViewModel<T>(ApplicationViewModel appViewModel) where T : GenericViewModel
        {
            var currentViewModels = GetViewModels(appViewModel);
            var desired = currentViewModels.FirstOrDefault(x => x is T);
            if (desired != null)
            {
                return desired as T;
            }
            else
            {
                var newInstance = CurrentPluginManager.CreateInstance(typeof(T)) as GenericViewModel;
                if (newInstance.SupportsObject(Model))
                {
                    newInstance.SetApplicationViewModel(appViewModel);
                    newInstance.SetModel(Model);
                    ViewModels.Add(newInstance);

                    RegisterViewModelEventHandlers(newInstance);
                    return newInstance as T;
                }
                else
                {
                    return null;
                }
            }
        }

        protected void RegisterViewModelEventHandlers(GenericViewModel viewModel)
        {
            // Register event handlers
            if (viewModel is ISavable)
            {
                (viewModel as ISavable).FileSaved += File_OnSaved;
            }
            if (viewModel is INotifyModified)
            {
                (viewModel as INotifyModified).Modified += File_OnModified;
            }
            viewModel.MenuItemRefreshRequested += OnMenuItemRefreshRequested;
        }

        #endregion

        #region Save

        /// <summary>
        /// Determines whether <see cref="Save(PluginManager)"/> can be called.
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>A boolean indicating if <see cref="Save(PluginManager)"/> can be called</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public bool CanSave(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.GetRegisteredObjects<IFileSaver>().Any(x => x.SupportsSave(Model) || // Fall back to SaveAs
                                                                        (Filename != null && x.SupportsSaveAs(Model)));
        }

        /// <summary>
        /// Determines whether <see cref="Save(String, PluginManager)"/> can be called.
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>A boolean indicating if <see cref="Save(String, PluginManager)"/> can be called</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public bool CanSaveAs(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.GetRegisteredObjects<IFileSaver>().Any(x => x.SupportsSaveAs(Model));
        }

        /// <summary>
        /// Saves the current file
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public async Task Save(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            ForceViewModelRefresh();

            var saver = manager.GetRegisteredObjects<IFileSaver>().FirstOrDefault(x => x.SupportsSave(Model));
            if (saver == null)
            {
                // Can't find saver that supports saving without filename; use explicit overload
                await Save(Filename, manager);
            }
            else
            {
                await saver.Save(Model, manager.CurrentFileSystem);
            }
            IsFileModified = false;
        }

        /// <summary>
        /// Saves the file to the given path
        /// </summary>
        /// <param name="filename">Path of the destination file</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filename"/> or <paramref name="manager"/> is null</exception>
        public async Task Save(string filename, PluginManager manager)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            ForceViewModelRefresh();
            var saver = manager.GetRegisteredObjects<IFileSaver>().First(x => x.SupportsSaveAs(Model)); // First (instead of FirstOrDefault) is intentional, save shouldn't be called if this is not true
            await saver.Save(Model, filename, manager.CurrentFileSystem);
            IsFileModified = false;
        }

        /// <summary>
        /// Gets the default extension for the file when using <see cref="Save(String, PluginManager)"/>
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public string GetDefaultExtension(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.GetRegisteredObjects<IFileSaver>().Where(x => x.SupportsSaveAs(Model)).Select(x => x.GetDefaultExtension(Model)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the supported extensions for the file when using <see cref="Save(String, PluginManager)"/>
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null</exception>
        public IEnumerable<string> GetSupportedExtensions(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.GetRegisteredObjects<IFileSaver>().Where(x => x.SupportsSaveAs(Model)).Select(x => x.GetSupportedExtensions(Model)).FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// The action to be executed when the file is closed
        /// </summary>
        protected virtual void CloseAction(object parameter)
        {
            CloseCommandExecuted?.Invoke(this, new EventArgs());
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (DisposeOnClose)
                    {
                        // Dispose of the underlying model if applicable
                        (_model as IDisposable)?.Dispose();
                    }                    

                    // Dispose of view models
                    ResetViewModels();

                    // Set the model property to null to remove event handlers
                    Model = null;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
