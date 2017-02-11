using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Extensions.Online
{
    /// <summary>
    /// Version of <see cref="ExtensionInfo"/> containing extra information about extensions in online collections
    /// </summary>
    public class OnlineExtensionInfo : ExtensionInfo
    {
        public List<string> AvailableVersions { get; set; }
    }
}
