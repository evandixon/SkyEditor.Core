using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// An I/O provider that can be mounted with the <see cref="IMountableIOProvider"/>
    /// </summary>
    /// <remarks>Implementing this interface says that the class knows how to accept file paths that start with a Sky Editor mount (customText:[normalPath])</remarks>
    public interface IMountableIOProvider : IIOProvider
    {
    }
}
