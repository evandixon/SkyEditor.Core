using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.ConsoleCommands.Commands;
using SkyEditor.Core.ConsoleCommands.ShellCommands;
using SkyEditor.Core.Extensions;
using SkyEditor.Core.IO;
using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.Core.Projects;
using SkyEditor.Core.Settings;
using SkyEditor.Core.UI;
using SkyEditor.Core.Utilities;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    /// <summary>
    /// A variant of SkyEditorPlugin that controls how the plugin manager loads other plugins and provides environment-specific providers.
    /// </summary>
    public abstract class CoreSkyEditorPlugin : SkyEditorPlugin
    {

        #region Environment Paths

        protected virtual string GetRootResourceDirectory()
        {
            string path = "Resources";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        protected virtual string GetSettingsFilename()
        {
            return Path.Combine(GetRootResourceDirectory(), "settings.json");
        }

        #endregion

        /// <summary>
        /// Creates an instance of the <see cref="IFileSystem"/> for the application environment.
        /// </summary>
        /// <returns>An instance of the <see cref="IFileSystem"/> for the application environment.</returns>
        /// <remarks>Defaults to <see cref="PhysicalFileSystem"/> unless overridden.</remarks>
        public virtual IFileSystem GetFileSystem()
        {
            return new PhysicalFileSystem();
        }

        /// <summary>
        /// Creates an instance of the <see cref="ISettingsProvider"/> for the application environment.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns>An instance of the <see cref="ISettingsProvider"/> for the application environment.</returns>
        public virtual ISettingsProvider GetSettingsProvider(PluginManager manager)
        {
            return SettingsProvider.Open(GetSettingsFilename(), manager);
        }

        /// <summary>
        /// Creates an instance of <see cref="IConsoleProvider"/> for the application environment.
        /// </summary>
        /// <returns>An instance of the <see cref="IConsoleProvider"/> for the application environment.</returns>
        public virtual IConsoleProvider GetConsoleProvider()
        {
            return new StandardConsoleProvider();
        }

        /// <summary>
        /// Creates an instance of <see cref="ApplicationViewModel"/> for the application environment.
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager.</param>
        public virtual ApplicationViewModel CreateApplicationViewModel(PluginManager manager)
        {
            return new ApplicationViewModel(manager);
        }

        /// <summary>
        /// Gets a cached instance of <see cref="ApplicationViewModel"/> for the application environment, or creates one if it does not yet exist.
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager.</param>
        public ApplicationViewModel GetApplicationViewModel(PluginManager manager)
        {
            if (_appViewModel == null)
            {
                _appViewModel = CreateApplicationViewModel(manager);
            }
            return _appViewModel;
        }
        private ApplicationViewModel _appViewModel = null;

        /// <summary>
        /// Gets the full path of the directory inside the current IO provider where extensions are stored.
        /// </summary>
        /// <returns>tThe full path of the directory inside the current IO provider where extensions are stored.</returns>
        public virtual string GetExtensionDirectory()
        {
            return Path.Combine(GetRootResourceDirectory(), "Extensions");
        }

        /// <summary>
        /// Gets whether or not to enable dynamicaly loading plugins.
        /// </summary>
        /// <returns>A boolean indicating whether or not plugin loading is enabled.</returns>
        public virtual bool IsPluginLoadingEnabled()
        {
#if NETSTANDARD2_0
            return true;
#elif NET462
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Whether or not types in the core plugin assembly will be dynamically loaded into the type registry
        /// </summary>
        public virtual bool IsCorePluginAssemblyDynamicTypeLoadEnabled()
        {
            return true;
        }

        /// <summary>
        /// Whether or not types in plugin assemblies will be dynamically loaded into the type registry.  This does not include the core plugin (see <see cref="IsCorePluginAssemblyDynamicTypeLoadEnabled"/>)
        /// </summary>
        public virtual bool IsExtraPluginAssemblyDynamicTypeLoadEnabled()
        {
            return true;
        }

        /// <summary>
        /// Loads the assembly located at the given path into the current AppDomain and returns it.
        /// </summary>
        /// <param name="assemblyPath">Full path of the assembly to load.</param>
        /// <returns>The assembly that was loaded.</returns>
        /// <exception cref="NotSupportedException">Thrown when the current platform does not support loading assemblies from a specific path.</exception>
        /// <exception cref="BadImageFormatException">Thrown when the assembly is not a valid .Net assembly.</exception>
        public virtual Assembly LoadAssembly(string assemblyPath)
        {
            return ReflectionHelpers.LoadAssemblyWithDependencies(assemblyPath);
        }

        public override void Load(PluginManager manager)
        {
            base.Load(manager);

            manager.AddSingletonDependency(GetApplicationViewModel(manager));
            manager.AddSingletonDependency(GetFileSystem());
            manager.AddSingletonDependency(GetSettingsProvider(manager));
            manager.AddSingletonDependency(GetConsoleProvider());

            manager.RegisterTypeRegister<IFileOpener>();
            manager.RegisterTypeRegister<IFileFromGenericFileOpener>();
            manager.RegisterTypeRegister<IFileSaver>();
            manager.RegisterTypeRegister<IFileTypeDetector>();
            manager.RegisterTypeRegister<IDirectoryTypeDetector>();
            manager.RegisterTypeRegister<ConsoleCommand>();
            manager.RegisterTypeRegister<LocalExtensionCollection>();
            manager.RegisterTypeRegister<Solution>();
            manager.RegisterTypeRegister<Project>();
            manager.RegisterTypeRegister<MenuAction>();
            manager.RegisterTypeRegister<GenericViewModel>();
            manager.RegisterTypeRegister<IViewControl>();

            manager.RegisterType<IFileOpener, OpenableFileOpener>();
            manager.RegisterType<IFileFromGenericFileOpener, OpenableFromGenericFileFileOpener>();
            manager.RegisterType<IFileTypeDetector, DetectableFileTypeDetector>();
            manager.RegisterType<IFileSaver, SavableFileSaver>();

            // Console Commands
            manager.RegisterType<ConsoleCommand, InstallExtension>();
            manager.RegisterType<ConsoleCommand, ListFiles>();
            manager.RegisterType<ConsoleCommand, ListPlugins>();
            manager.RegisterType<ConsoleCommand, ListProperties>();
            manager.RegisterType<ConsoleCommand, OpenFile>();
            manager.RegisterType<ConsoleCommand, SelectFile>();
            manager.RegisterType<ConsoleCommand, SettingCommand>();
            manager.RegisterType<ConsoleCommand, SolutionCommands>();
            manager.RegisterType<ConsoleCommand, DistPrep>();

            // Shell Console Commands
            manager.RegisterType<ConsoleCommand, cd>();
            manager.RegisterType<ConsoleCommand, ls>();
            manager.RegisterType<ConsoleCommand, mkdir>();

            manager.RegisterTypeRegister<IOpenableFile>();
            manager.RegisterTypeRegister<IOpenableFromGenericFile>();
            manager.RegisterTypeRegister<ICreatableFile>();
            manager.RegisterTypeRegister<IDetectableFileType>();

            manager.RegisterType<IOpenableFile, TextFile>();
            manager.RegisterType<ICreatableFile, TextFile>();
            manager.RegisterType<IDetectableFileType, TextFile>();

            manager.RegisterIOFilter("*.txt", Properties.Resources.File_TextFile);

            if (manager.CurrentSettingsProvider.GetIsDevMode())
            {
                manager.RegisterType<Solution, Solution>();
            }
        }
    }
}
