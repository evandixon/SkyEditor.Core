using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions.Online
{
    /// <summary>
    /// A collection of extensions stored online.
    /// </summary>
    public class OnlineExtensionCollection : IExtensionCollection
    {

        /// <summary>
        /// Creates a new instance of <see cref="OnlineExtensionCollection"/>.
        /// </summary>
        /// <param name="rootEndpoint">The root endpoint for connecting to the collection.</param>
        /// <remarks>The api expects something like the following endpoints:
        /// api/ExtensionCollection
        /// api/ExtensionCollection/5
        /// The endpoint name can vary, but the "/&lt;parentCollectionID&gt;" part must hold true.</remarks>
        public OnlineExtensionCollection(string rootEndpoint)
        {
            this.Client = new HttpClient();
            this.RootEndpoint = rootEndpoint;
            this.CachedInfo = new Dictionary<int, OnlineExtensionInfo>();
        }

        public OnlineExtensionCollection(string rootEndpoint, string parentCollectionId) : this(rootEndpoint)
        {
            this.ParentCollectionId = parentCollectionId;
        }

        private HttpClient Client { get; set; }
        private string RootEndpoint { get; set; }
        private string ParentCollectionId { get; set; }
        private string GetExtensionsEndpoint { get; set; }
        private Dictionary<int, OnlineExtensionInfo> CachedInfo { get; set; }

        private async Task<RootCollectionResponse> GetResponse()
        {
            if (ReferenceEquals(_response, null))
            {
                var endpoint = RootEndpoint;
                if (!string.IsNullOrEmpty(ParentCollectionId))
                {
                    endpoint += "/" + ParentCollectionId;
                }
                _response = Json.Deserialize<RootCollectionResponse>(await Client.GetStringAsync(new Uri(endpoint)).ConfigureAwait(false));
            }
            return _response;
        }
        private RootCollectionResponse _response;

        public async Task<string> GetName()
        {
            if (string.IsNullOrEmpty(_name))
            {
                _name = System.Convert.ToString((await GetResponse().ConfigureAwait(false)).Name);
            }
            return _name;
        }
        string _name;

        public async Task<IEnumerable<IExtensionCollection>> GetChildCollections(PluginManager manager)
        {
            if (ReferenceEquals(_childCollections, null))
            {
                _childCollections = new List<OnlineExtensionCollection>();
                foreach (var item in (await GetResponse().ConfigureAwait(false)).ChildCollections)
                {
                    _childCollections.Add(new OnlineExtensionCollection(this.RootEndpoint, item.ID));
                }
            }
            return _childCollections;
        }
        List<OnlineExtensionCollection> _childCollections;

        public async Task<int> GetExtensionCount(PluginManager manager)
        {
            if (!_extensionCount.HasValue)
            {
                _extensionCount = (await GetResponse().ConfigureAwait(false)).ExtensionCount;
            }
            return _extensionCount.Value;
        }
        int? _extensionCount;

        public async Task<IEnumerable<ExtensionInfo>> GetExtensions(int skip, int take, PluginManager manager)
        {
            var responseRaw = await Client.GetStringAsync((await GetResponse().ConfigureAwait(false)).GetExtensionListEndpoint + $"/{skip}/{take}");
            var response = Json.Deserialize<List<OnlineExtensionInfo>>(responseRaw);

            int i = skip;
            foreach (var item in response)
            {
                item.IsInstalled = ExtensionHelper.IsExtensionInstalled(item, manager);

                if (CachedInfo.ContainsKey(i))
                {
                    CachedInfo[i] = item;
                }
                else
                {
                    CachedInfo.Add(i, item);
                }
                i++;
            }

            return response;
        }

        public async Task<ExtensionInstallResult> InstallExtension(string extensionID, string version, PluginManager manager)
        {
            //Download zip
            var tempName = manager.CurrentIOProvider.GetTempFilename();
            manager.CurrentIOProvider.WriteAllText(tempName, await Client.GetStringAsync((await GetResponse().ConfigureAwait(false)).DownloadExtensionEndpoint + $"/{extensionID}/{version}"));

            //Install
            var result = await ExtensionHelper.InstallExtensionZip(tempName, manager).ConfigureAwait(false);

            //Clean up
            manager.CurrentIOProvider.DeleteFile(tempName);

            return result;
        }

        public Task<ExtensionUninstallResult> UninstallExtension(string extensionID, PluginManager manager)
        {
            string typeName = CachedInfo.Values.Where(x => x.ID == extensionID).Select(x => x.ExtensionTypeName).First();
            return ExtensionHelper.UninstallExtension(typeName, extensionID, manager);
        }
    }
}
