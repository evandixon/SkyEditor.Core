using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using System.Reflection;
using SkyEditor.Core.Utilities;
using System.IO;
using System.Linq;

namespace SkyEditor.Core.Projects
{
    public class Project : ProjectBase<ProjectFileWrapper>
    {
        private const string ProjectReferencesSettingName = "ProjectReferences";

        public const string ProjectFileExt = "skyproj";

        public override string ProjectFileExtension => ProjectFileExt;

        /// <summary>
        /// Raised when a file has been added to the project
        /// </summary>
        public event EventHandler<ProjectFileAddedEventArgs> FileAdded;

        /// <summary>
        /// Solution to which the current project belongs
        /// </summary>
        public Solution ParentSolution { get; set; }

        /// <summary>
        /// List of the names of all projects in the current solution this project references.
        /// </summary>
        public List<string> ProjectReferenceNames
        {
            get
            {
                var output = Settings[ProjectReferencesSettingName] as List<string>;
                if (output == null)
                {
                    output = new List<string>();
                    Settings[ProjectReferencesSettingName] = output;
                }
                return output;
            }
            set
            {
                Settings[ProjectReferencesSettingName] = value;
            }
        }
        protected override Task<IOnDisk> LoadProjectItem(ItemValue item)
        {
            return Task.FromResult(new ProjectFileWrapper(this.Filename, item.Filename, item.AssemblyQualifiedTypeName) as IOnDisk);
        }

        /// <summary>
        /// Gets the file at the given project path
        /// </summary>
        /// <param name="path">Project path of the file</param>
        /// <returns>The file at the given path, or null if it could not be found</returns>
        public async Task<object> GetFile(string path, IOHelper.DuplicateMatchSelector duplicateMatchSelector, PluginManager manager)
        {
            return await (GetItem(path)?.GetFile(manager, duplicateMatchSelector));
        }

        /// <summary>
        /// Gets the filename of the file at the given project path
        /// </summary>
        /// <param name="path">Project path of the file</param>
        /// <returns>The physical filename of the project file.</returns>
        public string GetFilename(string path)
        {
            return GetItem(path).Filename;
        }

        /// <summary>
        /// Determines whether or not a file can be created inside the given project directory
        /// </summary>
        /// <param name="path">Project directory inside which the file will be created</param>
        /// <returns>A boolean indicating whether or not a file could be created inside the given directory</returns>
        public virtual bool CanCreateFile(string path)
        {
            return CanCreateDirectory(path);
        }

        /// <summary>
        /// Determines whether or not a file can be imported inside the given project directory
        /// </summary>
        /// <param name="path">Project directory inside which the file will be imported</param>
        /// <returns>A boolean indicating whether or not a file could be imported inside the given directory</returns>
        public virtual bool CanImportFile(string path)
        {
            return CanCreateFile(path);
        }

        /// <summary>
        /// Determines the types of files that can be created inside the given project directory
        /// </summary>
        /// <param name="path">Project directory inside which the file will be created</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        /// <returns>The types of files that can be created inside the given project directory</returns>
        public virtual IEnumerable<TypeInfo> GetSupportedFileTypes(string path, PluginManager manager)
        {
            if (CanCreateDirectory(path))
            {
                return IOHelper.GetCreateableFileTypes(manager);
            }
            else
            {
                return new TypeInfo[] { };
            }
        }

        /// <summary>
        /// Creates a file inside the given directory
        /// </summary>
        /// <param name="parentPath">Project directory inside which the file will be created</param>
        /// <param name="name">Name of the new file</param>
        /// <param name="fileType">Type of the new file</param>
        public virtual void CreateFile(string parentPath, string name, Type fileType)
        {
            if (!ReflectionHelpers.IsOfType(fileType, typeof(ICreatableFile).GetTypeInfo()))
            {
                throw new ArgumentException(string.Format(Properties.Resources.Reflection_ErrorInvalidType, nameof(ICreatableFile)), nameof(fileType));
            }
            if (!DirectoryExists(parentPath))
            {
                CreateDirectory(parentPath);
            }

            var fixedPath = FixPath(parentPath);

            ICreatableFile fileObj = ReflectionHelpers.CreateInstance(fileType.GetTypeInfo()) as ICreatableFile;
            fileObj.CreateFile(name);
            fileObj.Filename = Path.Combine(Path.GetDirectoryName(this.Filename), parentPath.Replace("/", "\\").TrimStart("\\".ToCharArray()), name);

            AddItem(fixedPath + "/" + name, new ProjectFileWrapper(this.Filename, fileObj.Filename, fileObj));
        }

        /// <summary>
        /// Determines whether a file with the given name can be created inside the given directory
        /// </summary>
        /// <param name="parentProjectPath">Project directory inside which the file will be created</param>
        /// <param name="filename">Name of the new file</param>
        /// <returns>Whether or not a file with the given name can be created in the given directory</returns>
        public virtual bool IsFileSupported(string parentProjectPath, string filename)
        {
            return CanImportFile(parentProjectPath);
        }

        /// <summary>
        /// Gets the project path of the imported file.
        /// </summary>
        /// <param name="parentProjectPath">Project directory inside which the imported file will be put</param>
        /// <param name="filename">Physical path of the file to import</param>
        /// <returns>The path the new file would have after being imported</returns>
        protected virtual string GetImportedFilePath(string parentProjectPath, string filename)
        {
            return FixPath(Path.Combine(parentProjectPath, Path.GetFileName(filename)));
        }

        /// <summary>
        /// Gets the supported extensions that can be used for importing files
        /// </summary>
        /// <param name="parentProjectPath">Project directory into which the file will be imported</param>
        /// <returns>The supported extensions of files to be imported into the given directory</returns>
        public virtual IEnumerable<string> GetSupportedImportFileExtensions(string parentProjectPath)
        {
            return new string[] { "*" };
        }

        /// <summary>
        /// Sets the type of a file to the given type
        /// </summary>
        /// <param name="path">Project path of the file of which to set the type</param>
        /// <param name="fileType">Desired type of the file</param>
        /// <remarks>Only affects files that have not yet been opened.</remarks>
        protected void SetFileType(string path, Type fileType)
        {
            var item = GetItem(path);
            if (item != null)
            {
                item.FileAssemblyQualifiedTypeName = fileType.AssemblyQualifiedName;
            }
        }

        /// <summary>
        /// Imports a file into the project at the specified path
        /// </summary>
        /// <param name="destinationPath">Desired project path of the file to import</param>
        /// <param name="filePath">Physical path of the file to import</param>
        /// <param name="fileType">Type of the file, or null to auto-detect the type on load</param>
        /// <param name="provider">Instance of the IO provider from which to get the file located at <paramref name="filePath"/>.</param>
        public virtual void AddExistingFileToPath(string destinationPath, string filePath, Type fileType, IIOProvider provider)
        {
            var fixedPath = FixPath(destinationPath);

            var relativePath = filePath.Replace(GetRootDirectory(), "").Replace("\\", "/").TrimStart("/".ToCharArray());
            ProjectFileWrapper wrapper = new ProjectFileWrapper(this.Filename, relativePath);
            if (fileType != null)
            {
                wrapper.FileAssemblyQualifiedTypeName = fileType.AssemblyQualifiedName;
            }
            AddItem(destinationPath, wrapper);

            FileAdded?.Invoke(this, new ProjectFileAddedEventArgs { ProjectPath = Path.GetFileName(destinationPath), PhysicalPath = filePath });
        }

        /// <summary>
        /// Imports a file into the given project directory
        /// </summary>
        /// <param name="parentPath">Project directory in which to put the imported file</param>
        /// <param name="filePath">Physical path of the file to import</param>
        /// <param name="fileType">Type of the file, or null to auto-detect the type on load</param>
        /// <param name="provider">Instance of the IO provider from which to get the file located at <paramref name="filePath"/>.</param>
        public virtual void AddExistingFile(string parentPath, string filePath, Type fileType, IIOProvider provider)
        {
            var fixedPath = FixPath(parentPath);
            var importedName = GetImportedFilePath(fixedPath, filePath);

            //Copy the file
            var source = filePath;
            var dest = Path.Combine(Path.GetDirectoryName(this.Filename), importedName.Replace("/", "\\").TrimStart("\\".ToCharArray()));
            if (!(source.Replace("\\", "/").ToLower() == dest.Replace("\\", "/").ToLower()))
            {
                provider.CopyFile(filePath, dest);
            }

            //Add the file
            AddExistingFileToPath(importedName, dest, fileType, provider);
        }

        /// <summary>
        /// Adds a file to a directory in the project
        /// </summary>
        /// <param name="parentPath">Directory in which to put the imported file</param>
        /// <param name="filePath">Full path of the file to import</param>
        public virtual void AddExistingFile(string parentPath, string filePath, IIOProvider provider)
        {
            AddExistingFile(parentPath, filePath, null, provider);
        }

        /// <summary>
        /// Determines whether or not a file exists at the given project path
        /// </summary>
        /// <param name="path">Project path of the desired file</param>
        /// <returns>Whether or not a file exists at the given project path</returns>
        public bool FileExists(string path)
        {
            return ItemExists(path);
        }

        /// <summary>
        /// Determines whether or not the given file can be deleted
        /// </summary>
        /// <param name="path">Project path of the desired file</param>
        /// <returns>Whether or not the given file can be deleted</returns>
        public virtual bool CanDeleteFile(string path)
        {
            return ItemExists(path);
        }

        /// <summary>
        /// Deletes the given file
        /// </summary>
        /// <param name="path">Project path of the file to delete</param>
        public virtual void DeleteFile(string path)
        {
            DeleteItem(path);
        }

        /// <summary>
        /// The project directory
        /// </summary>
        /// <returns>The project directory</returns>
        public virtual string GetRootDirectory()
        {
            return Path.GetDirectoryName(this.Filename);
        }

        /// <summary>
        /// Gets the projects referenced by the current project
        /// </summary>
        /// <returns>The projects referenced by the current project</returns>
        public IEnumerable<Project> GetReferences()
        {
            List<Project> output = new List<Project>();
            foreach (var item in ProjectReferenceNames)
            {
                var p = ParentSolution.GetProjectsByName(item).FirstOrDefault();
                if (p != null)
                {
                    output.Add(p);
                }
            }
            return output;
        }

        /// <summary>
        /// Determines whether or not this project contains a circular reference back to itself.
        /// </summary>
        /// <returns>Whether or not this project contains a circular reference back to itself</returns>
        /// <remarks>It does not detect whether other projects this one references have their own circular references.</remarks>
        public bool HasCircularReferences()
        {
            List<Project> tree = new List<Project>();
            FillReferenceTree(tree, this);
            return tree.Contains(this);
        }

        /// <summary>
        /// Fills <paramref name="tree"/> with all the references of the current item.
        /// Stops if the last item added is the current instance of project.
        /// </summary>
        /// <param name="tree">The cumulative list of project references</param>
        /// <param name="currentItem">The project whose references should be added</param>
        private void FillReferenceTree(List<Project> tree, Project currentItem)
        {
            foreach (var item in currentItem.GetReferences())
            {
                tree.Add(item);
                if (ReferenceEquals(item, this))
                {
                    return;
                }
                else
                {
                    if (!item.HasCircularReferences())
                    {
                        FillReferenceTree(tree, item);
                    }
                }
            }
        }
    }
}
