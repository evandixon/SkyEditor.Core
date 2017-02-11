using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Extensions
{
    public enum ExtensionUninstallResult
    {
        /// <summary>
        /// The extension was uninstalled successfully
        /// </summary>
        Success,

        /// <summary>
        /// The extension will be uninstalled successfully after the application is restarted
        /// </summary>
        RestartRequired
    }
}
