using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    /// <summary>
    /// Represents a project that cannot be loaded due to an invalid type
    /// </summary>
    public class UnsupportedProject : Project
    {
        public UnsupportedProject()
        {
        }

        public UnsupportedProject(UnsupportedProjectBase unsupportedBase)
        {
            Settings = unsupportedBase.Settings;
            Filename = unsupportedBase.Filename;
            Name = unsupportedBase.Name;
        }
    }
}
