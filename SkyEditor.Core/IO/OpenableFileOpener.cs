using System.Reflection;
using SkyEditor.Core.Utilities;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Opens files using file types that implement IOpenableFile
    /// </summary>
    public class OpenableFileOpener : IFileOpener
    {

        public async Task<object> OpenFile(TypeInfo fileType, string filename, IIOProvider provider)
        {
            var file = ReflectionHelpers.CreateInstance(fileType) as IOpenableFile;
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