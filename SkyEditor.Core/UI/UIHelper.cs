using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using SkyEditor.Core.Utilities;

namespace SkyEditor.Core.UI
{
    public static class UIHelper
    {
        /// <summary>
        /// Gets the currently registered MenuActions in heiarchy form.
        /// </summary>
        /// <param name="isContextBased">Whether or not the desired menu item info is for context menus.</param>
        /// <param name="isDevMode">Whether or not to get the dev-only menu items.</param>
        /// <param name="target">Target of the menu item, or null if there is no target</param>
        /// <param name="pluginManager">Instance of the current application ViewModel.</param>
        /// <returns>A list of <see cref="MenuItemInfo"/> with each item's <see cref="MenuItemInfo.Children"/> correctly initialized</returns>
        private static async Task<List<MenuItemInfo>> GetMenuItemInfo(bool isContextBased, bool isDevMode, object target, ApplicationViewModel appViewModel)
        {
            if (appViewModel == null)
            {
                throw (new ArgumentNullException(nameof(appViewModel)));
            }

            var menuItems = new List<MenuItemInfo>();
            foreach (var actionInstance in appViewModel.CurrentPluginManager.GetRegisteredObjects<MenuAction>())
            {
                actionInstance.CurrentApplicationViewModel = appViewModel;

                //1: If this is a context menu, only get actions that support the target and are context based
                //2: Ensure menu actions are only visible based on their environment: non-context in regular menu, context in context menu
                //3: DevOnly menu actions are only supported if we're in dev mode.
                if ((!isContextBased || (await actionInstance.SupportsObject(target) && actionInstance.IsContextBased)) &&
                    (isContextBased == actionInstance.IsContextBased) &&
                    (isDevMode || !actionInstance.DevOnly))
                {

                    // Generate the MenuItem
                    if (actionInstance.ActionPath.Count >= 1)
                    {
                        // Find or Create parent menu items
                        var parent = menuItems.Where(x => x.Header == actionInstance.ActionPath[0]);

                        // - Find or create root parent
                        MenuItemInfo current = null;
                        if (parent.Any())
                        {
                            // Find
                            current = parent.First();
                            if (current.ActionTypes.Count == 0)
                            {
                                // Sort order of parent menu items depends on current menu action
                                current.SortOrder = Math.Min(current.SortOrder, actionInstance.SortOrder);
                            }
                        }
                        else
                        {
                            // Create
                            var m = new MenuItemInfo();
                            m.Header = actionInstance.ActionPath[0];
                            m.Children = new List<MenuItemInfo>();
                            m.ActionTypes = new List<TypeInfo>();
                            m.SortOrder = actionInstance.SortOrder;
                            if (actionInstance.ActionPath.Count == 1)
                            {
                                m.ActionTypes.Add(actionInstance.GetType().GetTypeInfo());
                            }
                            menuItems.Add(m);
                            current = m;
                        }

                        // - Find or create non-root parents
                        for (var i = 1; i <= actionInstance.ActionPath.Count - 2; i++)
                        {
                            var index = i; //To avoid potential issues with using the below linq expression.  Might not be needed, but it's probably best to avoid potential issues.
                            parent = from MenuItemInfo m in current.Children where m.Header == actionInstance.ActionPath[index] select m;
                            if (parent.Any())
                            {
                                // Find
                                current = parent.First();
                                if (current.ActionTypes.Count == 0)
                                {
                                    // Sort order of parent menu items depends on current menu action
                                    current.SortOrder = Math.Min(current.SortOrder, actionInstance.SortOrder);
                                }
                            }
                            else
                            {
                                // Create
                                MenuItemInfo m = new MenuItemInfo();
                                m.Header = actionInstance.ActionPath[i];
                                m.Children = new List<MenuItemInfo>();
                                m.SortOrder = actionInstance.SortOrder;
                                if (i == 0)
                                {
                                    menuItems.Add(m);
                                }
                                else
                                {
                                    current.Children.Add(m);
                                }
                                current = m;
                            }
                        }

                        // Find or create the desired menu item
                        if (actionInstance.ActionPath.Count > 1)
                        {
                            // Check to see if the menu item exists
                            parent = current.Children.Where(x => x.Header == actionInstance.ActionPath.Last());

                            if (parent.Any())
                            {
                                // Add action to existing menu item
                                var m = parent.First();
                                m.ActionTypes = new List<TypeInfo>();
                                m.ActionTypes.Add(actionInstance.GetType().GetTypeInfo());
                            }
                            else
                            {
                                // Create the menu item, and give it a proper tag
                                MenuItemInfo m = new MenuItemInfo();
                                m.Children = new List<MenuItemInfo>();
                                m.Header = actionInstance.ActionPath.Last();
                                m.SortOrder = actionInstance.SortOrder;
                                m.ActionTypes = new List<TypeInfo>();
                                m.ActionTypes.Add(actionInstance.GetType().GetTypeInfo());
                                current.Children.Add(m);
                            }
                        }

                    }
                    else //Count=0
                    {
                        throw (new ArgumentException(Properties.Resources.UI_ErrorActionMenuPathEmpty));
                    }
                }
            }
            return menuItems;
        }

        /// <summary>
        /// Gets the currently registered MenuActions in heiarchy form
        /// </summary>
        /// <param name="isDevMode">Whether or not to get the dev-only menu items</param>
        /// <param name="appViewModel">Instance of the current application ViewModel</param>
        /// <returns>A list of <see cref="MenuItemInfo"/> with each item's <see cref="MenuItemInfo.Children"/> correctly initialized</returns>
        public static async Task<List<MenuItemInfo>> GetMenuItemInfo(ApplicationViewModel appViewModel, bool isDevMode = false)
        {
            return await GetMenuItemInfo(false, isDevMode, null, appViewModel);
        }
        /// <summary>
        /// Gets the currently registered MenuActions in heiarchy form.
        /// </summary>
        /// <param name="isDevMode">Whether or not to get the dev-only menu items.</param>
        /// <param name="target">Target of the menu item, or null if there is no target</param>
        /// <param name="appViewModel">Instance of the current application ViewModel</param>
        /// <returns>A list of <see cref="MenuItemInfo"/> with each item's <see cref="MenuItemInfo.Children"/> correctly initialized</returns>
        public static async Task<List<MenuItemInfo>> GetContextMenuItemInfo(object target, ApplicationViewModel appViewModel, bool isDevMode = false)
        {
            return await GetMenuItemInfo(true, isDevMode, target, appViewModel);
        }

        /// <summary>
        /// Generates MenuItems from the given IEnumerable of MenuItemInfo.
        /// </summary>
        /// <param name="menuItemInfo">IEnumerable of MenuItemInfo that will be used to create the MenuItems.</param>
        /// <param name="appViewModel">Instance of the current application ViewModel</param>
        /// <param name="targets">Direct targets of the action, if applicable.  If Nothing, the IOUIManager will control the targets</param>
        /// <returns>A list of <see cref="ActionMenuItem"/> corresponding to the given menu item info (<paramref name="menuItemInfo"/>)</returns>
        public static List<ActionMenuItem> GenerateLogicalMenuItems(IEnumerable<MenuItemInfo> menuItemInfo, ApplicationViewModel appViewModel, IEnumerable<object> targets)
        {
            if (menuItemInfo == null)
            {
                throw (new ArgumentNullException(nameof(menuItemInfo)));
            }
            if (appViewModel == null)
            {
                throw new ArgumentNullException(nameof(appViewModel));
            }
            if (targets == null)
            {
                targets = new object[] { };
            }

            List<ActionMenuItem> output = new List<ActionMenuItem>();

            // Create the menu items
            foreach (var item in from m in menuItemInfo orderby m.SortOrder, m.Header select m)
            {
                var m = new ActionMenuItem();
                m.Header = item.Header;
                m.CurrentApplicationViewModel = appViewModel;
                m.ContextTargets = targets;
                foreach (var action in item.ActionTypes)
                {
                    var a = ReflectionHelpers.CreateInstance(action) as MenuAction;
                    a.CurrentApplicationViewModel = appViewModel;
                    m.Actions.Add(a);
                }
                foreach (var child in GenerateLogicalMenuItems(item.Children, appViewModel, targets))
                {
                    m.Children.Add(child);
                }
                output.Add(m);
            }

            return output;
        }

        /// <summary>
        /// Returns a new instance of each registered view control.
        /// </summary>
        /// <returns>An enumerable of view controls</returns>
        private static IEnumerable<IViewControl> GetViewControls(PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            return manager.GetRegisteredObjects<IViewControl>();
        }

        /// <summary>
        /// Gets a view control that can edit the given object
        /// </summary>
        /// <param name="viewModel">The ViewModel that the view control will target</param>
        /// <returns>A single view control for the given view model</returns>
        public static IViewControl GetViewControl(object viewModel, IEnumerable<Type> requestedTabTypes, ApplicationViewModel appViewModel)
        {
            return GetViewControlTabs(viewModel, requestedTabTypes, appViewModel).FirstOrDefault();
        }

        /// <summary>
        /// Gets view controls that edit the given ViewModel.
        /// </summary>
        /// <param name="model">Object the IObjectControl should edit.</param>
        /// <param name="requestedTabTypes">Limits what types of iObjectControl should be returned.  If the iObjectControl is not of any type in this IEnumerable, it will not be used.  If empty or nothing, no constraints will be applied, which is not recommended because the iObjectControl could be made for a different environment (for example, a Windows Forms user control being used in a WPF environment).</param>
        /// <returns>An enumerable of view controls that target the given view model</returns>
        public static IEnumerable<IViewControl> GetViewControlTabs(object viewModel, IEnumerable<Type> requestedTabTypes, ApplicationViewModel appViewModel)
        {
            if (appViewModel.GetViewModelsForModel(viewModel).Any())
            {
                // Use the new method
                return GetViewControlsByViewModel(viewModel, requestedTabTypes, appViewModel);
            }
            else
            {
                // Use the legacy method
                return GetViewControlsByView(viewModel, requestedTabTypes, appViewModel);
            }
        }

        /// <summary>
        /// Returns a list of IObjectControl that edit the given ObjectToEdit.
        /// </summary>
        /// <param name="model">Object the IObjectControl should edit.</param>
        /// <param name="requestedTabTypes">Limits what types of iObjectControl should be returned.  If the iObjectControl is not of any type in this IEnumerable, it will not be used.  If empty or nothing, no constraints will be applied, which is not recommended because the iObjectControl could be made for a different environment (for example, a Windows Forms user control being used in a WPF environment).</param>
        /// <param name="manager">Instance of the current plugin manager.</param>
        /// <returns>An IEnumerable of object controls for the given model.</returns>
        /// <remarks>This version of <see cref="GetRefreshedTabs(Object, IEnumerable(Of Type), PluginManager)"/> searches for view models, then finds paths to the views.</remarks>
        private static IEnumerable<IViewControl> GetViewControlsByViewModel(object model, IEnumerable<Type> requestedTabTypes, ApplicationViewModel appViewModel)
        {
            Dictionary<object, List<IViewControl>> targetTabs = new Dictionary<object, List<IViewControl>>();
            var modelType = model.GetType().GetTypeInfo();

            List<object> viewModels = new List<object>();
            viewModels.AddRange(appViewModel.GetViewModelsForModel(model));
            viewModels.Add(model); // We'll consider the model itself a view model, if anything directly targets it

            // Get all tabs that could be used for the model, given the RequestedTabTypes constraint
            var availableTabs = from viewModel in viewModels
                                from view in GetViewControls(appViewModel.CurrentPluginManager)
                                let viewModelSortOrder = viewModel is GenericViewModel ? ((viewModel as GenericViewModel).SortOrder) : 0
                                where requestedTabTypes.Any(x => ReflectionHelpers.IsOfType(view, x.GetTypeInfo())) &&
                                view.GetSupportedTypes().Any(x => ReflectionHelpers.IsOfType(viewModel.GetType().GetTypeInfo(), x)) &&
                                view.SupportsObject(viewModel)
                                orderby viewModelSortOrder, view.GetSortOrder(modelType, true)
                                select new { viewModel, view, viewModelSortOrder = viewModel is GenericViewModel ? ((viewModel as GenericViewModel).SortOrder) : 0 };

            var realTabs = availableTabs.Where(x => !x.view.GetIsBackupControl());
            foreach (var item in realTabs)
            {
                if (!targetTabs.ContainsKey(item.viewModel))
                {
                    targetTabs.Add(item.viewModel, new List<IViewControl>());
                }
                targetTabs[item.viewModel].Add(item.view);
            }

            // Find the supported backup controls
            foreach (var item in availableTabs.Where(x => x.view.GetIsBackupControl()).Select(x => x.viewModel).Distinct())
            {
                if (!realTabs.Where(x => x.view.SupportsObject(item)).Any())
                {
                    var usableBackup = availableTabs.Where(x => x.view.SupportsObject(item)).OrderByDescending(x => x.view.GetSortOrder(item.GetType().GetTypeInfo(), true)).FirstOrDefault();
                    if (usableBackup != null)
                    {
                        if (!targetTabs.ContainsKey(item))
                        {
                            targetTabs.Add(item, new List<IViewControl>());
                        }
                        targetTabs[item].Add(usableBackup.view);
                    }
                }
            }

            // Create new instances and set targets
            var newTabs = new List<IViewControl>();
            foreach (var item in targetTabs)
            {
                foreach (var view in item.Value)
                {
                    var tab = ReflectionHelpers.CreateNewInstance(view) as IViewControl;
                    tab.SetApplicationViewModel(appViewModel);

                    //Set the appropriate object
                    tab.ViewModel = item.Key;

                    newTabs.Add(tab);
                }
            }

            return newTabs;
        }

        /// <summary>
        /// Returns a list of view controls that edit the given ViewModel.
        /// </summary>
        /// <param name="viewModel">ViewModel for which to find view controls</param>
        /// <param name="requestedTabTypes">Limits what types of iObjectControl should be returned.  If the iObjectControl is not of any type in this IEnumerable, it will not be used.  If empty or nothing, no constraints will be applied, which is not recommended because the iObjectControl could be made for a different environment (for example, a Windows Forms user control being used in a WPF environment).</param>
        /// <param name="appViewModel">Instance of the current application ViewModel.</param>
        /// <returns>An IEnumerable of object controls for the given model.</returns>
        /// <remarks>This version of <see cref="GetViewControls(Object, IEnumerable(Of Type), PluginManager)"/> searches for views, then finds paths to the model.</remarks>
        private static IEnumerable<IViewControl> GetViewControlsByView(object viewModel, IEnumerable<Type> RequestedTabTypes, ApplicationViewModel appViewModel)
        {
            if (viewModel == null)
            {
                throw (new ArgumentNullException(nameof(viewModel)));
            }

            var modelType = viewModel.GetType().GetTypeInfo();
            var allTabs = new List<IViewControl>();
            var objControls = GetViewControls(appViewModel.CurrentPluginManager);

            foreach (var etab in objControls.Where(x => RequestedTabTypes.Any(y => ReflectionHelpers.IsOfType(x, y.GetTypeInfo()))).OrderBy(x => x.GetSortOrder(modelType, true)))
            {

                etab.SetApplicationViewModel(appViewModel);
                bool isMatch = false;
                GenericViewModel currentViewModel = null;

                //Check to see if the tab support the type of the given object
                var supportedTypes = etab.GetSupportedTypes();

                foreach (var typeInfo in supportedTypes)
                {
                    if (typeInfo.IsInterface)
                    {
                        //The target is an interface.  Check to see if there's a view model that implements it.
                        //Otherwise, check the model

                        //Get the view model for the model from the IOUI mangaer
                        var viewmodelsForModel = appViewModel.GetViewModelsForModel(viewModel);

                        //If there are none, and the model is a FileViewModel, get the view models that way
                        if (ReferenceEquals(viewmodelsForModel, null) && viewModel is FileViewModel)
                        {
                            viewmodelsForModel = (viewModel as FileViewModel).GetViewModels(appViewModel);
                        }

                        //If we still can't find anything, set viewModelsForModel to an empty enumerable
                        if (ReferenceEquals(viewmodelsForModel, null))
                        {
                            viewmodelsForModel = Array.Empty<GenericViewModel>();
                        }

                        // Of the view models that support the model, select the ones that the current view supports
                        var availableViewModels = viewmodelsForModel.Where(x => ReflectionHelpers.IsOfType(x, typeInfo));

                        if (availableViewModels != null && availableViewModels.Any())
                        {
                            // This view model fits the critera
                            var first = availableViewModels.First();
                            isMatch = etab.SupportsObject(first);
                            currentViewModel = first;
                            if (isMatch)
                            {
                                break;
                            }
                        }
                        else if (ReflectionHelpers.IsOfType(viewModel, typeInfo))
                        {
                            // The model implements this interface
                            isMatch = etab.SupportsObject(viewModel);
                            if (isMatch)
                            {
                                break;
                            }
                        }

                    }
                    else if (ReflectionHelpers.IsOfType(viewModel, typeInfo))
                    {
                        // The model is the same type as the target
                        isMatch = etab.SupportsObject(viewModel);
                        if (isMatch)
                        {
                            break;
                        }

                    }
                    else if (ReflectionHelpers.IsOfType(typeInfo, typeof(GenericViewModel).GetTypeInfo()))
                    {
                        // The object control is targeting a view model

                        // First, check to see if there's any view models for this model (i.e., is this an open file?)
                        var viewmodelsForModel = appViewModel.GetViewModelsForModel(viewModel);

                        if (ReferenceEquals(viewmodelsForModel, null) && viewModel is FileViewModel)
                        {
                            viewmodelsForModel = (viewModel as FileViewModel).GetViewModels(appViewModel);
                        }

                        //If there are, check to see if the target view supports the view model
                        if (viewmodelsForModel != null)
                        {
                            var potentialViewModel = viewmodelsForModel.FirstOrDefault(x => ReflectionHelpers.IsOfType(x, typeInfo)) as GenericViewModel;
                            if (potentialViewModel != null)
                            {
                                //This view model supports our model
                                isMatch = etab.SupportsObject(potentialViewModel);
                                currentViewModel = potentialViewModel;
                                if (isMatch)
                                {
                                    break;
                                }
                            }
                        }

                    }
                }

                // This is a supported tab.  We're adding it!
                if (isMatch)
                {
                    // Create another instance of etab, since etab is our cached, search-only instance.
                    var tab = ReflectionHelpers.CreateNewInstance(etab) as IViewControl;
                    tab.SetApplicationViewModel(appViewModel);

                    // Set the appropriate object
                    if (currentViewModel != null)
                    {
                        //We have a view model that the view wants
                        tab.ViewModel = currentViewModel;
                    }
                    else if (tab.SupportsObject(viewModel))
                    {
                        // This model is what the view wants
                        tab.ViewModel = viewModel;
                    }

                    allTabs.Add(tab);
                }
            }

            var backupTabs = new List<IViewControl>();
            var notBackup = new List<IViewControl>();

            //Sort the backup vs non-backup tabs
            foreach (var item in allTabs)
            {
                if (item.GetIsBackupControl())
                {
                    backupTabs.Add(item);
                }
                else
                {
                    notBackup.Add(item);
                }
            }

            //And use the non-backup ones if available
            if (notBackup.Count > 0)
            {
                return notBackup;
            }
            else
            {
                var toUse = backupTabs.OrderBy(x => x.GetSortOrder(modelType, true)).FirstOrDefault();
                if (toUse == null)
                {
                    return Array.Empty<IViewControl>();
                }
                else
                {
                    return new[] { toUse };
                }
            }
        }
    }
}
