using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    /// <summary>
    /// Settings specific to certian builds or distributions
    /// </summary>
    /// <remarks>
    /// The official Team City CI server may overwrite any values set here.
    /// </remarks>
    internal static class BuildSettings
    {
        /// <summary>
        /// The endpoint for the official extension collection
        /// </summary>
        public const string DefaultExtensionCollection = "http://localhost/SkyEditorExtensions/";
    }
}
