using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;

namespace SkyEditor.Core.Projects
{
    /// <summary>
    /// Represents a project that cannot be loaded due to an invalid type
    /// </summary>
    public class UnsupportedProject : ProjectBase
    {
        public override string ProjectFileExtension => Project.ProjectFileExt;

        protected override Task<IOnDisk> LoadProjectItem(ItemValue item)
        {
            return Task.FromResult<IOnDisk>(item);
        }
    }
}
