using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    /// <summary>
    /// Detects the type of a file using all registered implementors of <see cref="IDetectableFileType"/>
    /// </summary>
    public class DetectableFileTypeDetector : IFileTypeDetector
    {

        public async Task<IEnumerable<FileTypeDetectionResult>> DetectFileType(GenericFile file, PluginManager manager)
        {
            ConcurrentQueue<FileTypeDetectionResult> matches = new ConcurrentQueue<FileTypeDetectionResult>();
            Utilities.AsyncFor f = new Utilities.AsyncFor();
            f.RunSynchronously = !file.IsThreadSafe;
            await f.RunForEach(manager.GetRegisteredObjects<IDetectableFileType>(), async (x) =>
            {
                if (await x.IsOfType(file))
                {
                    matches.Enqueue(new FileTypeDetectionResult { FileType = x.GetType().GetTypeInfo(), MatchChance = 0.5f });
                }
            });
            return matches;
        }
    }
}