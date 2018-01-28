using SkyEditor.Core.Projects;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class SolutionCommands : ConsoleCommand
    {
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
                            solutionType = ReflectionHelpers.GetTypeByName(arguments[3], CurrentApplicationViewModel.CurrentPluginManager).AsType();
                        }
                        else
                        {
                            solutionType = typeof(Solution);
                        }

                        CurrentApplicationViewModel.CurrentSolution = await ProjectBase.CreateProject<Solution>(CurrentIOProvider.WorkingDirectory, arguments[2], solutionType, CurrentApplicationViewModel.CurrentPluginManager);

                        if (CurrentApplicationViewModel.CurrentSolution.RequiresInitializationWizard)
                        {
                            await CurrentApplicationViewModel.CurrentSolution.InitializationWizard.RunInConsole(CurrentApplicationViewModel.CurrentConsoleShell, true, CurrentIOProvider);
                        }

                        break;
                    case "open":
                        if (arguments.Length > 2)
                        {
                            CurrentApplicationViewModel.CurrentSolution = await ProjectBase.OpenProjectFile<Solution>(arguments[2], CurrentApplicationViewModel.CurrentPluginManager);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        break;
                    case "save":
                        if (CurrentApplicationViewModel.CurrentSolution != null)
                        {
                            await CurrentApplicationViewModel.CurrentSolution.Save(CurrentApplicationViewModel.CurrentIOProvider);
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
