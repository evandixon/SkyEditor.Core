using SkyEditor.Core.IO;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// Represents a class that can detect whether or not it is the same type as the given file
    /// </summary>
    public interface IDetectableFileType
    {
        /// <summary>
        /// Returns whether or not the given file is of the type that the class represents
        /// </summary>
        /// <param name="file">File to check</param>
        /// <returns>A boolean indicating whether or not the given file can be represented by the current instance of <see cref="IDetectableFileType"/></returns>
        Task<bool> IsOfType(GenericFile file);
    }
}