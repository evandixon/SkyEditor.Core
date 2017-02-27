using SkyEditor.Core.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.TestComponents
{
    public class TextPreprocessorSolution : Solution
    {
        public override async Task Initialize()
        {
            await base.Initialize();

            await AddNewProject("/", "Text Preprocessor Project", typeof(TextPreprocessorProject), CurrentPluginManager);
        }
    }
}
