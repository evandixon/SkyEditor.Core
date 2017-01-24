﻿Imports System.IO
Imports System.Reflection
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.UI
Imports SkyEditor.Core.Utilities

Namespace Projects
    Public Class Solution
        Inherits ProjectBase(Of Project)

#Region "Child Classes"
        Private Class SolutionFile
            Public Property FileFormat As String
            Public Property AssemblyQualifiedTypeName As String
            Public Property Name As String
            Public Property InternalSettings As String 'Serialized settings provider
            Public Property Projects As Dictionary(Of String, String)
            Public Sub New()
                FileFormat = "v2"
            End Sub
        End Class

        Private Class SettingValue
            Public Property AssemblyQualifiedTypeName As String
            Public Property ValueJson As String
        End Class

        Private Class SolutionFileLegacy
            Public Property AssemblyQualifiedTypeName As String
            Public Property Name As String
            ''' <summary>
            ''' Matches solution paths to project files, which are relative to the solution directory.
            ''' </summary>
            ''' <returns></returns>
            Public Property Projects As Dictionary(Of String, String)
            Public Property Settings As Dictionary(Of String, SettingValue)
            Public Sub New()
                Projects = New Dictionary(Of String, String)
                Settings = New Dictionary(Of String, SettingValue)
            End Sub
        End Class

        Public Class ProjectAlreadyExistsException
            Inherits Exception
            Public Sub New()
                MyBase.New
            End Sub
            Public Sub New(Message As String)
                MyBase.New(Message)
            End Sub
        End Class
#End Region

        Public Sub New()
        End Sub

#Region "Events"
        ''' <summary>
        ''' Raised when the solution is saved.
        ''' </summary>
        Public Event FileSaved(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Raised when the solution has been created.
        ''' </summary>
        Public Event Created(sender As Object, e As EventArgs)
        Public Event SolutionBuildStarted(sender As Object, e As EventArgs)
        Public Event SolutionBuildCompleted(sender As Object, e As EventArgs)
        Public Event ProjectAdded(sender As Object, e As ProjectAddedEventArgs)
        Public Event ProjectRemoving(sender As Object, e As ProjectRemovingEventArgs)
        Public Event ProjectRemoved(sender As Object, e As ProjectRemovedEventArgs)

#End Region

#Region "Functions"
        ''' <summary>
        ''' Raises the Created event
        ''' </summary>
        Private Sub RaiseCreated()
            RaiseEvent Created(Me, New EventArgs)
        End Sub

        Private Sub Project_Modified(sender As Object, e As EventArgs)
            UnsavedChanges = True
        End Sub

        ''' <summary>
        ''' Gets the types of projects that can be added in a particular directory.
        ''' </summary>
        ''' <param name="path">Logical solution path in question</param>
        ''' <param name="manager">Instance of the currrent plugin manager</param>
        ''' <returns></returns>
        Public Overridable Function GetSupportedProjectTypes(path As String, manager As PluginManager) As IEnumerable(Of TypeInfo)
            If CanCreateDirectory(path) Then
                Return manager.GetRegisteredTypes(GetType(Project).GetTypeInfo)
            Else
                Return {}
            End If
        End Function

        ''' <summary>
        ''' Gets all projects in the solution, regardless of their parent directory.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetAllProjects() As IEnumerable(Of Project)
            Return GetItems.Values.Where(Function(x) TypeOf x Is Project)
        End Function

        ''' <summary>
        ''' Saves all the projects in the solution
        ''' </summary>
        ''' <param name="provider"></param>
        Public Overridable Sub SaveAllProjects(provider As IIOProvider)
            For Each item In GetAllProjects()
                item.Save(provider)
            Next
        End Sub

        ''' <summary>
        ''' Returns all projects in the solution with the given name, regardless of directories.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Overridable Function GetProjectsByName(Name As String) As IEnumerable(Of Project)
            Return From p In GetAllProjects() Where p.Name.ToLower = Name.ToLower
        End Function

#Region "Solution Logical Filesystem"

        ''' <summary>
        ''' Gets the project at the given path.
        ''' Returns nothing if there is no project at that path.
        ''' </summary>
        ''' <param name="path">Path to look for a project.</param>
        ''' <returns></returns>
        Public Function GetProjectByPath(path As String) As Project
            Return GetItem(path)
        End Function

        ''' <summary>
        ''' Adds the project to the solution.
        ''' </summary>
        ''' <param name="path">Full path of the project</param>
        ''' <param name="project">Project to add</param>
        Public Sub AddProject(path As String, project As Project)
            AddItem(path, project)
            RaiseEvent ProjectAdded(Me, New ProjectAddedEventArgs With {.Path = path, .Project = project})
        End Sub

        Public Overridable Sub CreateProject(parentPath As String, ProjectName As String, ProjectType As Type, manager As PluginManager)
            Dim p = Project.CreateProject(Path.GetDirectoryName(Me.Filename), ProjectName, ProjectType, Me, manager)
            AddProject(FixPath(parentPath) & "/" & ProjectName, p)
        End Sub

        Public Overridable Sub AddExistingProject(parentPath As String, ProjectFilename As String, manager As PluginManager)
            Dim p = Project.OpenProjectFile(ProjectFilename, Me, manager)
            AddProject(FixPath(parentPath) & "/" & p.Name, p)
        End Sub

        Public Overridable Sub DeleteProject(projectPath As String)
            Dim fixedPath = FixPath(projectPath)

            Dim project = GetProjectByPath(fixedPath)
            RaiseEvent ProjectRemoving(Me, New ProjectRemovingEventArgs With {.Project = project})

            DeleteItem(fixedPath)
            RemoveHandler project.Modified, AddressOf Project_Modified

            RaiseEvent ProjectRemoved(Me, New ProjectRemovedEventArgs With {.DirectoryName = Path.GetFileName(fixedPath), .ParentPath = Path.GetDirectoryName(fixedPath), .FullPath = fixedPath})
        End Sub
#End Region

        Public Overridable Function CanCreateProject(Path As String) As Boolean
            Return CanCreateDirectory(Path)
        End Function

        Public Overridable Function CanDeleteProject(projectPath As String) As Boolean
            Return ItemExists(projectPath)
        End Function

#Region "Building"

        ''' <summary>
        ''' Cancels the solution's build, and the builds of any child projects.
        ''' </summary>
        Public Overrides Sub CancelBuild()
            'Cancel the solution's build
            MyBase.CancelBuild()

            'Cancel any projects that are building
            For Each item In GetAllProjects()
                If item.IsBuilding Then
                    item.CancelBuild()
                End If
            Next
        End Sub

        Public Overrides Function CanBuild() As Boolean
            Return Not IsBuilding()
        End Function

        Public Overridable Function GetProjectsToBuild() As IEnumerable(Of Project)
            Return From p In Me.GetAllProjects Where p.CanBuild
        End Function

        Public Overrides Async Function Build() As Task
            If Not IsBuilding() AndAlso CanBuild() Then
                Await Build(GetProjectsToBuild)
            End If
        End Function

        Public Overridable Overloads Async Function Build(projects As IEnumerable(Of Project)) As Task
            RaiseEvent SolutionBuildStarted(Me, New EventArgs)
            Dim toBuild As New Dictionary(Of Project, Boolean)

            For Each item In projects
                If Not item.HasCircularReferences(Me) Then
                    'Stop if the build has been canceled.
                    If IsCancelRequested() Then Exit Function

                    toBuild.Add(item, False)
                Else
                    Throw New ProjectCircularReferenceException
                End If
            Next

            For count = 0 To toBuild.Keys.Count - 1
                'Stop if the build has been canceled.
                If IsCancelRequested() Then Exit Function

                Dim key = toBuild.Keys(count)
                'If this project has not been built
                If Not toBuild(key) Then
                    'Then build the project, but build its dependencies first
                    Await BuildProjects(toBuild, key)
                End If
            Next

            RaiseEvent SolutionBuildCompleted(Me, New EventArgs)
        End Function

        Private Async Function BuildProjects(ToBuild As Dictionary(Of Project, Boolean), CurrentProject As Project) As Task
            Dim buildTasks As New List(Of Task)
            For Each item In From p In CurrentProject.GetReferences(Me) Where p.CanBuild
                'Stop if the build has been canceled.
                If IsCancelRequested() Then Exit Function

                'Start building this project
                buildTasks.Add(BuildProjects(ToBuild, item))
            Next
            Await Task.WhenAll(buildTasks)

            If Not ToBuild(CurrentProject) Then
                'Todo: make sure we won't get here twice, with all the async stuff going on
                ToBuild(CurrentProject) = True
                UpdateBuildLoadingStatus(ToBuild)
                Await CurrentProject.Build
            End If
        End Function

        Private Sub UpdateBuildLoadingStatus(toBuild As Dictionary(Of Project, Boolean))
            Dim built As Integer = (From v In toBuild.Values Where v = True).Count
            Me.BuildProgress = built / toBuild.Count
        End Sub
#End Region

#End Region

#Region "Create"
        ''' <summary>
        ''' Creates and returns a new solution.
        ''' </summary>
        ''' <param name="SolutionDirectory">Directory to store the solution.  Solution will be stored in a sub directory of the one given.</param>
        ''' <param name="SolutionName">Name of the solution.</param>
        ''' <returns></returns>
        Public Shared Function CreateSolution(SolutionDirectory As String, SolutionName As String, manager As PluginManager) As Solution
            Return CreateSolution(SolutionDirectory, SolutionName, GetType(Solution), manager)
        End Function

        ''' <summary>
        ''' Creates and returns a new solution.
        ''' </summary>
        ''' <param name="SolutionDirectory">Directory to store the solution.  Solution will be stored in a sub directory of the one given.</param>
        ''' <param name="SolutionName">Name of the solution.</param>
        ''' <param name="SolutionType">Type of the solution to create.  Must inherit from Solution.</param>
        ''' <returns></returns>
        Public Shared Function CreateSolution(SolutionDirectory As String, SolutionName As String, SolutionType As Type, manager As PluginManager) As Solution
            If SolutionDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(SolutionDirectory))
            End If
            If SolutionName Is Nothing Then
                Throw New ArgumentNullException(NameOf(SolutionName))
            End If
            If SolutionType Is Nothing Then
                Throw New ArgumentNullException(NameOf(SolutionType))
            End If
            If Not ReflectionHelpers.IsOfType(SolutionType, GetType(Solution).GetTypeInfo) Then
                Throw New ArgumentException("SolutionType must inherit from Solution.", NameOf(SolutionType))
            End If

            Dim dir = Path.Combine(SolutionDirectory, SolutionName)
            If Not manager.CurrentIOProvider.DirectoryExists(dir) Then
                manager.CurrentIOProvider.CreateDirectory(dir)
            End If

            Dim output As Solution = ReflectionHelpers.CreateInstance(SolutionType.GetTypeInfo)
            output.CurrentPluginManager = manager
            output.Filename = Path.Combine(dir, SolutionName & ".skysln")
            output.Name = SolutionName
            output.Settings = New SettingsProvider
            output.UnsavedChanges = True
            output.RaiseCreated()

            Return output
        End Function
#End Region

#Region "Open"
        ''' <summary>
        ''' Opens and returns the solution at the given filename.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <returns></returns>
        Public Shared Function OpenSolutionFile(Filename As String, manager As PluginManager) As Solution
            If Filename Is Nothing Then
                Throw New ArgumentNullException(NameOf(Filename))
            End If
            If Not manager.CurrentIOProvider.FileExists(Filename) Then
                Throw New FileNotFoundException("Could not find a file at the given filename.", Filename)
            End If
            'Open the file
            Dim solutionInfo As SolutionFile = Json.DeserializeFromFile(Of SolutionFile)(Filename, manager.CurrentIOProvider)

            'Legacy support
            If String.IsNullOrEmpty(solutionInfo.FileFormat) Then
                Dim legacy As SolutionFileLegacy = Json.DeserializeFromFile(Of SolutionFileLegacy)(Filename, manager.CurrentIOProvider)
                solutionInfo.FileFormat = "1"

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
                    solutionInfo.InternalSettings = s.Serialize
                End If

            End If

            'Get the solution type
            Dim type As TypeInfo = ReflectionHelpers.GetTypeByName(solutionInfo.AssemblyQualifiedTypeName, manager)
            If type Is Nothing Then
                'Default to a generic Solution
                type = GetType(Solution).GetTypeInfo
            End If

            Dim out As Solution = ReflectionHelpers.CreateInstance(type)
            out.Filename = Filename
            out.LoadSolutionFile(solutionInfo, manager)
            out.CurrentPluginManager = manager

            Return out
        End Function

        Private Sub LoadSolutionFile(File As SolutionFile, manager As PluginManager)
            Me.Name = File.Name

            'Load Settings
            Me.Settings = SettingsProvider.Deserialize(File.InternalSettings, manager)

            'Load Projects
            For Each item In File.Projects
                If item.Value Is Nothing Then
                    CreateDirectory(item.Key)
                Else
                    AddItem(FixPath(item.Key), Project.OpenProjectFile(Path.Combine(Path.GetDirectoryName(Filename), item.Value.Replace("/", "\").TrimStart("\")), Me, manager))
                End If
            Next
        End Sub
#End Region

#Region "Save"
        Public Sub Save(provider As IIOProvider)
            Dim file As New SolutionFile
            file.AssemblyQualifiedTypeName = Me.GetType.AssemblyQualifiedName
            file.Name = Me.Name
            file.InternalSettings = Me.Settings.Serialize
            file.Projects = GetSolutionDictionary()
            Json.SerializeToFile(Filename, file, provider)
            RaiseEvent FileSaved(Me, New EventArgs)
            UnsavedChanges = False
        End Sub

        Private Function GetSolutionDictionary() As Dictionary(Of String, String)
            Dim out As New Dictionary(Of String, String)
            For Each item In GetItems()

                If item.Value Is Nothing Then
                    'Directory
                    out.Add(FixPath(item.Key), Nothing)
                Else
                    'File
                    out.Add(FixPath(item.Key), item.Value.Filename.Replace(Path.GetDirectoryName(Filename), ""))
                End If
            Next
            Return out
        End Function

#End Region

    End Class
End Namespace