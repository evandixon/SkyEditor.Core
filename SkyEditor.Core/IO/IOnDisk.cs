using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Represents an object that is stored on disk
    /// </summary>
    public interface IOnDisk
    {
        string Filename { get; set; }
    }
}
