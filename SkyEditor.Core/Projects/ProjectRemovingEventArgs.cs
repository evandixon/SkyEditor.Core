using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class ProjectRemovingEventArgs : EventArgs
    {

        /// <summary>
        /// Path of the project in the solution
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The project that was removed
        /// </summary>
        public Project Project { get; set; }
    }
}
