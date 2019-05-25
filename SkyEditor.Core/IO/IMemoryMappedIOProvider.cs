using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SkyEditor.Core.IO
{
    public interface IMemoryMappedFileSystem : IFileSystem
    {
        MemoryMappedFile OpenMemoryMappedFile(string filename);
    }
}
