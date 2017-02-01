using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Represents an object with a user-friendly name
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }
}
