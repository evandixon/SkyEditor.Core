Imports SkyEditor.Core.Utilities

Namespace Projects
    ''' <summary>
    ''' Defines the common functionality of the nodes of both projects and solutions.
    ''' </summary>
    Public MustInherit Class ProjectNodeBase
        Implements IDisposable
        Implements INotifyPropertyChanged
        Implements IComparable(Of ProjectNodeBase)

        Public Sub New(project As ProjectBase, parentNode As ProjectNodeBase)
            Me.ParentProject = project
            Me.ParentNode = parentNode
            Children = New ObservableCollection(Of ProjectNodeBase)
        End Sub

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub RaisePropertyChanged(sender As Object, propertyName As String)
            RaiseEvent PropertyChanged(sender, New PropertyChangedEventArgs(propertyName))
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the name of the node.
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
            Get
                If IsDirectory Then
                    Return _name
                ElseIf TypeOf Item Is ProjectBase Then
                    Return DirectCast(Item, ProjectBase).Name
                Else
                    Return _name
                End If
            End Get
            Set(value As String)
                If _name <> value Then
                    _name = value
                    RaisePropertyChanged(Me, NameOf(Name))
                End If
            End Set
        End Property
        Dim _name As String

        ''' <summary>
        ''' Gets the prefix of the node
        ''' </summary>
        ''' <returns>A string representing the prefix of the node.  (e.g. "[Directory]" or "[Project]")</returns>
        Public MustOverride ReadOnly Property Prefix As String

        ''' <summary>
        ''' Gets the <see cref="ProjectBase"/> of which the current <see cref="ProjectNodeBase"/> is a child.
        ''' </summary>
        Public Overridable ReadOnly Property ParentProject As ProjectBase

        ''' <summary>
        ''' Gets the <see cref="ProjectNode"/> of which the current <see cref="ProjectNode"/> is a child.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property ParentNode As ProjectNodeBase

        ''' <summary>
        ''' The children of the current <see cref="ProjectNodeBase"/>.
        ''' </summary>
        ''' <returns>An ICollection of <see cref="ProjectNodeBase"/> representing the children of the current <see cref="ProjectNodeBase"/>.</returns>
        ''' <remarks>If the current node has no children, the children of <see cref="Item"/> will be used instead, if <see cref="Item"/> is a <see cref="ProjectBase"/>.</remarks>
        Public Overridable Property Children As ICollection(Of ProjectNodeBase)
            Get
                If _children IsNot Nothing AndAlso _children.Count > 0 Then
                    Return _children
                ElseIf TypeOf Item Is ProjectBase Then
                    Return DirectCast(Item, ProjectBase).Root.Children
                Else
                    Return _children
                End If
            End Get
            Set(value As ICollection(Of ProjectNodeBase))
                _children = value
            End Set
        End Property
        Dim _children As ICollection(Of ProjectNodeBase)

        ''' <summary>
        ''' Gets or sets the object that the current <see cref="ProjectNodeBase"/> represents.
        ''' </summary>
        ''' <remarks>For solutions, this is a project.  For projects, this is a file.  If null, the current node is a directory.</remarks>
        Public Overridable Property Item As Object

        ''' <summary>
        ''' Whether or not this node is a directory.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property IsDirectory As Boolean
            Get
                Return Item Is Nothing
            End Get
        End Property

#End Region

#Region "Functions"

        ''' <summary>
        ''' Gets the project path of the parent node.
        ''' </summary>
        ''' <returns>A string representing the location of the parent node, expressed as a path, or an empty string if this is the root node.</returns>
        Public Function GetParentPath() As String
            If ParentNode Is Nothing Then
                Return ""
            Else
                Return ParentNode.GetParentPath & "/"
            End If
        End Function

        ''' <summary>
        ''' Gets the project path of the current node.
        ''' </summary>
        ''' <returns>A string representing the location of the current node, expressed as a path.</returns>
        Public Function GetCurrentPath() As String
            Return GetParentPath() & "/" & Name
        End Function

        ''' <summary>
        ''' Determines whether or not a directory node can be created as a child of the current node.
        ''' </summary>
        ''' <returns>A boolean indicating whether or not a directory node can be created as a child of the current node</returns>
        Public Function CanCreateChildDirectory() As Boolean
            Return Me.IsDirectory AndAlso ParentProject IsNot Nothing AndAlso ParentProject.CanCreateDirectory(Me.GetCurrentPath)
        End Function

        ''' <summary>
        ''' Creates a new directory node as a child of the current node.
        ''' </summary>
        ''' <param name="name">Name of the new directory.</param>
        ''' <returns>The newly created child directory node, or null if the directory cannot be created.</returns>
        Public MustOverride Function CreateChildDirectory(name As String) As ProjectNodeBase

        Public Function CompareTo(other As ProjectNodeBase) As Integer Implements IComparable(Of ProjectNodeBase).CompareTo
            Return Me.Name.CompareTo(other.Name)
        End Function

#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls


        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    For Each child In Children
                        If TypeOf child Is IDisposable Then
                            DirectCast(child, IDisposable).Dispose()
                        End If
                    Next
                    If Item IsNot Nothing AndAlso TypeOf Item Is IDisposable Then
                        DirectCast(Item, IDisposable).Dispose()
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
