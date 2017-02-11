using SkyEditor.Core.Projects;
using SkyEditor.Core.UI;
using System;

namespace SkyEditor.Core.UI
{
    public class FileOpenedEventArguments : EventArgs
    {

        public FileOpenedEventArguments()
        {
            DisposeOnExit = false;
        }

        /// <summary>
        /// The model representing the file that was added.
        /// </summary>
        public object File { get; set; }

        /// <summary>
        /// The <see cref="UI.FileViewModel"/> wrapping <see cref="File"/>.
        /// </summary>
        public FileViewModel FileViewModel { get; set; }

        /// <summary>
        /// Whether or not the file will be disposed when closed.
        /// </summary>
        public bool DisposeOnExit { get; set; }

        /// <summary>
        /// The project the file was opened from.
        /// Null if the file is not in a project.
        /// </summary>
        public Project ParentProject { get; set; }

    }

}