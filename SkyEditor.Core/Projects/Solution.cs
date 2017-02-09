using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;

namespace SkyEditor.Core.Projects
{
    public class Solution : ProjectBase<Project>
    {
        public IEnumerable<Project> GetProjectsByName(string name)
        {
            throw new NotImplementedException();
        }

        protected override Task<IOnDisk> LoadProjectItem(ItemValue item)
        {
            throw new NotImplementedException();
        }
    }
}
