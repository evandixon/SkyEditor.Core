using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            if (obj.GetType() == typeof(TypeInfo))
            {
                original = (TypeInfo)obj;
            }
            else if (obj.GetType() == typeof(Type))
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
                t = name.FoundType.Value;
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
        public static bool CanCreateInstance(TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return !type.IsAbstract && type.DeclaredConstructors.Any(x => x.GetParameters().Length == 0);
        }

        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="type">Type to be created</param>
        /// <returns>A new object of the given type</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
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
                    manager = new ResourceManager(item.Replace(".resource", ""), parent);
                    friendlyName = manager.GetString(type.FullName.Replace(".", "_"));

                    if (friendlyName != null) break;
                }
            }
            catch (MissingManifestResourceException)
            {
                // Can't find the resouce file.  Default to just using the type name, or keep searching for resource files
            }

            return friendlyName ?? type.FullName;
        }
    }
}
