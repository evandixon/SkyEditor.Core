using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    }
}
