using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class ProjectFileAddedEventArgs : EventArgs
    {
        /// <summary>
        /// The file that was added, or null if it will not be loaded until requested
        /// </summary>
        public object File { get; set; }

        /// <summary>
        /// Name of the file.
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// Physical path of the newly added file.
        /// </summary>
        public string PhysicalPath { get; set; }
    }
}
