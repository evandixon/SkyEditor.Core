using System.Reflection;
using SkyEditor.Core.Utilities;
using System.Threading.Tasks;
using System;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Opens files using file types that implement <see cref="IOpenableFile"/>
    /// </summary>
    public class OpenableFileOpener : IFileOpener
    {
        public OpenableFileOpener(PluginManager pluginManager)
        {
            CurrentPluginManager = pluginManager;
        }

        private PluginManager CurrentPluginManager { get; }

        /// <summary>
        /// Creates an instance of given type representing the file located at the given path
        /// </summary>
        /// <param name="fileType">Type of the file</param>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">I/O provider containing the file</param>
        /// <returns>The newly opened file</returns>
        public async Task<object> OpenFile(TypeInfo fileType, string filename, IIOProvider provider)
        {
            if (fileType == null)
            {
                throw new ArgumentNullException(nameof(fileType));
            }
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (!ReflectionHelpers.IsOfType(fileType, typeof(IOpenableFile).GetTypeInfo()))
            {
                throw new ArgumentException(string.Format(Properties.Resources.Reflection_ErrorInvalidType, nameof(IOpenableFile)));
            }

            var file = CurrentPluginManager.CreateInstance(fileType) as IOpenableFile;
            await file.OpenFile(filename, provider);
            return file;
        }

        public bool SupportsType(TypeInfo fileType)
        {
            return ReflectionHelpers.IsOfType(fileType, typeof(IOpenableFile).GetTypeInfo());
        }

        public int GetUsagePriority(TypeInfo fileType)
        {
            return 0;
        }
    }
}