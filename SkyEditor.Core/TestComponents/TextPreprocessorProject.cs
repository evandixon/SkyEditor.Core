using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.TestComponents
{
    /// <summary>
    /// A project that replaces variable placeholders in text files
    /// </summary>
    public class TextPreprocessorProject : Project
    {
        public override async Task Initialize()
        {
            await base.Initialize();

            // Add the variables file if it does not exist
            if (!ItemExists("/variables.txt"))
            {
                var variablesFile = new TextFile();
                variablesFile.CreateFile("variables.txt");
                variablesFile.Contents = "# Define variables.  Each line should be in the form \"Variable=Value\".";
                await variablesFile.Save(Path.Combine(GetRootDirectory(), "variables.txt"), CurrentPluginManager.CurrentIOProvider);

                AddItem("/variables.txt", variablesFile);
            }

            // Add the files folder if it does not exist
            if (!DirectoryExists("/files"))
            {
                CreateDirectory("/files");
            }
        }
    }
}
