Imports System.Reflection
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.UI
Imports SkyEditor.Core.Utilities

Namespace Projects
    Public Class ProjectNode
        Inherits ProjectNodeBase

        Public Sub New(project As Project, parentNode As ProjectNode)
            MyBase.New(project, parentNode)
        End Sub

        Public Sub New(Project As Project, parentNode As ProjectNode, File As Object)
            Me.New(Project, parentNode)
            Me.Item = File
            Me.FileAssemblyQualifiedTypeName = File.GetType.AssemblyQualifiedName
        End Sub

        ''' <summary>
        ''' Gets or sets the path of the file, relative to the project directory.
        ''' </summary>
        Public Property Filename As String

        ''' <summary>
        ''' Assembly qualified name of the type of the file, if this is node is a file.
        ''' </summary>
        ''' <returns></returns>
        Public Property FileAssemblyQualifiedTypeName As String

        ''' <summary>
        ''' Whether or not this node is a directory.  If False, it's a file.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property IsDirectory As Boolean
            Get
                Return Filename Is Nothing AndAlso Item Is Nothing
            End Get
        End Property

        Public Overrides ReadOnly Property Prefix As String
            Get
                If IsDirectory Then
                    Return My.Resources.Language.DirectoryPrefix
                Else
                    Return String.Empty
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets the file at this node, opening it if it hasn't already been.
        ''' </summary>
        ''' <returns></returns>
        Public Async Function GetFile(manager As PluginManager, duplicateMatchSelector As IOHelper.DuplicateMatchSelector) As Task(Of Object)
            If Item Is Nothing Then
                Dim f = GetFilename()
                If String.IsNullOrEmpty(FileAssemblyQualifiedTypeName) Then
                    Return Await IOHelper.OpenObject(f, duplicateMatchSelector, manager).ConfigureAwait(False)
                Else
                    Dim t = ReflectionHelpers.GetTypeByName(FileAssemblyQualifiedTypeName, manager)
                    If t Is Nothing Then
                        Return Await IOHelper.OpenObject(f, duplicateMatchSelector, manager).ConfigureAwait(False)
                    Else
                        Return Await IOHelper.OpenFile(f, t, manager).ConfigureAwait(False)
                    End If
                End If
            Else
                Return Item
            End If
        End Function

        ''' <summary>
        ''' Gets the full path of the file
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFilename() As String
            Return Path.Combine(Path.GetDirectoryName(ParentProject.Filename), Filename?.TrimStart("\"))
        End Function

        Public Overrides Function CreateChildDirectory(directoryName As String) As ProjectNodeBase
            If CanCreateChildDirectory() Then
                Dim node As New ProjectNode(ParentProject, Me)
                node.Name = Name
                Children.Add(node)
                Return node
            Else
                Return Nothing
            End If
        End Function

        Public Function CanCreateFile() As Boolean
            Return Me.IsDirectory AndAlso TypeOf ParentProject Is Project AndAlso DirectCast(ParentProject, Project).CanCreateFile(GetCurrentPath)
        End Function

        Public Function CanDeleteCurrentNode() As Boolean
            If Me.IsDirectory Then
                Return ParentProject.CanDeleteDirectory(GetCurrentPath)
            ElseIf TypeOf ParentProject Is Project Then
                Return DirectCast(ParentProject, Project).CanDeleteFile(GetCurrentPath)
            Else
                Return False
            End If
        End Function

        Public Sub CreateFile(name As String, type As Type)
            If CanCreateFile() AndAlso TypeOf ParentProject Is Project Then
                DirectCast(ParentProject, Project).CreateFile(GetCurrentPath, name, type)
            End If
        End Sub

        Public Sub DeleteCurrentNode()
            If CanDeleteCurrentNode() AndAlso ParentNode IsNot Nothing Then
                ParentNode.Children.Remove(Me)
            End If
        End Sub

    End Class
End Namespace