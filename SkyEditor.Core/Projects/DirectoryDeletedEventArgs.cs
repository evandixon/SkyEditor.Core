using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class DirectoryDeletedEventArgs : EventArgs
    {
        public DirectoryDeletedEventArgs(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Path of the newly-deleted directory
        /// </summary>
        public string Path { get; set; }
    }
}
