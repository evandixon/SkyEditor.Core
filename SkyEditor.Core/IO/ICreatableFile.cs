using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Represents an object that can create a new file
    /// </summary>
    /// <remarks>
    /// The flow:
    /// - Object is created
    /// - <see cref="CreateFile(string)"/> is called
    /// </remarks>
    public interface ICreatableFile : IOnDisk, ISavable, INamed, IOpenableFile
    {
        /// <summary>
        /// Initializes the class to represent a new file
        /// </summary>
        /// <param name="name">Name of the file</param>
        void CreateFile(string name);
    }
}
