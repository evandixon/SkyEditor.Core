using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Marks an object that supports raising an event when modified.
    /// </summary>
    public interface INotifyModified
    {
        event EventHandler Modified;
    }
}
