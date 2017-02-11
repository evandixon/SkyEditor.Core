using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Extensions
{
    public enum ExtensionInstallResult
    {
        /// <summary>
        /// The extension was installed successfully
        /// </summary>
        Success,

        /// <summary>
        /// The extension will be installed successfully after the application is restarted
        /// </summary>
        RestartRequired,

        /// <summary>
        /// The provided extension file does not contain the correct information
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// The extension type is not supported
        /// </summary>
        /// <remarks>
        /// Each extension has a specific type.  In some cases, the extension type is supplied by a plugin.  If the type is not supported, it could be that a required plugin is missing
        /// </remarks>
        UnsupportedFormat,

        /// <summary>
        /// The extension is not supported on the current platform (For example, this is the status when trying to install a WPF UI plugin on Android).
        /// </summary>
        IncompatiblePlatform
    }
}
