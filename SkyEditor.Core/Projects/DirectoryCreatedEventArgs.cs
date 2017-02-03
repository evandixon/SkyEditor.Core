using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class DirectoryCreatedEventArgs : EventArgs
    {
        public DirectoryCreatedEventArgs(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Path of the newly-created directory
        /// </summary>
        public string Path { get; set; }
    }
}
