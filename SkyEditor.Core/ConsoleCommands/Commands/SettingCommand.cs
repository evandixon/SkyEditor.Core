﻿using SkyEditor.Core.IO;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.ConsoleCommands.Commands
{
    /// <summary>
    /// Sets a setting
    /// </summary>
    public class SettingCommand : ConsoleCommand
    {
        public SettingCommand(ISettingsProvider settingsProvider, IFileSystem FileSystem)
        {
            CurrentSettingsProvider = settingsProvider;
            CurrentFileSystem = FileSystem;
        }
        protected ISettingsProvider CurrentSettingsProvider { get; }
        protected IFileSystem CurrentFileSystem { get; }

        public override string CommandName => "setting";
        public override async Task MainAsync(string[] arguments)
        {
            if (arguments.Length >= 3)
            {
                var provider = CurrentSettingsProvider;
                switch (arguments[1].ToLower())
                {
                    case "get":
                        var theSetting = provider.GetSetting(arguments[2]);
                        if (theSetting == null)
                        {
                            Console.WriteLine("null");
                        }
                        else
                        {
                            Console.Write(theSetting);
                            Console.WriteLine($" ({theSetting.GetType().Name})");
                        }                        
                        break;
                    case "set":
                        if (arguments.Length >= 4)
                        {
                            provider.SetSetting(arguments[2], arguments[3]);
                            await provider.Save(CurrentFileSystem);
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.Console_Settings_Usage);
                        }                        
                        break;
                    default:
                        Console.WriteLine(Properties.Resources.Console_Settings_Usage);
                        break;
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.Console_Settings_Usage);
            }
        }
    }
}
