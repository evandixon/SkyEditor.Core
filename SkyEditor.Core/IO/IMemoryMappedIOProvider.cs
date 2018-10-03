using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SkyEditor.Core.IO
{
    public interface IMemoryMappedIOProvider : IIOProvider
    {
        MemoryMappedFile OpenMemoryMappedFile(string filename);
    }
}
