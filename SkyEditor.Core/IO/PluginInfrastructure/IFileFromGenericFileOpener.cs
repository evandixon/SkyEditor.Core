using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    public interface IFileFromGenericFileOpener : IBaseFileOpener
    {
        /// <summary>
        /// Creates an instance of fileType from the given filename
        /// </summary>
        /// <param name="fileType">Type of the file to open</param>
        /// <param name="file">Data source of the file</param>
        /// <returns>An object representing the requested file</returns>
        Task<object> OpenFile(TypeInfo fileType, GenericFile file);
    }
}
