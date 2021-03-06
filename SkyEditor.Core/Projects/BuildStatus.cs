﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public enum BuildStatus
    {
        /// <summary>
        /// No build has been run yet.
        /// </summary>
        None,

        /// <summary>
        /// Build has been started, but is not complete.
        /// </summary>
        Building,

        /// <summary>
        /// Build has completed successfully.
        /// </summary>
        Done,

        /// <summary>
        /// Build has completed with errors.
        /// </summary>
        Failed,

        /// <summary>
        /// The build's cancellation has been requested, but the build is still running.
        /// </summary>
        Canceling,

        /// <summary>
        /// The build is stopped because it has been canceled.
        /// </summary>
        Canceled,
    }
}
