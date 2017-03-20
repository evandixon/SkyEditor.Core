using SkyEditor.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO.FileTemplates
{
    public class FileTemplateExtensionCollection : LocalExtensionCollection
    {
        public override string InternalName => "FileTemplates";

        public override Task<string> GetName()
        {
            return Task.FromResult("File Templates");
        }
    }
}
