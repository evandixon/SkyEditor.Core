﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Functions that aid in reflection
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Determines whether the given object is, inherits, or implements the given type.
        /// </summary>
        /// <param name="obj">Object to check.  If this is a type, its value as opposed to its type will be evaluated.</param>
        /// <param name="typeToCheck">The type for which to check</param>
        /// <returns>A boolean indicating whether or not <paramref name="obj"/> is, inherits, or implements the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> or <paramref name="typeToCheck"/> is null.</exception>
        public static bool IsOfType(object obj, TypeInfo typeToCheck)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (typeToCheck == null)
            {
                throw new ArgumentNullException(nameof(typeToCheck));
            }

            TypeInfo original;

            // Determine which type we're evaluating
            // If obj is a type, use it.  Otherwise, get the type of obj.
            if (obj is TypeInfo)
            {
                original = (TypeInfo)obj;
            }
            else if (obj is Type)
            {
                original = ((Type)obj).GetTypeInfo();
            }
            else
            {
                original = obj.GetType().GetTypeInfo();
            }

            // True if the original and reference types are the same or if its base type and TypeToCheck are the same
            var isMatch = original.Equals(typeToCheck) || (original.BaseType != null && IsOfType(original.BaseType, typeToCheck));

            // Check to see if any of the obj's implemented interfaces match
            if (!isMatch)
            {
                isMatch = original.ImplementedInterfaces.Any((x) => IsOfType(x.GetTypeInfo(), typeToCheck));
            }

            return isMatch;
        }

        /// <summary>
        /// Determines whether the given object is, inherits, or implements the given type.
        /// </summary>
        /// <param name="obj">Object to check.  If this is a type, its value as opposed to its type will be evaluated.</param>
        /// <param name="typeToCheck">The type for which to check</param>
        /// <returns>A boolean indicating whether or not <paramref name="obj"/> is, inherits, or implements the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> or <paramref name="typeToCheck"/> is null.</exception>
        public static bool IsOfType(object obj, Type typeToCheck)
        {
            return IsOfType(obj, typeToCheck.GetTypeInfo());
        }

        /// <summary>
        /// Gets a type using the assembly-qualified name if possible
        /// </summary>
        /// <param name="assemblyQualifiedName">Assembly-qualified name of the type</param>
        /// <param name="assemblies">Assemblies to check.  Retrieve from the current app domain if possible; otherwise, provide known assemblies.</param>
        /// <returns>The type with the given assembly-qualified name or null if it cannot be found</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyQualifiedName"/> or <paramref name="assemblies"/> is null.</exception>
        public static TypeInfo GetTypeByName(string assemblyQualifiedName, IEnumerable<Assembly> assemblies)
        {
            if (assemblyQualifiedName == null)
            {
                throw new ArgumentNullException(nameof(assemblyQualifiedName));
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            var t = Type.GetType(assemblyQualifiedName, false);
            if (t == null)
            {
                // Can't find the time.
                // Parse it and search the available assemblies.
                var name = new ParsedAssemblyQualifiedName(assemblyQualifiedName, assemblies);
                t = name.FoundType?.Value;
            }
            return t?.GetTypeInfo();
        }

        /// <summary>
        /// Gets a type using the assembly-qualified name if possible
        /// </summary>
        /// <param name="assemblyQualifiedName">Assembly-qualified name of the type</param>
        /// <param name="manager">Instance of the current plugin manager from which to load available assemblies</param>
        /// <returns>The type with the given assembly-qualified name or null if it cannot be found</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyQualifiedName"/> or <paramref name="manager"/> is null.</exception>
        public static TypeInfo GetTypeByName(string assemblyQualifiedName, PluginManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return GetTypeByName(assemblyQualifiedName, manager.GetLoadedAssemblies());
        }

        /// <summary>
        /// Determines whether or not <see cref="CreateInstance(TypeInfo)"/> can create an instance of this type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>A boolean indicating whether or not an instance of this type can be created</returns>
        /// <remarks>
        /// Current criteria:
        /// - Type must not be abstract
        /// - Type must have a default constructor</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        [Obsolete("Call from a PluginManager to use dependency injection")]
        public static bool CanCreateInstance(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return !type.IsAbstract && type.DeclaredConstructors.Any(x => x.GetParameters().Length == 0);
        }

        /// <summary>
        /// Determines whether or not <see cref="CreateInstance(TypeInfo)"/> can create an instance of this type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>A boolean indicating whether or not an instance of this type can be created</returns>
        /// <remarks>
        /// Current criteria:
        /// - Type must not be abstract
        /// - Type must have a default constructor</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        [Obsolete("Call from a PluginManager to use dependency injection")]
        public static bool CanCreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return CanCreateInstance(type.GetTypeInfo());
        }

        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="type">Type to be created</param>
        /// <returns>A new object of the given type</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        [Obsolete("Call from a PluginManager to use dependency injection")]
        public static object CreateInstance(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Activator.CreateInstance(type.AsType());
        }

        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="type">Type to be created</param>
        /// <returns>A new object of the given type</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        [Obsolete("Call from a PluginManager to use dependency injection")]
        public static object CreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates a new instance of the type of the given object.
        /// </summary>
        /// <param name="target">Instance of the type of which to create a new instance</param>
        /// <returns>A new object with the same type as <paramref name="target"/>.</returns>        
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null.</exception>
        [Obsolete("Call from a PluginManager to use dependency injection")]
        public static object CreateNewInstance(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return CreateInstance(target.GetType());
        }

        /// <summary>
        /// A user-friendly representation of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The user-friendly name of the given type if its contained assembly has its name in its localized resource file or the full name of the type if it does not.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public static string GetTypeFriendlyName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var parent = type.GetTypeInfo().Assembly;            
            var resxNames = new List<string>(parent.GetManifestResourceNames());
            ResourceManager manager = null;
            string friendlyName = null;

            try
            {
                foreach (var item in resxNames)
                {
                    manager = new ResourceManager(item.Replace(".resources", ""), parent);
                    friendlyName = manager.GetString(type.FullName.Replace(".", "_"));

                    if (friendlyName != null) break;
                }
            }
            catch (MissingManifestResourceException ex)
            {
                // Can't find the resouce file.  Default to just using the type name, or keep searching for resource files
                Console.WriteLine(ex.ToString());
            }

            return friendlyName ?? type.FullName;
        }

        /// <summary>
        /// Loads the assembly located at the given path into the current AppDomain and returns it.
        /// </summary>
        /// <param name="assemblyPath">Full path of the assembly to load.</param>
        /// <returns>The assembly that was loaded.</returns>
        /// <exception cref="NotSupportedException">Thrown when the current platform does not support loading assemblies from a specific path.</exception>
        /// <exception cref="BadImageFormatException">Thrown when the assembly is not a valid .Net assembly.</exception>
        public static Assembly LoadSingleAssembly(string assemblyPath)
        {
            if (!Path.IsPathRooted(assemblyPath))
            {
                assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), assemblyPath);
            }

            return Assembly.LoadFrom(assemblyPath);
        }

        /// <summary>
        /// Loads the assembly located at the given path into the current AppDomain and returns it.
        /// If the assembly is already loaded, no action is taken besides returning it.
        /// </summary>
        /// <param name="assemblyPath">Full path of the assembly to load.</param>
        /// <returns>The assembly that was loaded.</returns>
        /// <exception cref="NotSupportedException">Thrown when the current platform does not support loading assemblies from a specific path.</exception>
        /// <exception cref="BadImageFormatException">Thrown when the assembly is not a valid .Net assembly.</exception>
        public static Assembly LoadAssemblyWithDependencies(string path)
        {
            // First, check to see if it's already loaded
            var name = AssemblyName.GetAssemblyName(path);
            var candidateAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == name.Name);

            if (candidateAssemblies.Any())
            {
                // It was loaded, then there's no point in loading it again. In some cases, it could cause more problems
                return candidateAssemblies.First();
            }
            else
            {
                // Load it
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                var loadedAssembly = LoadSingleAssembly(path);

                // Load the assembly's dependencies
                // For some reason, this doesn't happen automatically for executables
                foreach (var item in loadedAssembly.GetReferencedAssemblies())
                {
                    Assembly.Load(item);
                }

                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
                return loadedAssembly;
            }
        }

        /// <summary>
        /// Gets the full paths of all assemblies referenced by the given assembly
        /// </summary>
        /// <param name="source">Assembly from which which to find the dependencies</param>
        /// <returns>A list containing all of the file paths of all depencency assemblies, both direct and indirect</returns>
        /// <remarks>Only returns assemblies in the same directory as the source assembly.
        /// 
        /// A side effect that may cause issues is that any referenced assembly that is not in the current app domain will be loaded</remarks>
        public static List<string> GetAssemblyDependencies(Assembly source)
        {
            var output = new List<string>();
            var devAssemblyPaths = new List<string>();
            var workingDirectory = Path.GetDirectoryName(source.Location);
            devAssemblyPaths.AddRange(Directory.GetFiles(workingDirectory, "*.dll"));
            devAssemblyPaths.AddRange(Directory.GetFiles(workingDirectory, "*.exe"));

            // Get regional resources
            var resourcesName = Path.GetFileNameWithoutExtension(source.Location) + ".resources.dll";
            foreach (var item in Directory.GetDirectories(Path.GetDirectoryName(source.Location)))
            {
                if (File.Exists(Path.Combine(item, resourcesName)))
                {
                    output.Add(Path.Combine(item, resourcesName));
                }
            }

            // Look at the dependencies
            foreach (var reference in source.GetReferencedAssemblies())
            {
                var isLocal = false;

                // Try to find the filename of this reference
                foreach (var sourcePath in devAssemblyPaths)
                {
                    var name = AssemblyName.GetAssemblyName(sourcePath);
                    if (reference.Name == name.Name)
                    {
                        if (!output.Contains(sourcePath))
                        {
                            output.Add(sourcePath);
                            isLocal = true;
                            break;
                        }
                    }
                }

                if (isLocal)
                {
                    // Try to find the references of this reference
                    
                    var currentAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == reference.FullName).FirstOrDefault();

                    if (currentAssembly == null)
                    {
                        // Nothing found; expand the search to account for different versions
                        currentAssembly = (from a in AppDomain.CurrentDomain.GetAssemblies()
                                           let name = a.GetName()
                                           where name.Name == reference.Name
                                           orderby name.Version descending
                                           select a).FirstOrDefault();
                    }

                    if (currentAssembly == null)
                    {
                        // This reference is not in the current AppDomain
                        // Try to find the assembly
                        // To-Do: it would be optimal to do this in another AppDomain, but since this assembly would be loaded if it was needed, there shouldn't(tm) be any real harm
                        foreach (var sourcePath in devAssemblyPaths)
                        {
                            var name = AssemblyName.GetAssemblyName(sourcePath);
                            if (reference.FullName == name.FullName)
                            {
                                var a = LoadSingleAssembly(sourcePath);
                                if (a != null)
                                {
                                    output.AddRange(GetAssemblyDependencies(a));
                                }
                            }
                        }
                    }
                    else
                    {
                        output.AddRange(GetAssemblyDependencies(currentAssembly));
                    }
                }
            }
            return output;
        }
        
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => string.Equals(a.FullName, e.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string requestingDir = null;
                if (e.RequestingAssembly != null)
                {
                    requestingDir = Path.GetDirectoryName(e.RequestingAssembly.Location);
                }

                // Get filenames of all assemblies to check
                var candidates = new List<string>();
                candidates.AddRange(Directory.GetFiles(appDir, "*.dll"));
                candidates.AddRange(Directory.GetFiles(appDir, "*.exe"));

                if (!string.IsNullOrEmpty(requestingDir) && requestingDir != appDir)
                {
                    candidates.AddRange(Directory.GetFiles(requestingDir, "*.dll"));
                    candidates.AddRange(Directory.GetFiles(requestingDir, "*.exe"));
                }

                foreach (var item in Directory.GetDirectories(EnvironmentPaths.GetPluginsExtensionDirectory()))
                {
                    if (item != appDir && item != requestingDir)
                    {
                        candidates.AddRange(Directory.GetFiles(item, "*.dll"));
                        candidates.AddRange(Directory.GetFiles(item, "*.exe"));
                    }
                }

                // Return the first assembly that has the requested name
                var desiredName = new AssemblyName(e.Name);
                return candidates.Select(x => new { AssemblyName = AssemblyName.GetAssemblyName(x), Location = x })
                    .Where(x => x.AssemblyName.Name == desiredName.Name)          
                    .OrderByDescending(x => x.AssemblyName.Version)
                    .Select(x => LoadAssemblyWithDependencies(x.Location))
                    .FirstOrDefault();
            }
        }
    }
}
