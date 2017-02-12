﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SkyEditor.Core.Properties {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SkyEditor.Core.Properties.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The following commands are available:.
        /// </summary>
        internal static string Console_AvailableCommands {
            get {
                return ResourceManager.GetString("Console_AvailableCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown command &quot;{0}&quot;..
        /// </summary>
        internal static string Console_CommandNotFound {
            get {
                return ResourceManager.GetString("Console_CommandNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sky Editor Project.
        /// </summary>
        internal static string File_SkyEditorProject {
            get {
                return ResourceManager.GetString("File_SkyEditorProject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sky Editor Solution.
        /// </summary>
        internal static string File_SkyEditorSolution {
            get {
                return ResourceManager.GetString("File_SkyEditorSolution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No method to open the file of type &quot;{0}&quot; could be found.  This type must either implement IOpenableFile or have a registered IFileOpener that supports this type..
        /// </summary>
        internal static string IO_ErrorNoFileOpener {
            get {
                return ResourceManager.GetString("IO_ErrorNoFileOpener", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempted to write to read-only file.
        /// </summary>
        internal static string IO_ErrorReadOnly {
            get {
                return ResourceManager.GetString("IO_ErrorReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find a file at the given path in the current I/O provider.
        /// </summary>
        internal static string IO_FileNotFound {
            get {
                return ResourceManager.GetString("IO_FileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When the file is in memory, length cannot exceed int.MaxValue.
        /// </summary>
        internal static string IO_GenericFile_ErrorLengthTooLarge {
            get {
                return ResourceManager.GetString("IO_GenericFile_ErrorLengthTooLarge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using GenericFile.Save() requires GenericFile.Filename to not be null..
        /// </summary>
        internal static string IO_GenericFile_ErrorNoSaveFilename {
            get {
                return ResourceManager.GetString("IO_GenericFile_ErrorNoSaveFilename", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index &quot;{0}&quot; out of range.  Total length of file: &quot;{1}&quot;..
        /// </summary>
        internal static string IO_GenericFile_OutOfRange {
            get {
                return ResourceManager.GetString("IO_GenericFile_OutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You!.
        /// </summary>
        internal static string PluginDevExtAuthor {
            get {
                return ResourceManager.GetString("PluginDevExtAuthor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All of the plugins that are placed in the development directory..
        /// </summary>
        internal static string PluginDevExtDescription {
            get {
                return ResourceManager.GetString("PluginDevExtDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Development Plugins.
        /// </summary>
        internal static string PluginDevExtName {
            get {
                return ResourceManager.GetString("PluginDevExtName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0.0.
        /// </summary>
        internal static string PluginDevExtVersion {
            get {
                return ResourceManager.GetString("PluginDevExtVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An item already exists at the given path.
        /// </summary>
        internal static string Project_ItemExistsAtPath {
            get {
                return ResourceManager.GetString("Project_ItemExistsAtPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid type supplied.  It should implement or inherit &quot;{0}&quot;..
        /// </summary>
        internal static string Reflection_ErrorInvalidType {
            get {
                return ResourceManager.GetString("Reflection_ErrorInvalidType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The supplied type cannot be instatiated.  Please make sure it has a default constructor and is not an abstract class or interface..
        /// </summary>
        internal static string Reflection_ErrorNoDefaultConstructor {
            get {
                return ResourceManager.GetString("Reflection_ErrorNoDefaultConstructor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All Files.
        /// </summary>
        internal static string UI_AllFiles {
            get {
                return ResourceManager.GetString("UI_AllFiles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A menu action&apos;s ActionPath must contain at least one element..
        /// </summary>
        internal static string UI_ErrorActionMenuPathEmpty {
            get {
                return ResourceManager.GetString("UI_ErrorActionMenuPathEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot load sibling view models..
        /// </summary>
        internal static string UI_ErrorCantLoadSiblingViewModels {
            get {
                return ResourceManager.GetString("UI_ErrorCantLoadSiblingViewModels", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This IExtensionCollection lists extensions that are currently installed, not ones that can be installed, so this cannnot install extensions..
        /// </summary>
        internal static string UI_ErrorLocalExtensionCollectionInstall {
            get {
                return ResourceManager.GetString("UI_ErrorLocalExtensionCollectionInstall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No sibling view model of type &quot;{0}&quot; could be found..
        /// </summary>
        internal static string UI_ErrorNoSiblingViewModelOfType {
            get {
                return ResourceManager.GetString("UI_ErrorNoSiblingViewModelOfType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Installed Extensions.
        /// </summary>
        internal static string UI_InstalledExtension {
            get {
                return ResourceManager.GetString("UI_InstalledExtension", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loading....
        /// </summary>
        internal static string UI_LoadingGeneric {
            get {
                return ResourceManager.GetString("UI_LoadingGeneric", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ready.
        /// </summary>
        internal static string UI_Ready {
            get {
                return ResourceManager.GetString("UI_Ready", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Supported Files.
        /// </summary>
        internal static string UI_SupportedFiles {
            get {
                return ResourceManager.GetString("UI_SupportedFiles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} Files.
        /// </summary>
        internal static string UI_UnknownFileRegisterTemplate {
            get {
                return ResourceManager.GetString("UI_UnknownFileRegisterTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only one AsyncFor operation can run at any one time.  To run another, create another instance of AsyncFor..
        /// </summary>
        internal static string Utilities_AsyncFor_ErrorNoConcurrentExecution {
            get {
                return ResourceManager.GetString("Utilities_AsyncFor_ErrorNoConcurrentExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Step count cannot be zero..
        /// </summary>
        internal static string Utilities_AsyncFor_ErrorStepCount0 {
            get {
                return ResourceManager.GetString("Utilities_AsyncFor_ErrorStepCount0", resourceCulture);
            }
        }
    }
}
