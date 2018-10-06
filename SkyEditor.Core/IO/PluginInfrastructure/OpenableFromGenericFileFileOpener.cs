using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// Opens files using file types that implement <see cref="IOpenableFromGenericFile"/>
    /// </summary>
    public class OpenableFromGenericFileFileOpener : IFileFromGenericFileOpener
    {
        public OpenableFromGenericFileFileOpener(PluginManager pluginManager)
        {
            CurrentPluginManager = pluginManager;
        }

        private PluginManager CurrentPluginManager { get; }

        /// <summary>
        /// Creates an instance of given type representing the file located at the given path
        /// </summary>
        /// <param name="fileType">Type of the file</param>
        /// <param name="file">Data source of the file</param>
        /// <returns>The newly opened file</returns>
        public async Task<object> OpenFile(TypeInfo fileType, GenericFile file)
        {
            if (fileType == null)
            {
                throw new ArgumentNullException(nameof(fileType));
            }
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!ReflectionHelpers.IsOfType(fileType, typeof(IOpenableFromGenericFile).GetTypeInfo()))
            {
                throw new ArgumentException(string.Format(Properties.Resources.Reflection_ErrorInvalidType, nameof(IOpenableFromGenericFile)));
            }

            var model = CurrentPluginManager.CreateInstance(fileType) as IOpenableFromGenericFile;
            await model.OpenFile(file);
            return model;
        }

        public bool SupportsType(TypeInfo fileType)
        {
            return ReflectionHelpers.IsOfType(fileType, typeof(IOpenableFromGenericFile).GetTypeInfo());
        }

        public int GetUsagePriority(TypeInfo fileType)
        {
            return 1;
        }
    }
}
