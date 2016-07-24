Namespace IO
    ''' <summary>
    ''' Defines the common functionality of both projects and solutions.
    ''' </summary>
    Public MustInherit Class ProjectBase
        Implements INotifyModified
        Implements INotifyPropertyChanged
        Implements IDisposable

#Region "Events"
        Public Event Modified(sender As Object, e As EventArgs) Implements INotifyModified.Modified
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Event BuildStatusChanged(sender As Object, e As ProjectBuildStatusChanged)
#End Region

#Region "Event Handlers"
        Private Sub _root_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _root.PropertyChanged
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Root)))
        End Sub
#End Region

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the instance of the current plugin manager
        ''' </summary>
        ''' <returns>The instance of the current plugin manager</returns>
        Public Property CurrentPluginManager As PluginManager

        ''' <summary>
        ''' Gets or sets the name of the project
        ''' </summary>
        ''' <returns>A string containing the name of the project</returns>
        Public Property Name As String

        ''' <summary>
        ''' Gets or sets the full path of the project file
        ''' </summary>
        ''' <returns>A string containing the absolute path of the project file</returns>
        Public Property Filename As String

        ''' <summary>
        ''' The project settings
        ''' </summary>
        ''' <returns>A <see cref="SettingsProvider"/> containing the settings for the project.</returns>
        Public Property Settings As SettingsProvider

        ''' <summary>
        ''' Gets or sets whether or not there are unsaved changes
        ''' </summary>
        ''' <returns></returns>
        Public Property UnsavedChanges As Boolean
            Get
                Return _unsavedChanges
            End Get
            Set(value As Boolean)
                _unsavedChanges = value
                If value Then
                    RaiseEvent Modified(Me, New EventArgs)
                End If
            End Set
        End Property
        Dim _unsavedChanges As Boolean

        ''' <summary>
        ''' Gets or sets an individual setting in the project's settings.
        ''' </summary>
        ''' <param name="SettingName">Name of the setting to get or set.</param>
        ''' <returns>The value of the setting with the name <paramref name="SettingName"/></returns>
        Protected Property Setting(SettingName As String) As Object
            Get
                Return Settings.GetSetting(SettingName)
            End Get
            Set(value As Object)
                Settings.SetSetting(SettingName, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the root node of the project.
        ''' </summary>
        Public Property Root As ProjectNodeBase
            Get
                Return _root
            End Get
            Set(value As ProjectNodeBase)
                If _root IsNot value Then
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Root)))
                End If
            End Set
        End Property
        Private WithEvents _root As ProjectNodeBase

        ''' <summary>
        ''' Gets or sets the progress of the current project's build.
        ''' </summary>
        ''' <returns>A percentage indicating the progression of the build.</returns>
        Public Property BuildProgress As Single
            Get
                Return _buildProgress
            End Get
            Set(value As Single)
                If _buildProgress <> value Then
                    _buildProgress = value
                    RaiseEvent BuildStatusChanged(Me, New ProjectBuildStatusChanged With {.Progress = BuildProgress, .StatusMessage = BuildStatusMessage})
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(BuildProgress)))
                End If
            End Set
        End Property
        Private _buildProgress As Single

        ''' <summary>
        ''' Gets or sets the current build message.
        ''' </summary>
        ''' <returns>A string indicating what is being done in the build.</returns>
        Public Property BuildStatusMessage As String
            Get
                Return _buildStatusMessage
            End Get
            Set(value As String)
                If _buildStatusMessage <> value Then
                    _buildStatusMessage = value
                    RaiseEvent BuildStatusChanged(Me, New ProjectBuildStatusChanged With {.Progress = BuildProgress, .StatusMessage = BuildStatusMessage})
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(BuildStatusMessage)))
                End If
            End Set
        End Property
        Private _buildStatusMessage As String

        ''' <summary>
        ''' Gets or sets whether or not the build progress is indeterminate.
        ''' </summary>
        ''' <returns>A boolean indicating whether or not the build progress is indeterminate.</returns>
        Public Property IsBuildProgressIndeterminate As Boolean
            Get
                Return _isBuildProgressIndeterminate
            End Get
            Set(value As Boolean)
                If _isBuildProgressIndeterminate <> value Then
                    _isBuildProgressIndeterminate = value
                    RaiseEvent BuildStatusChanged(Me, New ProjectBuildStatusChanged With {.Progress = BuildProgress, .StatusMessage = BuildStatusMessage})
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(IsBuildProgressIndeterminate)))
                End If
            End Set
        End Property
        Dim _isBuildProgressIndeterminate As Boolean
#End Region

        ''' <summary>
        ''' Determines whether or not a node at the given path exists.
        ''' </summary>
        ''' <param name="nodePath">Path of the node to find.</param>
        ''' <returns>A boolean indicating whether or not a node exists at the requested path.</returns>
        Public Function NodeExists(nodePath As String) As Boolean
            Return GetNodeOrNull(nodePath) IsNot Nothing
        End Function

        ''' <summary>
        ''' Determines whether or not a directory node at the given path exists.
        ''' </summary>
        ''' <param name="nodePath">Path of the directory to find.</param>
        ''' <returns>A boolean indicating whether or not a directory node exists at the requested path.</returns>
        Public Function DirectoryExists(nodePath As String) As Boolean
            Dim node As ProjectNodeBase = GetNodeOrNull(nodePath)
            Return node IsNot Nothing AndAlso node.IsDirectory
        End Function

        ''' <summary>
        ''' Gets the project node at the given path, or returns null if it cannot be found.
        ''' </summary>
        ''' <param name="nodePath">Path of the node to find.</param>
        ''' <returns>The requested <see cref="ProjectNode"/>, or null if it cannot be found.</returns>
        Public Function GetNodeOrNull(nodePath As String) As ProjectNode
            If String.IsNullOrEmpty(nodePath) Then
                Return Root
            Else
                Dim pathParts = nodePath.Replace("\", "/").TrimStart("/").Split("/")
                Dim current As ProjectNode = Root
                For Each item In pathParts
                    Dim child = current.Children.Where(Function(x) x.Name.ToLower = item.ToLower).FirstOrDefault
                    If child Is Nothing Then
                        'The node cannot be found.
                        current = Nothing
                        Exit For
                    Else
                        current = child
                    End If
                Next
                Return current
            End If
        End Function

        ''' <summary>
        ''' Determines whether or not a directory node can be created at the given path.
        ''' </summary>
        ''' <param name="parentPath">Path of the node in which a directory is to be created.</param>
        ''' <returns>A boolean indicating whether or not a directory is allowed to be created in the given path.</returns>
        Public Overridable Function CanCreateDirectory(parentPath As String) As Boolean
            Return NodeExists(parentPath)
        End Function

        ''' <summary>
        ''' Determines whether or not a directory node located at the given path is allowed to be deleted.
        ''' </summary>
        ''' <param name="directoryPath">Path of the directory node to be deleted.</param>
        ''' <returns>A boolean indicating whether or not a directory is allowed to be deleted from the given path.</returns>
        Public Overridable Function CanDeleteDirectory(directoryPath As String) As Boolean
            Return NodeExists(directoryPath)
        End Function

        ''' <summary>
        ''' Creates a directory if it does not exist.
        ''' </summary>
        ''' <param name="path">Path of the new directory node.</param>
        ''' <returns>The newly created directory node, or nothing if the directory cannot be created.</returns>
        ''' <exception cref="NullReferenceException">Thrown if <see cref="Root"/> or the <see cref="ProjectNodeBase.Children"/> property of a node is null.</exception>
        Public Function CreateDirectory(path As String) As ProjectNodeBase
            Return CreateDirectory(Root, path)
        End Function

        ''' <summary>
        ''' Creates a directory if it does not exist.
        ''' </summary>
        ''' <param name="path">Path of the new directory node.</param>
        ''' <returns>The newly created directory node, or nothing if the directory cannot be created.</returns>
        ''' <exception cref="NullReferenceException">Thrown if <param name="rootNode"/> or the <see cref="ProjectNodeBase.Children"/> property of a node is null.</exception>
        Public Function CreateDirectory(rootNode As ProjectNodeBase, path As String) As ProjectNodeBase
            If Root Is Nothing Then
                Throw New NullReferenceException(My.Resources.Language.ErrorProjectNullRoot)
            End If
            Dim pathParts = path.Replace("\", "/").TrimStart("/").Split("/")
            Dim current = Root
            For Each item In pathParts
                Dim child = current.Children.Where(Function(x) x.IsDirectory AndAlso x.Name.ToLower = item.ToLower).FirstOrDefault
                If child Is Nothing Then
                    'Directory does not exist.  Create it.
                    current = current.CreateChildDirectory(item)
                    If current Is Nothing Then
                        'Directory cannot be created.  Return nothing.
                        current = Nothing
                        Exit For
                    End If
                Else
                    current = child
                End If
            Next
            Return current
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If Root IsNot Nothing Then
                        Root.Dispose()
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
