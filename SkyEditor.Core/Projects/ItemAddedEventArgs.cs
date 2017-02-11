using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class ItemAddedEventArgs : EventArgs
    {
        public ItemAddedEventArgs(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Path of the newly-created directory
        /// </summary>
        public string Path { get; set; }
    }
}
