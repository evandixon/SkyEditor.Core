using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.Extensions;
using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using SkyEditor.Core.UI;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Creates an instance of the <see cref="IIOProvider"/> for the application environment.
        /// </summary>
        /// <returns>An instance of the <see cref="IIOProvider"/> for the application environment.</returns>
        public abstract IIOProvider GetIOProvider();

        /// <summary>
        /// Creates an instance of the <see cref="ISettingsProvider"/> for the application environment.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns>An instance of the <see cref="ISettingsProvider"/> for the application environment.</returns>
        public virtual ISettingsProvider GetSettingsProvider(PluginManager manager)
        {
            return new SettingsProvider(manager);
        }

        /// <summary>
        /// Creates an instance of <see cref="IConsoleProvider"/> for the application environment.
        /// </summary>
        /// <returns>An instance of the <see cref="IConsoleProvider"/> for the application environment.</returns>
        public virtual IConsoleProvider GetConsoleProvider()
        {
            return new DummyConsoleProvider();
        }

        /// <summary>
        /// Creates an instance of <see cref="IOUIManager"/> for the application environment.
        /// </summary>
        /// <param name="manager">Instance of the current plugin manager.</param>
        public virtual ApplicationViewModel GetIOUIManager(PluginManager manager)
        {
            return new ApplicationViewModel(manager);
        }

        /// <summary>
        /// Gets the full path of the directory inside the current IO provider where extensions are stored.
        /// </summary>
        /// <returns>tThe full path of the directory inside the current IO provider where extensions are stored.</returns>
        public abstract string GetExtensionDirectory();

        /// <summary>
        /// Gets whether or not to enable dynamicaly loading plugins.
        /// </summary>
        /// <returns>A boolean indicating whether or not plugin loading is enabled.</returns>
        public virtual bool IsPluginLoadingEnabled()
        {
            return false;
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
            throw new NotSupportedException();
        }

        public override void Load(PluginManager manager)
        {
            base.Load(manager);

            manager.RegisterTypeRegister<IFileOpener>();
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
            manager.RegisterType<IFileTypeDetector, DetectableFileTypeDetector>();

            manager.RegisterTypeRegister<IOpenableFile>();
            manager.RegisterTypeRegister<ICreatableFile>();
            manager.RegisterTypeRegister<IDetectableFileType>();

            manager.RegisterType<IOpenableFile, TextFile>();
            manager.RegisterType<ICreatableFile, TextFile>();
            manager.RegisterType<IDetectableFileType, TextFile>();

            manager.RegisterIOFilter("*.txt", Properties.Resources.File_TextFile);
        }
    }
}
