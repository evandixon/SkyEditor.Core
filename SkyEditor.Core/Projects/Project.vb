Imports System.Reflection
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace Projects
    Public Class Project
        Inherits ProjectBase(Of ProjectFileWrapper)
        Implements ISavable

        Public Sub New()
            Settings = New SettingsProvider
        End Sub


#Region "Child Classes"
        Private Class SettingValue
            Public Property AssemblyQualifiedTypeName As String
            Public Property ValueJson As String
        End Class

        Private Class FileValue
            Public Property AssemblyQualifiedTypeName As String
            Public Property Filename As String
        End Class

        Private Class ProjectFile
            Public Const CurrentVersion As String = "v2"
            Public Property FileFormat As String
            Public Property AssemblyQualifiedTypeName As String
            Public Property Name As String
            ''' <summary>
            ''' Matches project paths to project files, which are relative to the project directory.
            ''' </summary>
            ''' <returns></returns>
            Public Property Files As Dictionary(Of String, FileValue)
            Public Property InternalSettings As String
            Public Sub New()
                Files = New Dictionary(Of String, FileValue)
            End Sub
        End Class

        Private Class ProjectFileLegacy
            Public Property AssemblyQualifiedTypeName As String
            Public Property Name As String
            ''' <summary>
            ''' Matches project paths to project files, which are relative to the project directory.
            ''' </summary>
            ''' <returns></returns>
            Public Property Files As Dictionary(Of String, FileValue)
            Public Property Settings As Dictionary(Of String, SettingValue)
            Public Sub New()
                Files = New Dictionary(Of String, FileValue)
                Settings = New Dictionary(Of String, SettingValue)
            End Sub
        End Class

#End Region

#Region "Events"
        ''' <summary>
        ''' Raised when the project has been opened.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Public Event ProjectOpened(sender As Object, e As EventArgs)
        Public Event FileSaved(sender As Object, e As EventArgs) Implements ISavable.FileSaved
        Public Event FileAdded(sender As Object, e As ProjectFileAddedEventArgs)
        Public Event FileRemoved(sender As Object, e As ProjectFileRemovedEventArgs)
#End Region

        Public Property ParentSolution As Solution

        ''' <summary>
        ''' List of the names of all projects in the current solution this project references.
        ''' </summary>
        ''' <returns></returns>
        Public Property ProjectReferences As List(Of String)
            Get
                If Setting("ProjectReferences") Is Nothing Then
                    Setting("ProjectReferences") = New List(Of String)
                End If
                Return Setting("ProjectReferences")
            End Get
            Set(value As List(Of String))
                Setting("ProjectReferences") = value
            End Set
        End Property

#Region "Functions"

        ''' <summary>
        ''' Gets the file at the given path.
        ''' Returns nothing if there is no file at that path.
        ''' </summary>
        ''' <param name="path">Path to look for a file.</param>
        ''' <returns></returns>
        Public Async Function GetFileByPath(path As String, manager As PluginManager, duplicateMatchSelector As IOHelper.DuplicateMatchSelector) As Task(Of Object)
            Return Await (GetItem(path)?.GetFile(manager, duplicateMatchSelector))
        End Function

        ''' <summary>
        ''' Gets the filename of the file at the given project path.
        ''' </summary>
        ''' <param name="path">Path of the project file of which to get the physical filename.</param>
        ''' <returns>The physical filename of the project file.</returns>
        Public Function GetFilename(path As String) As String
            Return GetItem(path).GetFilename
        End Function

        Public Overridable Function CanCreateFile(Path As String) As Boolean
            Return CanCreateDirectory(Path)
        End Function

        Public Overridable Function CanAddExistingFile(Path As String) As Boolean
            Return CanCreateFile(Path)
        End Function

        Public Overridable Function GetSupportedFileTypes(Path As String, manager As PluginManager) As IEnumerable(Of TypeInfo)
            If CanCreateDirectory(Path) Then
                Return IOHelper.GetCreatableFileTypes(manager)
            Else
                Return {}
            End If
        End Function

        Public Overridable Sub CreateFile(parentPath As String, name As String, fileType As Type)
            If Not DirectoryExists(parentPath) Then
                CreateDirectory(parentPath)
            End If

            Dim fixedPath = FixPath(parentPath)

            Dim fileObj As ICreatableFile = ReflectionHelpers.CreateInstance(fileType.GetTypeInfo)
            fileObj.CreateFile(name)
            fileObj.Filename = Path.Combine(Path.GetDirectoryName(Me.Filename), parentPath.Replace("/", "\").TrimStart("\"), name)

            AddItem(fixedPath & "/" & name, New ProjectFileWrapper(Me.Filename, fileObj.Filename, fileObj))
        End Sub

        Public Overridable Function IsFileSupported(ParentProjectPath As String, Filename As String)
            Return CanAddExistingFile(ParentProjectPath)
        End Function

        ''' <summary>
        ''' Gets the project path of the imported file.
        ''' </summary>
        ''' <param name="parentProjectPath">Directory to put the imported file.</param>
        ''' <param name="filename">Full path of the file to import.</param>
        ''' <returns></returns>
        Protected Overridable Function GetImportedFilePath(parentProjectPath As String, filename As String) As String
            Return FixPath(Path.Combine(parentProjectPath, Path.GetFileName(filename)))
        End Function

        Public Overridable Function GetImportIOFilter(ParentProjectPath As String, manager As PluginManager) As String
            Return $"{My.Resources.Language.AllFiles} (*.*)|*.*" 'manager.IOFiltersString
        End Function


        Public Overridable Sub AddExistingFile(parentPath As String, FilePath As String, provider As IOProvider)
            Dim fixedParentPath = FixPath(parentPath)
            Dim importedName = GetImportedFilePath(parentPath, FilePath)

            'Copy the file
            Dim source = FilePath
            Dim dest = Path.Combine(Path.GetDirectoryName(Me.Filename), importedName.Replace("/", "\").TrimStart("\"))
            If Not source.Replace("\", "/").ToLower = dest.Replace("\", "/").ToLower Then
                provider.CopyFile(FilePath, dest)
            End If

            'Add the file
            Dim relativePath = dest.Replace(Path.GetDirectoryName(Me.Filename), "").Replace("\", "/").TrimStart("/")
            Dim wrapper As New ProjectFileWrapper(Me.Filename, relativePath)
            AddItem(importedName, wrapper)
            RaiseEvent FileAdded(Me, New ProjectFileAddedEventArgs With {.Filename = Path.GetFileName(FilePath), .FullFilename = dest})
        End Sub

        Public Function FileExists(path As String) As Boolean
            Return ItemExists(path)
        End Function

        Public Overridable Function CanDeleteFile(path As String) As Boolean
            Return ItemExists(path)
        End Function

        Public Overridable Sub DeleteFile(path As String)
            DeleteItem(path)
        End Sub

        Public Overridable Function GetRootDirectory() As String
            Return Path.GetDirectoryName(Me.Filename)
        End Function

        Public Function GetReferences(Solution As Solution) As IEnumerable(Of Project)
            Dim out As New List(Of Project)
            For Each item In ProjectReferences
                Dim p = Solution.GetProjectsByName(item).FirstOrDefault
                If p IsNot Nothing Then
                    out.Add(p)
                End If
            Next
            Return out
        End Function

        ''' <summary>
        ''' Returns whether or not this project contains a circular reference back to itself.
        ''' It does not detect whether other projects this one references have their own circular references.
        ''' </summary>
        ''' <param name="Solution"></param>
        ''' <returns></returns>
        Public Function HasCircularReferences(Solution As Solution) As Boolean
            Dim tree As New List(Of Project)
            FillReferenceTree(Solution, tree, Me)
            Return tree.Contains(Me)
        End Function

        ''' <summary>
        ''' Fills Tree with all the references of the current item.
        ''' Stops if the last item added is the current instance of project.
        ''' </summary>
        ''' <param name="Solution"></param>
        ''' <param name="Tree"></param>
        ''' <param name="CurrentItem"></param>
        Private Sub FillReferenceTree(Solution As Solution, Tree As List(Of Project), CurrentItem As Project)
            For Each item In CurrentItem.GetReferences(Solution)
                Tree.Add(item)
                If item Is Me Then
                    Exit Sub
                Else
                    If Not item.HasCircularReferences(Solution) Then
                        FillReferenceTree(Solution, Tree, item)
                    End If
                End If
            Next
        End Sub
#End Region

#Region "Create New"
        ''' <summary>
        ''' Creates and returns a new Project.
        ''' </summary>
        ''' <param name="ProjectDirectory">Directory to store the Project.  Project will be stored in a sub directory of the one given.</param>
        ''' <param name="ProjectName">Name of the Project.</param>
        ''' <returns></returns>
        Public Shared Function CreateProject(ProjectDirectory As String, ProjectName As String, parent As Solution, manager As PluginManager) As Project
            Return CreateProject(ProjectDirectory, ProjectName, GetType(Project), parent, manager)
        End Function

        ''' <summary>
        ''' Creates and returns a new Project.
        ''' </summary>
        ''' <param name="ProjectDirectory">Directory to store the Project.  Project will be stored in a sub directory of the one given.</param>
        ''' <param name="ProjectName">Name of the Project.</param>
        ''' <param name="ProjectType">Type of the Project to create.  Must inherit from Project.</param>
        ''' <returns></returns>
        Public Shared Function CreateProject(ProjectDirectory As String, ProjectName As String, ProjectType As Type, parent As Solution, manager As PluginManager) As Project
            If ProjectDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(ProjectDirectory))
            End If
            If ProjectName Is Nothing Then
                Throw New ArgumentNullException(NameOf(ProjectName))
            End If
            If ProjectType Is Nothing Then
                Throw New ArgumentNullException(NameOf(ProjectType))
            End If
            If Not ReflectionHelpers.IsOfType(ProjectType, GetType(Project).GetTypeInfo) Then
                Throw New ArgumentException("ProjectType must inherit from Project.", NameOf(ProjectType))
            End If

            Dim dir = Path.Combine(ProjectDirectory, ProjectName)
            If Not manager.CurrentIOProvider.DirectoryExists(dir) Then
                manager.CurrentIOProvider.CreateDirectory(dir)
            End If

            Dim output As Project = ReflectionHelpers.CreateInstance(ProjectType.GetTypeInfo)
            output.Filename = Path.Combine(dir, ProjectName & ".skyproj")
            output.Name = ProjectName
            output.CurrentPluginManager = manager
            output.ParentSolution = parent

            Dim projFile As New ProjectFile With {.Name = ProjectName, .AssemblyQualifiedTypeName = ProjectType.AssemblyQualifiedName}
            projFile.FileFormat = ProjectFile.CurrentVersion
            output.LoadProjectFile(projFile, manager)

            Return output
        End Function
#End Region

#Region "Open"
        ''' <summary>
        ''' Opens and returns the solution at the given filename.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <returns></returns>
        Public Shared Function OpenProjectFile(Filename As String, parent As Solution, manager As PluginManager) As Project
            If Filename Is Nothing Then
                Throw New ArgumentNullException(NameOf(Filename))
            End If
            If Not manager.CurrentIOProvider.FileExists(Filename) Then
                Throw New FileNotFoundException("Could not find a file at the given filename.", Filename)
            End If

            Dim projectInfo As ProjectFile = Json.DeserializeFromFile(Of ProjectFile)(Filename, manager.CurrentIOProvider)
            'Legacy support
            If String.IsNullOrEmpty(projectInfo.FileFormat) Then
                Dim legacy As ProjectFileLegacy = Json.DeserializeFromFile(Of ProjectFileLegacy)(Filename, manager.CurrentIOProvider)
                projectInfo.FileFormat = "1"

                'Read the settings
                If legacy.Settings IsNot Nothing Then
                    Dim s As New SettingsProvider
                    For Each item In legacy.Settings
                        Dim valueType = ReflectionHelpers.GetTypeByName(item.Value.AssemblyQualifiedTypeName, manager)
                        If valueType IsNot Nothing Then
                            s.SetSetting(item.Key, Json.Deserialize(valueType.AsType, item.Value.ValueJson))
                        End If
                        'If the valueType IS nothing, then the type can't be found, and we won't save the setting
                    Next
                    projectInfo.InternalSettings = s.Serialize
                End If

            End If
            Dim type As TypeInfo = ReflectionHelpers.GetTypeByName(projectInfo.AssemblyQualifiedTypeName, manager)
            If type Is Nothing Then
                'Default to Project if the saved type cannot be found
                type = GetType(Project).GetTypeInfo
            End If

            Dim out As Project = ReflectionHelpers.CreateInstance(type)
            out.Filename = Filename
            out.LoadProjectFile(projectInfo, manager)
            out.CurrentPluginManager = manager
            out.ParentSolution = parent

            Return out
        End Function

        Private Sub LoadProjectFile(File As ProjectFile, manager As PluginManager)
            Me.Name = File.Name

            'Load Settings
            Settings = SettingsProvider.Deserialize(File.InternalSettings, manager)

            'Load Files
            For Each item In File.Files
                If item.Value Is Nothing Then
                    CreateDirectory(item.Key)
                Else
                    AddItem(FixPath(item.Key), New ProjectFileWrapper(Me.Filename, item.Value.Filename, item.Value.AssemblyQualifiedTypeName))
                End If
            Next
            RaiseEvent ProjectOpened(Me, New EventArgs)
        End Sub
#End Region

#Region "Save"
        Public Sub Save(provider As IOProvider) Implements ISavable.Save
            Dim file As New ProjectFile
            file.FileFormat = ProjectFile.CurrentVersion
            file.AssemblyQualifiedTypeName = Me.GetType.AssemblyQualifiedName
            file.Name = Me.Name
            file.InternalSettings = Me.Settings.Serialize
            file.Files = GetProjectDictionary()
            Json.SerializeToFile(Filename, file, provider)
            RaiseEvent FileSaved(Me, New EventArgs)
        End Sub

        Private Function GetProjectDictionary() As Dictionary(Of String, FileValue)
            Dim out As New Dictionary(Of String, FileValue)
            For Each item In Me.GetItems
                If item.Value Is Nothing Then
                    'Directory
                    out.Add(FixPath(item.Key), Nothing)
                Else
                    'File
                    out.Add(FixPath(item.Key), New FileValue With {.Filename = item.Value.Filename.Replace(Path.GetDirectoryName(Filename), ""), .AssemblyQualifiedTypeName = item.Value.FileAssemblyQualifiedTypeName})
                End If
            Next
            Return out
        End Function
#End Region

    End Class
End Namespace

