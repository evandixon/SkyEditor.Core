using SkyEditor.Core.Projects;
using SkyEditor.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core
{
    /// <summary>
    /// A user-friendly representation of an error
    /// </summary>
    public class ErrorInfo
    {
        public ErrorInfo()
        {
        }

        public ErrorInfo(Exception innerException) : this()
        {
            Type = ErrorType.Exception;
            InnerException = innerException;
            Message = innerException.ToString();
        }

        public ErrorInfo(Project project)
        {
            SourceProject = project;
        }

        /// <summary>
        /// The type of error
        /// </summary>
        public ErrorType Type { get; set; }

        /// <summary>
        /// Internal identifier for the error
        /// </summary>
        public string StandardCode { get; set; }

        /// <summary>
        /// User-friendly message describing the erorr
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The project from which the error originated, or null if not applicable
        /// </summary>
        public Project SourceProject { get; set; }

        /// <summary>
        /// The file from which the error originated, or null if not applicable
        /// </summary>
        public FileViewModel SourceFile { get; set; }

        /// <summary>
        /// The line in the file from which the error originated, or null if not applicable
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// The inner exception, or null if not applicable
        /// </summary>
        public Exception InnerException { get; set; }
    }
}
