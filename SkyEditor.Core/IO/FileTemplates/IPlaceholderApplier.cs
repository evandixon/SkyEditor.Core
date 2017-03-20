using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO.FileTemplates
{
    /// <summary>
    /// An object that can apply placeholders to a file
    /// </summary>
    public interface IPlaceholderApplier
    {
        /// <summary>
        /// The types of files supported by this placeholder applier
        /// </summary>
        IEnumerable<Type> GetSupportedTypes();

        /// <summary>
        /// Determines whether or not the given file is supported
        /// </summary>
        /// <param name="file">The file to which placeholders should be applied</param>
        /// <returns>A boolean indicating whether or not the current placeholder applier supports the given file</returns>
        bool SupportsFile(object file);

        /// <summary>
        /// Applies the given placeholders to the given file
        /// </summary>
        /// <param name="file">The file to which placeholders should be applied</param>
        /// <param name="placeholders">An IDictionary containing the placeholders to apply.  Key: name of the placeholder, Value: the value to use</param>
        void ApplyPlaceholders(object file, IDictionary<string, object> placeholders);

    }
}
