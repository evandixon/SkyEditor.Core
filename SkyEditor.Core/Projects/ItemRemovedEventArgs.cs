using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class ItemRemovedEventArgs : EventArgs
    {
        public ItemRemovedEventArgs(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Path of the newly-removed item
        /// </summary>
        public string Path { get; set; }
    }
}
