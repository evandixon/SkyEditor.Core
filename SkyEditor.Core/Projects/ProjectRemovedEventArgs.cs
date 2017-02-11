using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class ProjectRemovedEventArgs : EventArgs
    {

        /// <summary>
        /// Path of the project in the solution
        /// </summary>
        public string Path { get; set; }

    }
}
