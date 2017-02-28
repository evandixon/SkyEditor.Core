using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.TestComponents
{
    /// <summary>
    /// A project that replaces variable placeholders in text files
    /// </summary>
    public class TextPreprocessorProject : Project
    {
        public virtual string GetOutputDirectory()
        {
            return Path.Combine(GetRootDirectory(), "output");
        }

        public override async Task Load()
        {
            await base.Load();

            // Add the variables file if it does not exist
            if (!ItemExists("/variables.txt"))
            {               
                await CreateFile("/", "variables.txt", typeof(TextFile));

                var variablesFile = await GetFile("/variables.txt", IOHelper.PickFirstDuplicateMatchSelector, CurrentPluginManager) as TextFile;
                variablesFile.CreateFile("variables.txt");
                variablesFile.Contents = "# Define variables.  Each line should be in the form \"Variable=Value\".";
                await variablesFile.Save(Path.Combine(GetRootDirectory(), "variables.txt"), CurrentPluginManager.CurrentIOProvider);
            }

            // Add the files folder if it does not exist
            if (!DirectoryExists("/files"))
            {
                CreateDirectory("/files");
            }

            // Create the output physical directory
            CurrentPluginManager.CurrentIOProvider.CreateDirectory(GetOutputDirectory());
        }

        public override async Task Build()
        {
            var outputDirectory = GetOutputDirectory();

            // Parse the variables
            var variables = (await GetFile("/variables.txt", IOHelper.PickFirstDuplicateMatchSelector, CurrentPluginManager) as TextFile)
                                    .Contents
                                    .Split('\n')
                                    .Where(x => !x.StartsWith("#"))
                                    .Select(x => x.Trim().Split("=".ToCharArray(), 2).Select(y => y.Trim()).ToArray());

            // Build the files
            foreach (var filename in GetFiles("/files", "*.*", false))
            {
                var file = await GetFile(filename, IOHelper.PickFirstDuplicateMatchSelector, CurrentPluginManager);
                var outputPath = Path.Combine(outputDirectory, filename.Replace("/files/", ""));
                var contents = (file as TextFile).Contents;
                foreach (var variable in variables)
                {
                    contents = contents.Replace($"%{variable[0]}%", variable[1]);
                }
                CurrentPluginManager.CurrentIOProvider.WriteAllText(outputPath, contents);
            }

            await base.Build();
        }
    }
}
