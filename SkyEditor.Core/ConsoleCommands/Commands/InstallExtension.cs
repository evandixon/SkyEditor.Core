using SkyEditor.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    public class InstallExtension : ConsoleCommand
    {
        public override async Task MainAsync(string[] arguments)
        {
            if (arguments.Length > 1)
            {
                if (CurrentApplicationViewModel.CurrentPluginManager.CurrentIOProvider.FileExists(arguments[1]))
                {
                    var result = await ExtensionHelper.InstallExtensionZip(arguments[1], CurrentApplicationViewModel.CurrentPluginManager).ConfigureAwait(false);
                    if (result == ExtensionInstallResult.Success)
                    {
                        Console.WriteLine("Extension install was successful.");
                    }
                    else if (result == ExtensionInstallResult.RestartRequired)
                    {
                        Console.WriteLine("Application must be restarted to complete installation.");
                    }
                    else if (result == ExtensionInstallResult.InvalidFormat)
                    {
                        Console.WriteLine("The provided zip file is not a Sky Editor extension.");
                    }
                    else if (result == ExtensionInstallResult.UnsupportedFormat)
                    {
                        Console.WriteLine("The provided extension is not supported.  Is this an extension to an extension that's not currently installed?");
                    }
                    else
                    {
                        Console.WriteLine("Unknown error.");
                    }
                }
                else
                {
                    Console.WriteLine("File doesn't exist.");
                }
            }
            else
            {
                Console.WriteLine("Usage: InstallExtension <Filename>");
            }
        }
    }
}
