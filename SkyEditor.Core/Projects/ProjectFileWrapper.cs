using SkyEditor.Core.IO;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Projects
{
    public class ProjectFileWrapper
    {
        /// <summary>
        /// Creates a new instance of <see cref="ProjectFileWrapper"/>
        /// </summary>
        /// <param name="projectFilename">Path of the project file</param>
        /// <param name="filename">Path of the file, relative to the project directory</param>
        public ProjectFileWrapper(string projectFilename, string filename)
        {
            this.ProjectFilename = projectFilename;
            this.Filename = filename;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ProjectFileWrapper"/>
        /// </summary>
        /// <param name="projectFilename">Path of the project file</param>
        /// <param name="filename">Path of the file, relative to the project directory</param>
        /// <param name="fileTypeAssemblyQualifiedName">Assembly-qualified name of the type of the file</param>
        public ProjectFileWrapper(string projectFilename, string filename, string fileTypeAssemblyQualifiedName) : this(projectFilename, filename)
        {
            this.FileAssemblyQualifiedTypeName = fileTypeAssemblyQualifiedName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ProjectFileWrapper"/>
        /// </summary>
        /// <param name="projectFilename">Path of the project file</param>
        /// <param name="filename">Path of the file, relative to the project directory</param>
        /// <param name="file">An object representing the file</param>
        public ProjectFileWrapper(string projectFilename, string filename, object file) : this(projectFilename, filename)
        {
            this.FileAssemblyQualifiedTypeName = file.GetType().AssemblyQualifiedName;
            this.File = file;
        }

        /// <summary>
        /// The contained file
        /// </summary>
        private object File { get; set; }

        /// <summary>
        /// Assembly-qualified name of the type of the file
        /// </summary>
        public string FileAssemblyQualifiedTypeName { get; set; }

        /// <summary>
        /// Path of the file, relative to the project directory
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Path of the project containing the file
        /// </summary>
        public string ProjectFilename { get; set; }

        /// <summary>
        /// Gets the full path of the file
        /// </summary>
        /// <returns>A string containing the full path of the file</returns>
        public string GetFullPath()
        {
            return Path.Combine(Path.GetDirectoryName(ProjectFilename), Filename.TrimStart('\\'));
        }

        public async Task<object> GetFile(PluginManager manager, IOHelper.DuplicateMatchSelector duplicateMatchSelector)
        {
            if (File == null)
            {
                var path = GetFullPath();
                if (string.IsNullOrEmpty(FileAssemblyQualifiedTypeName))
                {
                    File = await IOHelper.OpenFile(path, duplicateMatchSelector, manager);
                }
                else
                {
                    var type = ReflectionHelpers.GetTypeByName(FileAssemblyQualifiedTypeName, manager);
                    if (type == null)
                    {
                        File = await IOHelper.OpenFile(path, duplicateMatchSelector, manager);
                    }
                    else
                    {
                        File = await IOHelper.OpenFile(path, type, manager);
                    }
                }
            }
            return File;
        }
    }
}
