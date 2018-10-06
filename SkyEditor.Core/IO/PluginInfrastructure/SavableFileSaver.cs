using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.PluginInfrastructure
{
    public class SavableFileSaver : IFileSaver
    {
        public string GetDefaultExtension(object model)
        {
            return (model as ISavableAs)?.GetDefaultExtension();
        }

        public IEnumerable<string> GetSupportedExtensions(object model)
        {
            return (model as ISavableAs)?.GetSupportedExtensions();
        }

        public async Task Save(object model, IIOProvider provider)
        {
            var savable = model as ISavable;
            if (savable != null)
            {
                await savable.Save(provider);
            }
        }

        public async Task Save(object model, string filename, IIOProvider provider)
        {
            var savable = model as ISavableAs;
            if (savable != null)
            {
                await savable.Save(filename, provider);
            }
        }

        public bool SupportsSave(object model)
        {
            if (model is ISavableAs && model is IOnDisk)
            {
                return !string.IsNullOrEmpty((model as IOnDisk).Filename);
            }
            else
            {
                return model is ISavable;
            }            
        }

        public bool SupportsSaveAs(object model)
        {
            return model is ISavableAs;
        }
    }
}
