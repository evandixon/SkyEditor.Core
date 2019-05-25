using SkyEditor.Core.IO;
using SkyEditor.Core.Projects;
using SkyEditor.Core.Utilities;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class SolutionCommands : ConsoleCommand
    {
        public SolutionCommands(ApplicationViewModel applicationViewModel, PluginManager pluginManager, IFileSystem provider)
        {
            CurrentApplicationViewModel = applicationViewModel;
            CurrentPluginManager = pluginManager;
            CurrentFileSystem = provider;
        }

        protected ApplicationViewModel CurrentApplicationViewModel { get; }
        protected PluginManager CurrentPluginManager { get; }
        protected IFileSystem CurrentFileSystem { get; }

        public override string CommandName => "Solution";
        public override async Task MainAsync(string[] arguments)
        {
            if (arguments.Length > 1)
            {
                switch (arguments[1].ToLower())
                {
                    case "create":
                        Type solutionType;

                        if (arguments.Length >= 4)
                        {
                            solutionType = ReflectionHelpers.GetTypeByName(arguments[3], CurrentPluginManager).AsType();
                        }
                        else
                        {
                            solutionType = typeof(Solution);
                        }

                        CurrentApplicationViewModel.CurrentSolution = await ProjectBase.CreateProject<Solution>(CurrentFileSystem.WorkingDirectory, arguments[2], solutionType, CurrentPluginManager);

                        if (CurrentApplicationViewModel.CurrentSolution.RequiresInitializationWizard)
                        {
                            var initWizard = CurrentApplicationViewModel.CurrentSolution.GetInitializationWizard();
                            await initWizard.RunInConsole(CurrentApplicationViewModel.CurrentConsoleShell, true);
                        }

                        break;
                    case "open":
                        if (arguments.Length > 2)
                        {
                            var solution = await ProjectBase.OpenProjectFile(arguments[2], CurrentPluginManager) as Solution;
                            if (solution != null)
                            {
                                CurrentApplicationViewModel.CurrentSolution = solution;
                            }
                            else
                            {
                                Console.WriteLine(Properties.Resources.Console_Solution_Open_InvalidType);
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        break;
                    case "save":
                        if (CurrentApplicationViewModel.CurrentSolution != null)
                        {
                            await CurrentApplicationViewModel.CurrentSolution.Save(CurrentFileSystem);
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.Console_Solution_NoneLoaded);
                        }
                        break;
                    default:
                        if (CurrentApplicationViewModel.CurrentSolution != null)
                        {
                            string args;
                            if (arguments.Length > 2)
                            {
                                args = arguments[2];
                            }
                            else
                            {
                                args = "";
                            }
                            
                            await CurrentApplicationViewModel.CurrentConsoleShell.RunCommand(arguments[1], arguments[1] + " " + args, true, CurrentApplicationViewModel.CurrentSolution);
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.Console_Solution_NoneLoaded);
                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.Console_Solution_Usage);
            }
        }
    }
}
