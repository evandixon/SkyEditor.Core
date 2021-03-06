﻿using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using SkyEditor.Core.Utilities;
using SkyEditor.IO.FileSystem;
using SkyEditor.Utilities.AsyncFor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class BuildCommand : ConsoleCommand
    {
        public BuildCommand(IFileSystem FileSystem, PluginManager pluginManager)
        {
            CurrentFileSystem = FileSystem ?? throw new ArgumentNullException(nameof(FileSystem));
            CurrentPluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        }

        protected IFileSystem CurrentFileSystem { get; }
        protected PluginManager CurrentPluginManager { get; }

        public override async Task MainAsync(string[] arguments)
        {
            var solutionFiles = new List<string>();
            if (arguments.Length > 1)
            {
                if (CurrentFileSystem.FileExists(arguments[1]))
                {
                    solutionFiles.Add(arguments[1]);
                }
            }
            else
            {
                foreach (var file in CurrentFileSystem.GetFiles("./", "*.skysln", true))
                {
                    solutionFiles.Add(file);
                }
            }

            var progress = new ProgressReportToken();
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                var baseProgress = i / solutionFiles.Count;
                progress.Progress = baseProgress;

                using (var solution = await Solution.OpenProjectFile(solutionFiles[i], CurrentPluginManager))
                {
                    if (solution.CanBuild)
                    {
                        void onProgressChanged(object sender, ProgressReportedEventArgs e)
                        {
                            progress.Progress = baseProgress + (e.Progress / solutionFiles.Count);
                        }

                        solution.ProgressChanged += onProgressChanged;

                        await solution.Build();
                    }
                    else
                    {
                        Console.WriteLine(string.Format(Properties.Resources.Console_Build_SolutionNotBuildable, Path.GetFileName(solutionFiles[i])));
                    }
                }
            }
        }
    }
}
