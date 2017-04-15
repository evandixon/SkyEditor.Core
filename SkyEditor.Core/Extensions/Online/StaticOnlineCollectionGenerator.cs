using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Extensions.Online
{
    /// <summary>
    /// Generates JSON files that can serve as an Online Extension Collection when served on a static-serve HTTP server
    /// </summary>
    public class StaticOnlineCollectionGenerator
    {
        private class Extension
        {
            public Guid ID { get; set; }
            public Guid CollectionID { get; set; }
            public string ExtensionID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Author { get; set; }
        }

        private class ExtensionVersion
        {
            public Guid ID { get; set; }
            public string Version { get; set; }
            public Guid ExtensionID { get; set; }
        }

        public static async Task Generate(string baseEndpoint, string collectionName, IEnumerable<string> inputExtensions, string outputDirectory, IIOProvider provider)
        {
            if (!baseEndpoint.Replace("\\", "/").EndsWith("/"))
            {
                baseEndpoint = baseEndpoint + "/";
            }

            var extensions = new List<Extension>();
            var extensionVersions = new List<ExtensionVersion>();

            var extensionsDir = Path.Combine(outputDirectory, "Extensions");

            // Create the output directory
            provider.CreateDirectoryIfNotExists(outputDirectory);

            // Generate the collection
            var response = new RootCollectionResponse()
            {
                Name = collectionName,
                ChildCollections = new List<ExtensionCollectionModel>(),
                DownloadExtensionEndpoint = baseEndpoint + "Extensions",
                GetExtensionListEndpoint = baseEndpoint + "Extensions/extensions.json",
                EnablePaging = false
            };

            // Generate the extensions & copy files
            foreach (var item in inputExtensions)
            {
                // Read the info file
                string infoContents;
                using (var archive = provider.OpenFile(item))
                {
                    using (var zip = new ZipArchive(archive, ZipArchiveMode.Create))
                    {
                        var entry = zip.GetEntry("info.skyext");
                        using (var entryStream = entry.Open())
                        {
                            using (var reader = new System.IO.StreamReader(entryStream))
                            {
                                infoContents = await reader.ReadToEndAsync();
                            }
                        }
                    }
                }

                // Parse the info file
                if (!string.IsNullOrEmpty(infoContents))
                {
                    var infoFile = ExtensionInfo.Deserialize(infoContents);

                    // Create or select the extension
                    var extension = extensions.Where(x => x.ExtensionID == infoFile.ID).FirstOrDefault();
                    if (extension == null)
                    {
                        extension = new Extension { ID = Guid.NewGuid(), ExtensionID = infoFile.ID, Name = infoFile.Name, Description = infoFile.Description, Author = infoFile.Author };
                        extensions.Add(extension);
                    }

                    // Create or select version info
                    var version = extensionVersions.Where(x => x.ExtensionID == extension.ID && x.Version == infoFile.Version).FirstOrDefault();
                    if (version == null)
                    {
                        version = new ExtensionVersion { ID = Guid.NewGuid(), Version = infoFile.Version, ExtensionID = extension.ID };
                        extensionVersions.Add(version);
                    }

                    // Ensure target directory exists
                    var extensionDir = Path.Combine(extensionsDir, "files", extension.ExtensionID);
                    provider.CreateDirectoryIfNotExists(extensionDir);

                    // Copy the file
                    File.Copy(item, Path.Combine(extensionDir, version.Version + ".zip"));
                }
            }

            // Save extension JSON
            var extensionInfos = (from e in extensions
                                  let v = extensionVersions.Where(x => x.ExtensionID == e.ID).Select(x => x.Version)
                                  orderby e.Name
                                  select new OnlineExtensionInfo
                                  {
                                      ID = e.ExtensionID,
                                      Name = e.Name,
                                      Description = e.Description,
                                      Author = e.Author,
                                      AvailableVersions = v.ToList()
                                  }).ToList();
            Json.SerializeToFile(Path.Combine(extensionsDir, "extensions.json"), extensionInfos, provider);

            // Save the collection
            response.ExtensionCount = extensionInfos.Count;
            Json.SerializeToFile(Path.Combine(outputDirectory, "collection.json"), response, provider);
        }
    }
}
