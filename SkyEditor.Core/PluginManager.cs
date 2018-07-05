using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.Extensions;
using SkyEditor.Core.IO;
using SkyEditor.Core.Settings;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    public class PluginManager : IDisposable
    {
        public PluginManager()
        {
            TypeInstanceCache = new Dictionary<TypeInfo, object>();
            TypeRegistry = new Dictionary<TypeInfo, List<TypeInfo>>();
            PluginAssemblies = new List<Assembly>();
            Plugins = new List<SkyEditorPlugin>();
            FailedPluginLoads = new List<string>();
            DependantPlugins = new Dictionary<SkyEditorPlugin, List<SkyEditorPlugin>>();
            DependantPluginLoadingQueue = new Queue<SkyEditorPlugin>();
            IOFilters = new Dictionary<string, string>();
            RegisteredSingletons = new Dictionary<Type, Func<PluginManager, object>>();
            RegisteredTransients = new Dictionary<Type, Func<PluginManager, object>>();
            InitializedSingletons = new Dictionary<Type, object>();

            // Everything created by this PluginManager gets to have a reference to it
            AddSingletonDependency(pluginManager => pluginManager);
        }

        #region Properties

        /// <summary>
        /// Cache of shared instances of classes.
        /// Used for classes in <see cref="TypeRegistry"/> to read metadata without contstantly creating new instances.
        /// </summary>
        protected Dictionary<TypeInfo, object> TypeInstanceCache { get; set; }

        /// <summary>
        /// The core of the plugin manager: matches base types or interfaces to types that inherit or implement them
        /// </summary>
        protected Dictionary<TypeInfo, List<TypeInfo>> TypeRegistry { get; set; }

        /// <summary>
        /// Assembly containing the core plugin
        /// </summary>
        protected Assembly CorePluginAssembly { get; set; }

        /// <summary>
        /// List of all assemblies containing loaded plugins.
        /// </summary>
        protected List<Assembly> PluginAssemblies { get; set; }

        /// <summary>
        /// List of all loaded plugins
        /// </summary>
        protected List<SkyEditorPlugin> Plugins { get; set; }

        /// <summary>
        /// List of plugins that were unable to be loaded
        /// </summary>
        protected List<string> FailedPluginLoads { get; set; }

        /// <summary>
        /// Matches plugins to the plugins that depend on that plugin.
        /// </summary>
        /// <remarks>
        /// Key: Plugin that is manually loaded by each plugin in the value.
        /// Value: List of plugins that manually load the key
        /// </remarks>
        protected Dictionary<SkyEditorPlugin, List<SkyEditorPlugin>> DependantPlugins { get; set; }

        /// <summary>
        /// Plugins to be loaded that have been requested by other plugins.
        /// This is only used during plugin loading.
        /// </summary>
        protected Queue<SkyEditorPlugin> DependantPluginLoadingQueue { get; set; }

        /// <summary>
        /// Location of extensions within the current IO provider.
        /// </summary>
        public string ExtensionDirectory { get; set; }

        /// <summary>
        /// Filters used in Open and Save dialogs.  Key: Extension, Value: Friendly name
        /// </summary>
        public Dictionary<string, string> IOFilters { get; private set; }

        /// <summary>
        /// The current IO Provider for the application.
        /// </summary>
        public IIOProvider CurrentIOProvider { get; protected set; }

        /// <summary>
        /// The current Settings Provider for the applicaiton.
        /// </summary>
        public ISettingsProvider CurrentSettingsProvider { get; protected set; }

        /// <summary>
        /// The current Console Provider for the application.  This is the abstraction layer between the console and the application.
        /// </summary>
        public IConsoleProvider CurrentConsoleProvider { get; protected set; }

        private Dictionary<Type, Func<PluginManager, object>> RegisteredTransients { get; set; }

        private Dictionary<Type, Func<PluginManager, object>> RegisteredSingletons { get; set; }

        private Dictionary<Type, object> InitializedSingletons { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a type is added into the type registry.
        /// </summary>
        public event EventHandler<TypeRegisteredEventArgs> TypeRegistered;

        /// <summary>
        /// Raised before the plugins' Load methods are called.
        /// </summary>
        public event EventHandler PluginsLoading;

        /// <summary>
        /// Raised after all plugins have been loaded.
        /// </summary>
        public event EventHandler PluginLoadComplete;
        #endregion

        #region Plugin Loading

        /// <summary>
        /// Loads the given Core plugin and any other available plugins, if supported by the environment.
        /// </summary>
        /// <param name="core">The core plugin that controls the environment.</param>
        /// <remarks>
        /// Misc things this function does:
        /// - Delete files scheduled for deletion
        /// - Install pending extensions
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="core"/> is null.</exception>
        public virtual async Task LoadCore(CoreSkyEditorPlugin core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core));
            }

            // Load providers
            CurrentIOProvider = core.GetIOProvider();
            CurrentSettingsProvider = core.GetSettingsProvider(this);
            CurrentConsoleProvider = core.GetConsoleProvider();

            // Delete files and directories scheduled for deletion
            await DeleteScheduledFiles(CurrentSettingsProvider, CurrentIOProvider);

            // Install pending extensions
            ExtensionDirectory = core.GetExtensionDirectory();
            await ExtensionHelper.InstallPendingExtensions(ExtensionDirectory, this);

            // Load the provided core
            CorePluginAssembly = core.GetType().GetTypeInfo().Assembly;
            Plugins.Add(core);
            core.Load(this);

            // Load plugins, if enabled
            if (core.IsPluginLoadingEnabled())
            {
                // Get the paths of all plugins to be loaded
                var supportedPlugins = GetPluginPaths();

                // Load the plugin assemblies
                foreach (var item in supportedPlugins)
                {
                    try
                    {
                        var assemblyActual = core.LoadAssembly(item);
                        if (assemblyActual != null)
                        {
                            PluginAssemblies.Add(assemblyActual);
                            foreach (var plg in assemblyActual.DefinedTypes.Where((x) => ReflectionHelpers.IsOfType(x, typeof(SkyEditorPlugin).GetTypeInfo()) && this.CanCreateInstance(x)))
                            {
                                Plugins.Add(this.CreateInstance(plg) as SkyEditorPlugin);
                            }
                        }
                    }
                    catch (FileLoadException)
                    {
                        // The assembly is a bad assembly.  We can continue loading plugins, but not with this
                        FailedPluginLoads.Add(item);
                    }
                    catch (BadImageFormatException)
                    {
                        // The assembly is a bad assembly.  We can continue loading plugins, but not with this
                        FailedPluginLoads.Add(item);
                    }
                    catch (NotSupportedException)
                    {
                        // The current environment does not support loading assemblies this way.
                        // Abort dynamic assembly loading
                        break;
                    }
                }
            }

            // Load logical plugins
            PluginsLoading?.Invoke(this, new EventArgs());

            var coreType = core.GetType();
            foreach (var item in Plugins.Where(p => p.GetType() != core.GetType()))
            {
                item.Load(this);
            }

            // Load dependant plugins
            while (DependantPluginLoadingQueue.Count > 0)
            {
                var item = DependantPluginLoadingQueue.Dequeue();
                var pluginType = item.GetType();

                // Determine if it has already been loaded
                if (!Plugins.Where((x) => x.GetType() == pluginType).Any())
                {
                    // Add the plugin
                    Plugins.Add(item);

                    // Add the assembly if it hasn't been added already
                    var pluginAssembly = pluginType.GetTypeInfo().Assembly;
                    if (!PluginAssemblies.Contains(pluginAssembly))
                    {
                        PluginAssemblies.Add(pluginAssembly);
                    }

                    // Load the plugin
                    item.Load(this);
                }
            }

            // Use reflection to fill the type registry
            if (core.IsCorePluginAssemblyDynamicTypeLoadEnabled())
            {
                LoadTypes(CorePluginAssembly);
            }
            if (core.IsExtraPluginAssemblyDynamicTypeLoadEnabled())
            {
                foreach (var item in PluginAssemblies)
                {
                    LoadTypes(item);
                }
            }

            PluginLoadComplete?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Deletes files and directories scheduled for deletion by the settings provider.
        /// </summary>
        /// <param name="settings"><see cref="ISettingsProvider"/> managing the files and directories scheduled for deletion.</param>
        /// <param name="provider"><see cref="IIOProvider"/> managing where the files and directories are located.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> or <paramref name="provider"/> is null.</exception>
        protected async Task DeleteScheduledFiles(ISettingsProvider settings, IIOProvider provider)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            // Files
            foreach (var item in settings.GetFilesScheduledForDeletion().ToList()) // Create a new list because the original will be continuously modified
            {
                if (CurrentIOProvider.FileExists(item))
                {
                    CurrentIOProvider.DeleteFile(item);
                }
                CurrentSettingsProvider.UnscheduleFileForDeletion(item);
                await CurrentSettingsProvider.Save(provider);
            }

            // Directories
            foreach (var item in settings.GetDirectoriesScheduledForDeletion().ToList())
            {
                if (CurrentIOProvider.DirectoryExists(item))
                {
                    CurrentIOProvider.DeleteDirectory(item);
                }
                CurrentSettingsProvider.UnscheduleDirectoryForDeletion(item);
                await CurrentSettingsProvider.Save(provider);
            }
        }

        /// <summary>
        /// Gets the paths corresponding to all plugin assemblies.
        /// </summary>
        /// <returns>Full paths of all plugin assemblies</returns>
        protected List<string> GetPluginPaths()
        {
            var supportedPlugins = new List<string>();

            // Look at plugin extensions to find plugins
            var pluginExtType = new PluginExtensionType();
            pluginExtType.CurrentPluginManager = this;

            foreach (var item in pluginExtType.GetExtensions(this))
            {
                supportedPlugins.AddRange(item.ExtensionFiles.Select((x) => Path.Combine(pluginExtType.GetExtensionDirectory(item.ID), x)));
            }

            return supportedPlugins;
        }

        /// <summary>
        /// Loads supported types inside the given assembly into the type registry
        /// </summary>
        /// <param name="item">Assembly from which to load types</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
        protected virtual void LoadTypes(Assembly item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            foreach (var actualType in item.DefinedTypes)
            {
                // Check to see if this type inherits from one we're looking for
                foreach (var registeredType in TypeRegistry.Keys)
                {
                    if (ReflectionHelpers.IsOfType(actualType, registeredType))
                    {
                        RegisterType(registeredType, actualType);
                    }
                }

                // Do the same for each interface
                foreach (var i in actualType.ImplementedInterfaces)
                {
                    foreach (var registeredType in TypeRegistry.Keys)
                    {
                        if (ReflectionHelpers.IsOfType(i, registeredType))
                        {
                            RegisterType(registeredType, actualType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads a plugin required by another.
        /// </summary>
        /// <param name="targetPlugin">The plugin to load</param>
        /// <param name="dependantPlugin">The plugin requesting the load</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetPlugin"/> or <paramref name="dependantPlugin"/> is null.</exception>
        public virtual void LoadRequiredPlugin(SkyEditorPlugin pluginToLoad, SkyEditorPlugin dependantPlugin)
        {
            if (pluginToLoad == null)
            {
                throw new ArgumentNullException(nameof(pluginToLoad));
            }

            if (dependantPlugin == null)
            {
                throw new ArgumentNullException(nameof(dependantPlugin));
            }

            // - Create the dependant plugin list if it doesn't exist
            if (!DependantPlugins.ContainsKey(pluginToLoad))
            {
                DependantPlugins.Add(pluginToLoad, new List<SkyEditorPlugin>());
            }

            // - Add the plugin to the dependant plugin list
            if (!DependantPlugins[pluginToLoad].Contains(dependantPlugin))
            {
                DependantPlugins[pluginToLoad].Add(dependantPlugin);
            }

            // Mark this plugin as a dependant, will be loaded by plugin engine later
            // Because loading takes place in a For Each loop iterating through Plugins, we cannot load plugins here, because that would change the collection.
            DependantPluginLoadingQueue.Enqueue(pluginToLoad);
        }

        /// <summary>
        /// Determines whether or not a given assembly is a plugin assembly that is directly loaded by another plugin assembly.
        /// </summary>
        /// <param name="assembly">Assembly to check</param>
        /// <returns>A boolean indicating whether or not the given assembly was loaded directly by another plugin.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> is null.</exception>
        public bool IsAssemblyDependant(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return DependantPlugins.Keys.Any((x) => x.GetType().GetTypeInfo().Assembly.Equals(assembly));
        }

        #endregion

        #region Registration

        /// <summary>
        /// Adds the given type to the type registry.
        /// </summary>
        /// <param name="type">Type of the type register.</param>
        /// <remarks>
        /// After plugins are loaded, any type that inherits or implements the given Type can be easily found.
        /// If the type is already in the type registry, nothing will be done.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public void RegisterTypeRegister(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!TypeRegistry.ContainsKey(type))
            {
                TypeRegistry.Add(type, new List<TypeInfo>());

                // Add the type to its own registry.
                // The function will only add it if an instance can be created.
                // This will happen automatically if assembly searching is enabled,
                // but it needs to happen now to avoid issues where it doesn't exist until later, despite some things expecting it now.
                RegisterType(type, type);
            }
        }

        /// <summary>
        /// Adds the given type to the type registry.
        /// </summary>
        /// <typeparam name="T">Type of the type register.</typeparam>
        /// <remarks>
        /// After plugins are loaded, any type that inherits or implements the given Type can be easily found.
        /// If the type is already in the type registry, nothing will be done.
        /// </remarks>
        public void RegisterTypeRegister<T>()
        {
            RegisterTypeRegister(typeof(T).GetTypeInfo());
        }

        /// <summary>
        /// Registers the given type into the given registry, if possible.
        /// </summary>
        /// <param name="register">The base type or interface that the given Type inherits or implements.</param>
        /// <param name="type">The type to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="register"/> or <paramref name="type"/> is null.</exception>
        public void RegisterType(TypeInfo register, TypeInfo type)
        {
            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // Only register types that can be created
            // This excludes generic types, abstract classes, and interfaces.
            if (this.CanCreateInstance(type))
            {
                // Ensure the register was in fact registered
                RegisterTypeRegister(register);

                // Duplicates can cause minor issues
                if (!TypeRegistry[register].Contains(type))
                {
                    TypeRegistry[register].Add(type);
                }

                TypeRegistered?.Invoke(this, new TypeRegisteredEventArgs { BaseType = register, RegisteredType = type });
            }
        }

        /// <summary>
        /// Registers the given type into the given registry, if possible.
        /// </summary>
        /// <typeparam name="R">The base type or interface that the given Type inherits or implements.</typeparam>
        /// <param name="type">The type to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public void RegisterType<R>(TypeInfo type)
        {
            RegisterType(typeof(R).GetTypeInfo(), type);
        }

        /// <summary>
        /// Registers the given type into the given registry, if possible.
        /// </summary>
        /// <typeparam name="R">The base type or interface that the given Type inherits or implements.</typeparam>
        /// <typeparam name="T">The type to register.</typeparam>
        public void RegisterType<R, T>()
        {
            RegisterType(typeof(R).GetTypeInfo(), typeof(T).GetTypeInfo());
        }

        /// <summary>
        /// Registers a filter for use in open and save file dialogs.
        /// </summary>
        /// <param name="FileExtension">Filter for the dialog.  If this is by extension, should be *.extension</param>
        /// <param name="FileFormatName">Name of the file format</param>
        public void RegisterIOFilter(string fileExtension, string fileFormatName)
        {
            if (!IOFilters.ContainsKey(fileExtension))
            {
                IOFilters.Add(fileExtension, fileFormatName);
            }
        }
        #endregion

        #region Functions

        #region Read Type Registry

        /// <summary>
        /// Gets a cached instance of the given type, creating one if necessary
        /// </summary>
        /// <param name="type">Type of the desired object</param>
        /// <returns>A shared instance of the desired type</returns>
        /// <remarks>This is a shared instance, useful for metadata of specific instances of abstract classes (or other similar things).  If anything unique needs to be done with this, use <see cref="ReflectionHelpers.CreateNewInstance(object)"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null</exception>
        protected object GetCachedInstance(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (TypeInstanceCache.ContainsKey(type))
            {
                return TypeInstanceCache[type];
            }
            else
            {
                var instance = this.CreateInstance(type);
                TypeInstanceCache.Add(type, instance);
                return instance;
            }
        }

        /// <summary>
        /// Gets the registered types that inherit or implement the given base type.
        /// </summary>
        /// <param name="baseType">Type of which to get the children or implementors.</param>
        /// <returns>An IEnumerable of types that inherit or implement the given base type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseType"/> is null</exception>
        public IEnumerable<TypeInfo> GetRegisteredTypes(TypeInfo baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            if (TypeRegistry.ContainsKey(baseType))
            {
                return TypeRegistry[baseType];
            }
            else
            {
                return Enumerable.Empty<TypeInfo>();
            }
        }

        /// <summary>
        /// Gets the registered types that inherit or implement the given base type.
        /// </summary>
        /// <typeparam name="T">Type of which to get the children or implementors.</typeparam>
        /// <returns>An IEnumerable of types that inherit or implement the given base type.</returns>
        public IEnumerable<TypeInfo> GetRegisteredTypes<T>()
        {
            return GetRegisteredTypes(typeof(T).GetTypeInfo());
        }

        /// <summary>
        /// Gets instances of registered types that inherit or implement the given base type.
        /// </summary>
        /// <param name="baseType">Type of which to get the children or implementors.</param>
        /// <returns>An IEnumerable containing shared instances of classes that inherit or implement the given base type.</returns>
        /// <remarks>These are shared instances, useful for metadata of specific instances of abstract classes (or other similar things).  If anything unique needs to be done with this, use <see cref="ReflectionHelpers.CreateNewInstance(object)"/> to create new instances of desired objects.</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseType"/> is null</exception>
        public IEnumerable<object> GetRegisteredObjects(TypeInfo baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            return GetRegisteredTypes(baseType).Where((x) => this.CanCreateInstance(x)).Select((x) => GetCachedInstance(x));
        }

        /// <summary>
        /// Gets instances of registered types that inherit or implement the given base type.
        /// </summary>
        /// <typeparam name="T">Type of which to get the children or implementors.</typeparam>
        /// <returns>An IEnumerable containing shared instances of classes that inherit or implement the given base type.</returns>
        /// <remarks>These are shared instances, useful for metadata of specific instances of abstract classes (or other similar things).  If anything unique needs to be done with this, use <see cref="ReflectionHelpers.CreateNewInstance(object)"/> to create new instances of desired objects.</remarks>

        public IEnumerable<T> GetRegisteredObjects<T>() where T : class
        {
            return GetRegisteredObjects(typeof(T).GetTypeInfo()).Select(x => x as T);
        }
        #endregion

        #region Dependency Injection

        /// <summary>
        /// Registers a type that can be provided to dynamically-created objects in their constructor, created every time it is requested.
        /// </summary>
        /// <typeparam name="TAbstract">Type to be provided to dynamically-created objects</typeparam>
        public void AddTransientDependency<TAbstract, TInstantiate>() where TInstantiate : TAbstract where TAbstract : class
        {
            AddTransientDependency<TAbstract>(_ => this.CreateInstance(typeof(TInstantiate)) as TAbstract);
        }

        /// <summary>
        /// Registers a type that can be provided to dynamically-created objects in their constructor, created every time it is requested.
        /// </summary>
        /// <typeparam name="T">Type to be provided to dynamically-created objects</typeparam>
        /// <param name="constructor">Function that creates the type to be provided</param>
        public void AddTransientDependency<T>(Func<PluginManager, T> constructor) where T : class
        {
            RegisteredTransients.Add(typeof(T), p => constructor(p));
        }

        /// <summary>
        /// Registers a type that can be provided to dynamically-created objects in their constructor, created once then cached.
        /// Creation will take place the first time it is needed
        /// </summary>
        /// <typeparam name="TAbstract">Type to be provided to dynamically-created objects</typeparam>
        /// <param name="constructor">Function that creates the type to be provided</param>
        public void AddSingletonDependency<TAbstract, TInstantiate>() where TInstantiate : TAbstract where TAbstract : class
        {
            AddSingletonDependency<TAbstract>(_ => this.CreateInstance(typeof(TInstantiate)) as TAbstract);
        }

        /// <summary>
        /// Registers a type that can be provided to dynamically-created objects in their constructor, created once then cached.
        /// Creation will take place the first time it is needed.
        /// </summary>
        /// <typeparam name="T">Type to be provided to dynamically-created objects</typeparam>
        /// <param name="constructor">Function that creates the type to be provided</param>
        public void AddSingletonDependency<T>(Func<PluginManager, T> constructor) where T : class
        {
            RegisteredSingletons.Add(typeof(T), p => constructor(p));
        }

        /// <summary>
        /// Registers a type that can be provided to dynamically-created objects in their constructor.
        /// </summary>
        /// <typeparam name="T">Type to be provided to dynamically-created objects</typeparam>
        /// <param name="value">Value to use for the singleton</param>
        public void AddSingletonDependency<T>(T value) where T : class
        {
            if (!InitializedSingletons.ContainsKey(typeof(T)))
            {
                InitializedSingletons.Add(typeof(T), value);
            }
        }

        /// <summary>
        /// Gets a registered singleton or transient dependency, or null if the dependency could not be found
        /// </summary>
        /// <typeparam name="TAbstract">Type of the singleton or dependency</typeparam>
        /// <remarks>
        /// Services will be loaded if exists in this order:
        /// - Initialized singletons
        /// - Uninitialized singletons
        /// - Transients
        /// </remarks>
        public TAbstract GetRequiredDependency<TAbstract>() where TAbstract : class
        {
            var serviceType = typeof(TAbstract);
            return GetRequiredDependency(serviceType) as TAbstract;
        }

        /// <summary>
        /// Gets a registered singleton or transient dependency, or null if the dependency could not be found
        /// </summary>
        /// <typeparam name="TAbstract">Type of the singleton or dependency</typeparam>
        /// <remarks>
        /// Services will be loaded if exists in this order:
        /// - Initialized singletons
        /// - Uninitialized singletons
        /// - Transients
        /// </remarks>
        public object GetRequiredDependency(Type serviceType)
        {
            if (InitializedSingletons.ContainsKey(serviceType))
            {
                return InitializedSingletons[serviceType];
            }
            else if (RegisteredSingletons.ContainsKey(serviceType))
            {
                var instance = RegisteredSingletons[serviceType].Invoke(this);
                InitializedSingletons.Add(serviceType, instance);
                return instance;
            }
            else if (RegisteredTransients.ContainsKey(serviceType))
            {
                return RegisteredTransients[serviceType].Invoke(this);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether the required dependency has been registered
        /// </summary>
        /// <typeparam name="TAbstract">Type of the singleton or dependency</typeparam>
        /// <remarks>
        /// Services will be loaded if exists in this order:
        /// - Initialized singletons
        /// - Uninitialized singletons
        /// - Transients
        /// </remarks>
        public bool HasRequiredDependency<TAbstract>() where TAbstract : class
        {
            var serviceType = typeof(TAbstract);
            return HasRequiredDependency(serviceType);
        }

        /// <summary>
        /// Determines whether the required dependency has been registered
        /// </summary>
        /// <typeparam name="TAbstract">Type of the singleton or dependency</typeparam>
        /// <remarks>
        /// Services will be loaded if exists in this order:
        /// - Initialized singletons
        /// - Uninitialized singletons
        /// - Transients
        /// </remarks>
        public bool HasRequiredDependency(Type serviceType)
        {
            if (InitializedSingletons.ContainsKey(serviceType))
            {
                return true;
            }
            else if (RegisteredSingletons.ContainsKey(serviceType))
            {
                return true;
            }
            else if (RegisteredTransients.ContainsKey(serviceType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether or not <see cref="CreateInstance(TypeInfo)"/> can create an instance of this type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>A boolean indicating whether or not an instance of this type can be created</returns>
        /// <remarks>
        /// Current criteria:
        /// - Type must not be abstract
        /// - Type must have a default constructor</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public bool CanCreateInstance(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return !type.IsAbstract && !type.IsInterface && type.DeclaredConstructors.Any(x =>
                    {
                        var parameters = x.GetParameters();
                        return parameters.Length == 0 || parameters.All(p => HasRequiredDependency(p.ParameterType));
                    });
        }

        /// <summary>
        /// Determines whether or not <see cref="CreateInstance(TypeInfo)"/> can create an instance of this type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>A boolean indicating whether or not an instance of this type can be created</returns>
        /// <remarks>
        /// Current criteria:
        /// - Type must not be abstract
        /// - Type must have a default constructor</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public bool CanCreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return CanCreateInstance(type.GetTypeInfo());
        }

        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="type">Type to be created</param>
        /// <returns>A new object of the given type</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        /// <remarks>The constructor with the most supported parameters will be used</remarks>
        public object CreateInstance(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            foreach (var constructor in type.GetConstructors().OrderByDescending(c => c.GetParameters().Length))
            {
                var parameters = constructor.GetParameters();
                var parameterValues = new List<object>();
                var useConstructor = true;

                foreach (var parameter in parameters)
                {
                    var value = GetRequiredDependency(parameter.ParameterType);
                    
                    if (value == null)
                    {
                        useConstructor = false;
                    }

                    parameterValues.Add(value);
                }

                if (useConstructor)
                {
                    return constructor.Invoke(parameterValues.ToArray());
                }
            }

            return Activator.CreateInstance(type.AsType());
        }

        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="type">Type to be created</param>
        /// <returns>A new object of the given type</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public object CreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return CreateInstance(type.GetTypeInfo());
        }

        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="type">Type to be created</param>
        /// <returns>A new object of the given type</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public T CreateInstance<T>() where T : class
        {
            return CreateInstance(typeof(T)) as T;
        }

        /// <summary>
        /// Creates a new instance of the type of the given object.
        /// </summary>
        /// <param name="target">Instance of the type of which to create a new instance</param>
        /// <returns>A new object with the same type as <paramref name="target"/>.</returns>        
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null.</exception>
        public object CreateNewInstance(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return CreateInstance(target.GetType().GetTypeInfo());
        }


        #endregion

        /// <summary>
        /// Gets all loaded assemblies in the app domain or that the plugin manager knows about
        /// </summary>
        /// <returns>A list of assemblies that are in the app domain or that the plugin manager knows about</returns>
        public virtual List<Assembly> GetLoadedAssemblies()
        {
            // Add plugin assemblies
            var output = PluginAssemblies.ToList();

            // Add current assembly
            var currentAssembly = typeof(PluginManager).GetTypeInfo().Assembly;
            if (!output.Contains(currentAssembly))
            {
                output.Add(currentAssembly);
            }

            // Add core plugin assembly
            if (!output.Contains(CorePluginAssembly))
            {
                output.Add(CorePluginAssembly);
            }

            return output;
        }

        /// <summary>
        /// Gets the currently loaded plugins
        /// </summary>
        /// <returns>A new list of the currently loaded Sky Editor plugins.</returns>
        public List<SkyEditorPlugin> GetPlugins()
        {
            return Plugins.ToList();
        }

        /// <summary>
        /// Instructs all plugins to prepare for distribution
        /// </summary>
        public void PreparePluginsForDistribution()
        {
            foreach (var item in Plugins)
            {
                item.PrepareForDistribution(this);
            }
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
                    // Dispose managed state (managed objects).
                    foreach (var item in Plugins)
                    {
                        item.Unload(this);
                    }

                    foreach (var item in TypeInstanceCache)
                    {
                        (item.Value as IDisposable)?.Dispose();
                    }

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PluginManager() {
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
