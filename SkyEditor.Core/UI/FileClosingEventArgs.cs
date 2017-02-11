using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.UI
{
    public class FileClosingEventArgs : EventArgs
    {

        /// <summary>
        /// Whether or not to cancel the close
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// The file being closed
        /// </summary>
        public object File { get; set; }
    }
}
