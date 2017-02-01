using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// View model for a specific kind of model
    /// </summary>
    public abstract class GenericViewModel
    {
        public GenericViewModel()
        {
        }

        public GenericViewModel(object model, PluginManager manager)
        {
            SetPluginManager(manager);
            SetModel(model);
        }

        #region Events

        /// <summary>
        /// Raised when the view model requests the menu items in the <see cref="IOUIManager"/> to be refreshed
        /// </summary>
        public event EventHandler MenuItemRefreshRequested;

        /// <summary>
        /// Requests menu items in the <see cref="IOUIManager"/> to be refreshed
        /// </summary>
        protected void RequestMenuItemRefresh()
        {
            MenuItemRefreshRequested?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Properties

        /// <summary>
        /// The underlying model
        /// </summary>
        public virtual object Model { get; set; }

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public virtual PluginManager CurrentPluginManager { get; set; }

        /// <summary>
        /// An integer used to sort a list of sibling view models with the same model
        /// </summary>
        public virtual int SortOrder => 0;
        #endregion

        #region Functions

        #region Metadata
        /// <summary>
        /// Gets an IEnumerable of every type that the view model is programmed to handle
        /// </summary>
        public abstract IEnumerable<TypeInfo> GetSupportedTypes();

        /// <summary>
        /// Determines whether or not the view model supports the given object
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>A boolean indicating whether or not the view model supports the given object</returns>
        public virtual bool SupportsObject(object obj)
        {
            var currentType = obj.GetType().GetTypeInfo();
            return GetSupportedTypes().Any(x => ReflectionHelpers.IsOfType(currentType, x));
        }

        #endregion

        #region Sibling ViewModels
        /// <summary>
        /// Determines whether or not a view model of the given type is loaded for the same model as <see cref="Model"/>
        /// </summary>
        /// <typeparam name="T">Type of the desired view model</typeparam>
        /// <returns>A boolean indicating whether or not a view model of the desired type exists with the same model as <see cref="Model"/></returns>
        public bool HasSiblingViewModel<T>() where T : GenericViewModel
        {
            var siblings = CurrentPluginManager.CurrentIOUIManager.GetViewModelsForModel(Model);
            return siblings?.Any(x => x is T) ?? false;
        }

        /// <summary>
        /// Gets a view model with the same model as <see cref="Model"/>
        /// </summary>
        /// <typeparam name="T">Type of the desired view model</typeparam>
        /// <param name="throwOnError">Whether or not to throw an exception if the view model does not exist.</param>
        /// <returns>An instance of a view model with the desired type that has the same model as <see cref="Model"/>, or null if <paramref name="throwOnError"/> is false and no such view model could be found</returns>
        public T GetSiblingViewModel<T>(bool throwOnError = true) where T : GenericViewModel
        {
            var siblings = CurrentPluginManager.CurrentIOUIManager.GetViewModelsForModel(Model);
            if (siblings == null)
            {
                if (throwOnError)
                {
                    throw new KeyNotFoundException(Properties.Resources.UI_ErrorCantLoadSiblingViewModels);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var target = siblings.FirstOrDefault(x => x is T);
                if (target == null)
                {
                    if (throwOnError)
                    {
                        throw new KeyNotFoundException(string.Format(Properties.Resources.UI_ErrorNoSiblingViewModelOfType, typeof(T).FullName));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return target as T;
                }
            }
        }

#endregion

        #region Current Model/Manager

        /// <summary>
        /// Sets the current plugin manager
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager</param>
        public virtual void SetPluginManager(PluginManager manager)
        {
            this.CurrentPluginManager = manager;
        }

        /// <summary>
        /// Sets the view model's model to the given object
        /// </summary>
        /// <param name="model">New model for the view model</param>
        public virtual void SetModel(object model)
        {
            this.Model = model;
        }

        /// <summary>
        /// Updates the given model's properties with any unsaved changes
        /// </summary>
        /// <param name="model">The model to update</param>
        public virtual void UpdateModel(object model)
        {
        }
        #endregion

        #endregion


    }

    /// <summary>
    /// View model for an instance of <see cref="T"/>
    /// </summary>
    public abstract class GenericViewModel<T> : GenericViewModel
    {
        public GenericViewModel()
        {
        }

        public GenericViewModel(T model, PluginManager manager) : base(model, manager)
        {
        }

        new public T Model
        {
            get
            {
                return (T)base.Model;
            }
            set
            {
                base.Model = value;
            }
        }

        public override IEnumerable<TypeInfo> GetSupportedTypes()
        {
            return new TypeInfo[] { typeof(T).GetTypeInfo() };
        }
    }
}
