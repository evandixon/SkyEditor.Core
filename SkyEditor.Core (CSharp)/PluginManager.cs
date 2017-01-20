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
        /// Function that can determine whether or not a file of a given length will safely fit in memory.
        /// </summary>
        public Func<long, bool> CanLoadFileInMemoryFunction { get; protected set; }

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

        /// <summary>
        /// The current <see cref="ConsoleManager"/> for the application.  This is the class that handles parsing and executing commands from the console.
        /// </summary>
        public ConsoleManager CurrentConsoleManager { get; protected set; }

        /// <summary>
        /// The current instance of the IO/UI Manager, helping manage open files and their associated UI.
        /// </summary>
        public IOUIManager CurrentIOUIManager { get; protected set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a type is added into the type registry.
        /// </summary>
        public event TypeRegisteredEventHandler TypeRegistered;
        public delegate void TypeRegisteredEventHandler(object sender, TypeRegisteredEventHandler e);

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
            CurrentIOUIManager = core.GetIOUIManager(this);

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
                            foreach (var plg in assemblyActual.DefinedTypes.Where((x) => ReflectionHelpers.IsOfType(x, typeof(SkyEditorPlugin).GetTypeInfo()) && ReflectionHelpers.CanCreateInstance(x)))
                            {
                                Plugins.Add(ReflectionHelpers.CreateInstance(plg));
                            }
                        }
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
            PluginsLoading.Invoke(this, new EventArgs());

            foreach (var item in Plugins)
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
            LoadTypes(CorePluginAssembly);
            foreach (var item in PluginAssemblies)
            {
                LoadTypes(item);
            }
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

            foreach(var item in pluginExtType.GetInstalledExtensions(this))
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
        public virtual void LoadRequiredPlugin(SkyEditorPlugin targetPlugin, SkyEditorPlugin dependantPlugin)
        {
            if (targetPlugin == null)
            {
                throw new ArgumentNullException(nameof(targetPlugin));
            }

            if (dependantPlugin == null)
            {
                throw new ArgumentNullException(nameof(dependantPlugin));
            }

            // - Create the dependant plugin list if it doesn't exist
            if (!DependantPlugins.ContainsKey(targetPlugin))
            {
                DependantPlugins.Add(targetPlugin, new List<SkyEditorPlugin>());
            }

            // - Add the plugin to the dependant plugin list
            if (!DependantPlugins[targetPlugin].Contains(dependantPlugin))
            {
                DependantPlugins[targetPlugin].Add(dependantPlugin);
            }

            // Mark this plugin as a dependant, will be loaded by plugin engine later
            // Because loading takes place in a For Each loop iterating through Plugins, we cannot load plugins here, because that would change the collection.
            DependantPluginLoadingQueue.Enqueue(targetPlugin);
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
            if (ReflectionHelpers.CanCreateInstance(type))
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
                var instance = ReflectionHelpers.CreateInstance(type);
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

            return GetRegisteredTypes(baseType).Where((x) => ReflectionHelpers.CanCreateInstance(x)).Select((x) => GetCachedInstance(x));
        }

        /// <summary>
        /// Gets instances of registered types that inherit or implement the given base type.
        /// </summary>
        /// <typeparam name="T">Type of which to get the children or implementors.</typeparam>
        /// <returns>An IEnumerable containing shared instances of classes that inherit or implement the given base type.</returns>
        /// <remarks>These are shared instances, useful for metadata of specific instances of abstract classes (or other similar things).  If anything unique needs to be done with this, use <see cref="ReflectionHelpers.CreateNewInstance(object)"/> to create new instances of desired objects.</remarks>

        public IEnumerable<object> GetRegisteredObjects<T>()
        {
            return GetRegisteredObjects(typeof(T).GetTypeInfo());
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
