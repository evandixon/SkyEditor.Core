using System;

namespace SkyEditor.Core.UI
{
    public class FileClosedEventArgs : EventArgs
    {
        public object File { get; set; }
    }
}