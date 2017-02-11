using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Extensions.Online
{
    /// <summary>
    /// The response of an extension server
    /// </summary>
    public class RootCollectionResponse
    {
        public string Name { get; set; }
        public List<ExtensionCollectionModel> ChildCollections { get; set; }
        public int ExtensionCount { get; set; }
        public string GetExtensionListEndpoint { get; set; }
        public string DownloadExtensionEndpoint { get; set; }
    }
}
