using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Represents a class that can be opened from an instance of <see cref="GenericFile"/>
    /// </summary>
    public interface IOpenableFromGenericFile
    {
        Task OpenFile(GenericFile file);
    }
}
