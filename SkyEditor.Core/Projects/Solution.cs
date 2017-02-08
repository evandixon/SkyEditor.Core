using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;

namespace SkyEditor.Core.Projects
{
    public class Solution : ProjectBase<Project>
    {
        protected override Task<IOnDisk> LoadProjectItem(ItemValue item)
        {
            throw new NotImplementedException();
        }
    }
}
