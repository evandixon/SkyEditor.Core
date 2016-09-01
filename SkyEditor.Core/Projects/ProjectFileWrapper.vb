Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace Projects
    ''' <summary>
    ''' Lazy-loads a file, for use with projects.
    ''' </summary>
    Public Class ProjectFileWrapper

        ''' <summary>
        ''' Creates a new instance of <see cref="ProjectFileWrapper"/>.
        ''' </summary>
        ''' <param name="projectFilename">Filename of the project file.</param>
        ''' <param name="filename">Path of the file, relative to the project directory.</param>
        Public Sub New(projectFilename As String, filename As String)
            Me.ProjectFilename = projectFilename
            Me.Filename = filename
        End Sub

        ''' <summary>
        ''' Creates a new instance of <see cref="ProjectFileWrapper"/>.
        ''' </summary>
        ''' <param name="projectFilename">Filename of the project file.</param>
        ''' <param name="filename">Path of the file, relative to the project directory.</param>
        ''' <param name="fileTypeAssemblyQualifiedName">Assembly qualified name of the file type.</param>
        Public Sub New(projectFilename As String, filename As String, fileTypeAssemblyQualifiedName As String)
            Me.New(projectFilename, filename)
            Me.FileAssemblyQualifiedTypeName = fileTypeAssemblyQualifiedName
        End Sub

        ''' <summary>
        ''' Creates a new instance of <see cref="ProjectFileWrapper"/>.
        ''' </summary>
        ''' <param name="projectFilename">Filename of the project file.</param>
        ''' <param name="filename">Path of the file, relative to the project directory.</param>
        ''' <param name="file">File to contain.</param>
        Public Sub New(projectFilename As String, filename As String, file As Object)
            Me.New(projectFilename, filename)
            Me.FileAssemblyQualifiedTypeName = file.GetType.AssemblyQualifiedName
            Me.File = file
        End Sub

        ''' <summary>
        ''' The contained file.
        ''' </summary>
        ''' <returns></returns>
        Private Property File As Object

        ''' <summary>
        ''' Assembly qualified name of the type of the file.
        ''' </summary>
        Public Property FileAssemblyQualifiedTypeName As String

        ''' <summary>
        ''' Gets or sets the path of the file, relative to the project directory.
        ''' </summary>
        Public Property Filename As String

        ''' <summary>
        ''' Full path of the project.
        ''' </summary>
        Public Property ProjectFilename As String

        ''' <summary>
        ''' Gets the full path of the file
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFilename() As String
            Return Path.Combine(Path.GetDirectoryName(ProjectFilename), Filename.TrimStart("\"))
        End Function

        ''' <summary>
        ''' Gets the contained, opening it if it hasn't already been.
        ''' </summary>
        ''' <returns></returns>
        Public Async Function GetFile(manager As PluginManager, duplicateMatchSelector As IOHelper.DuplicateMatchSelector) As Task(Of Object)
            If File Is Nothing Then
                Dim f = GetFilename()
                If String.IsNullOrEmpty(FileAssemblyQualifiedTypeName) Then
                    File = Await IOHelper.OpenObject(f, duplicateMatchSelector, manager).ConfigureAwait(False)
                Else
                    Dim t = ReflectionHelpers.GetTypeByName(FileAssemblyQualifiedTypeName, manager)
                    If t Is Nothing Then
                        File = Await IOHelper.OpenObject(f, duplicateMatchSelector, manager).ConfigureAwait(False)
                    Else
                        File = Await IOHelper.OpenFile(f, t, manager).ConfigureAwait(False)
                    End If
                End If
            End If
            Return File
        End Function
    End Class
End Namespace
