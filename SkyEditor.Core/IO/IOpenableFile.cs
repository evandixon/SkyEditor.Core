using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Represents a class that can be opened from a file on disk
    /// </summary>
    public interface IOpenableFile
    {
        Task OpenFile(string filename, IFileSystem provider);
    }
}
