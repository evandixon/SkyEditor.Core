using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class ProjectErrorReportedEventArgs : EventArgs
    {
        public ErrorInfo ErrorInfo { get; set; }
    }
}
