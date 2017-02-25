using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkyEditor.Core.IO;
using System.Reflection;
using System.Linq;
using System.IO;

namespace SkyEditor.Core.Projects
{
    public class Solution : ProjectBase<Project>
    {

        public const string SolutionFileExt = "skysln";

        public override string ProjectFileExtension => SolutionFileExt;

        #region Events
        /// <summary>
        /// Raised when the solution builds one or more projects
        /// </summary>
        public event EventHandler SolutionBuildStarted;

        /// <summary>
        /// Raised when the solution build has completed
        /// </summary>
        public event EventHandler SolutionBuildCompleted;

        /// <summary>
        /// Raised when a project is added
        /// </summary>
        public event EventHandler<ProjectAddedEventArgs> ProjectAdded;

        /// <summary>
        /// Raised just prior to removing a project
        /// </summary>
        public event EventHandler<ProjectRemovingEventArgs> ProjectRemoving;

        /// <summary>
        /// Raised when a project is removed
        /// </summary>
        public event EventHandler<ProjectRemovedEventArgs> ProjectRemoved;
        #endregion

        private void Project_Modified(object sender, EventArgs e)
        {
            this.HasUnsavedChanges = true;
        }

        protected override Task<IOnDisk> LoadProjectItem(ItemValue item)
        {
            throw new NotImplementedException();
        }        

        /// <summary>
        /// Gets the types of projects that can be added into a particular directory
        /// </summary>
        /// <param name="path">Path in the solution in which to add the project</param>
        /// <param name="manager">Instance of the currrent plugin manager</param>
        /// <returns>The types of projects that can be added into the given directory</returns>
        public virtual IEnumerable<TypeInfo> GetSupportedProjectTypes(string path, PluginManager manager)
        {
            if (CanCreateDirectory(path))
            {
                return manager.GetRegisteredTypes(typeof(Project).GetTypeInfo());
            }
            else
            {
                return new TypeInfo[] { };
            }
        }

        /// <summary>
        /// Gets all projects in the solution, regardless of their parent directory.
        /// </summary>
        /// <returns>The projects in the solution</returns>
        public IEnumerable<Project> GetAllProjects()
        {
            return GetItems("/", true).Values.Where(x => x is Project).Select(x => x as Project);
        }

        /// <summary>
        /// Saves all the projects in the solution
        /// </summary>
        /// <param name="provider"></param>
        public virtual void SaveAllProjects(IIOProvider provider)
        {
            foreach (var item in GetAllProjects())
            {
                item.Save(provider);
            }
        }

        /// <summary>
        /// Returns all projects in the solution with the given name, regardless of directories.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public virtual IEnumerable<Project> GetProjectsByName(string Name)
        {
            return from p in GetAllProjects() where p.Name.ToLower() == Name.ToLower() select p;
        }

        #region Solution Logical Filesystem

        /// <summary>
        /// Gets the project at the given path.
        /// </summary>
        /// <param name="path">Soluton path of the project</param>
        /// <returns>The project at the given path, or null if it could not be found</returns>
        public Project GetProject(string path)
        {
            return GetItem(path);
        }

        /// <summary>
        /// Adds the project to the solution.
        /// </summary>
        /// <param name="path">Full path of the project</param>
        /// <param name="project">Project to add</param>
        public void AddProject(string path, Project project)
        {
            AddItem(path, project);
            ProjectAdded?.Invoke(this, new ProjectAddedEventArgs { Path = path, Project = project });
        }

        /// <summary>
        /// Creates a project
        /// </summary>
        /// <param name="parentPath">Solution directory in which to add the project</param>
        /// <param name="projectName">Name of the project</param>
        /// <param name="projectType">Type of the project</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        public virtual async Task CreateProject(string parentPath, string projectName, Type projectType, PluginManager manager)
        {
            var p = await ProjectBase.CreateProject<Project>(Path.Combine(Path.GetDirectoryName(this.Filename), parentPath.TrimStart('/')), projectName, projectType, manager);
            p.ParentSolution = this;
            AddProject(FixPath(parentPath) + "/" + projectName, p);
        }

        /// <summary>
        /// Opens a project and adds it to the solution
        /// </summary>
        /// <param name="parentPath">Solution directory in which to add the project</param>
        /// <param name="projectFilename">Physical path of the project file</param>
        /// <param name="manager">Instance of the current plugin manager</param>
        public virtual async Task AddExistingProject(string parentPath, string projectFilename, PluginManager manager)
        {
            var p = await ProjectBase.OpenProjectFile<Project>(projectFilename, manager);
            p.ParentSolution = this;
            AddProject(FixPath(parentPath) + "/" + p.Name, p);
        }

        /// <summary>
        /// Deletes the project at the given path
        /// </summary>
        /// <param name="projectPath">The path of the project to delete</param>
        public virtual void DeleteProject(string projectPath)
        {
            var fixedPath = FixPath(projectPath);

            var project = GetProject(fixedPath);
            ProjectRemoving?.Invoke(this, new ProjectRemovingEventArgs { Project = project, Path = fixedPath });

            DeleteItem(fixedPath);
            project.Modified -= Project_Modified;

            project.Dispose();

            ProjectRemoved?.Invoke(this, new ProjectRemovedEventArgs { Path = fixedPath });            
        }
        #endregion

        /// <summary>
        /// Determines whether or not a project can be created inside the given directory
        /// </summary>
        /// <param name="path">Directory in which the project will be created</param>
        /// <returns>Whether or not a project can be created inside the given directory</returns>
        public virtual bool CanCreateProject(string path)
        {
            return CanCreateDirectory(path);
        }

        /// <summary>
        /// Determines whether or not the project at the given path can be deleted
        /// </summary>
        /// <param name="projectPath">Path of the project</param>
        /// <returns>Determines whether or not the project at the given path can be deleted</returns>
        public virtual bool CanDeleteProject(string projectPath)
        {
            return ItemExists(projectPath);
        }

        #region Building

        /// <summary>
        /// Cancels the solution's build, and the builds of any child projects.
        /// </summary>
        public override void CancelBuild()
        {
            // Cancel the solution's build
            base.CancelBuild();

            // Cancel any projects that are building
            foreach (var item in GetAllProjects())
            {
                if (item.IsBuilding)
                {
                    item.CancelBuild();
                }
            }
        }

        /// <summary>
        /// Gets the projects that should be built with the solution
        /// </summary>
        /// <returns>The projects to build</returns>
        public virtual IEnumerable<Project> GetProjectsToBuild()
        {
            return GetAllProjects().Where(x => x.CanBuild);
        }

        /// <summary>
        /// Builds the projects in the solution
        /// </summary>
        public override async Task Build()
        {
            if (!IsBuilding && CanBuild)
            {
                await Build(GetProjectsToBuild());
            }
        }

        /// <summary>
        /// Builds the given projects
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public virtual async Task Build(IEnumerable<Project> projects)
        {
            SolutionBuildStarted?.Invoke(this, new EventArgs());
            Dictionary<Project, bool> toBuild = new Dictionary<Project, bool>(); // Key: project, value: if it has been built

            foreach (var item in projects)
            {
                if (!item.HasCircularReferences())
                {
                    // Stop if the build has been canceled.
                    if (IsCancelRequested)
                    {
                        return;
                    }

                    toBuild.Add(item, false);
                }
                else
                {
                    throw (new ProjectCircularReferenceException());
                }
            }

            var toBuildKeys = toBuild.Keys.ToList(); // Because C# doesn't allow getting `toBuild.Keys[count]`
            for (var count = 0; count < toBuild.Keys.Count; count++)
            {
                // Stop if the build has been canceled.
                if (IsCancelRequested)
                {
                    return;
                }

                var key = toBuildKeys[count];
                // If this project has not been built
                if (!toBuild[key])
                {
                    // Then build the project, but build its dependencies first
                    await BuildProjects(toBuild, key);
                }
            }

            SolutionBuildCompleted?.Invoke(this, new EventArgs());
        }

        private async Task BuildProjects(Dictionary<Project, bool> toBuild, Project currentProject)
        {
            List<Task> buildTasks = new List<Task>();
            foreach (var item in currentProject.GetReferences().Where(x => x.CanBuild))
            {
                // Stop if the build has been canceled.
                if (IsCancelRequested)
                {
                    return;
                }

                // Start building this project
                buildTasks.Add(BuildProjects(toBuild, item));
            }
            await Task.WhenAll(buildTasks);

            // Check to see if a build is needed
            var doBuild = false; // Can't await inside a lock statement, so do the check, then await outside of it
            if (!toBuild[currentProject])
            {                
                lock(_toBuildLock) // Ensure build isn't started twice
                {
                    if (!toBuild[currentProject]) // Check again in case there were threading issues
                    {
                        doBuild = true;
                        toBuild[currentProject] = true;
                    }
                }                
            }

            // Do the build
            if (doBuild)
            {                
                UpdateBuildLoadingStatus(toBuild);
                await currentProject.Build();
            }           
        }
        private object _toBuildLock = new object();

        private void UpdateBuildLoadingStatus(Dictionary<Project, bool> toBuild)
        {
            int built = toBuild.Values.Where(x => x).Count();
            this.Progress = built / toBuild.Count;
        }
        #endregion
    }
}
