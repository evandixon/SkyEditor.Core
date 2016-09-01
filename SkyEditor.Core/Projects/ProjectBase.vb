Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace Projects
    ''' <summary>
    ''' Defines the common functionality of both projects and solutions.
    ''' </summary>
    ''' <typeparam name="T">Type of project items.</typeparam>
    Public MustInherit Class ProjectBase(Of T)
        Implements INotifyModified
        Implements IDisposable
        Implements IReportProgress

        Public Sub New()
            Items = New Dictionary(Of String, T)
        End Sub

#Region "Events"
        Public Event Modified(sender As Object, e As EventArgs) Implements INotifyModified.Modified
        Public Event BuildStatusChanged(sender As Object, e As ProgressReportedEventArgs) Implements IReportProgress.ProgressChanged
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
        ''' Matches logical paths to items
        ''' </summary>
        ''' <remarks>Key: logical path; Value: Item (Projects for Solutions, Files for Projects).
        ''' If the value is null, the path is an empty directory.
        ''' 
        ''' Example Paths (In form: "{Path}"/{Value})
        ''' ""/null - Represents the root directory (not allowed)
        ''' "Test"/null - directory
        ''' "Test/Ing"/null - directory
        ''' "Test/File"/[GenericFile] - File of type GenericFile, named "File", in directory "Test"</remarks>
        Protected Property Items As Dictionary(Of String, T)

#End Region

#Region "Build"

        ''' <summary>
        ''' Gets or sets the status of the project's current build.
        ''' </summary>
        ''' <returns>The status of the project's current build.</returns>
        Public Property BuildStatus As BuildStatus
            Get
                Return _buildStatus
            End Get
            Protected Set(value As BuildStatus)
                _buildStatus = value
            End Set
        End Property
        Dim _buildStatus As BuildStatus

        ''' <summary>
        ''' Gets or sets the progress of the current project's build.
        ''' </summary>
        ''' <returns>A percentage indicating the progression of the build.</returns>
        Public Property BuildProgress As Single Implements IReportProgress.Progress
            Get
                Return _buildProgress
            End Get
            Set(value As Single)
                If _buildProgress <> value Then
                    _buildProgress = value
                    RaiseEvent BuildStatusChanged(Me, New ProgressReportedEventArgs With {.Progress = BuildProgress, .Message = BuildStatusMessage})
                End If
            End Set
        End Property
        Private _buildProgress As Single

        ''' <summary>
        ''' Gets or sets the current build message.
        ''' </summary>
        ''' <returns>A string indicating what is being done in the build.</returns>
        Public Property BuildStatusMessage As String Implements IReportProgress.Message
            Get
                Return _buildStatusMessage
            End Get
            Set(value As String)
                If _buildStatusMessage <> value Then
                    _buildStatusMessage = value
                    RaiseEvent BuildStatusChanged(Me, New ProgressReportedEventArgs With {.Progress = BuildProgress, .Message = BuildStatusMessage})
                End If
            End Set
        End Property
        Private _buildStatusMessage As String

        ''' <summary>
        ''' Gets or sets whether or not the build progress is indeterminate.
        ''' </summary>
        ''' <returns>A boolean indicating whether or not the build progress is indeterminate.</returns>
        Public Property IsBuildProgressIndeterminate As Boolean Implements IReportProgress.IsIndeterminate
            Get
                Return _isBuildProgressIndeterminate
            End Get
            Set(value As Boolean)
                If _isBuildProgressIndeterminate <> value Then
                    _isBuildProgressIndeterminate = value
                    RaiseEvent BuildStatusChanged(Me, New ProgressReportedEventArgs With {.Progress = BuildProgress, .Message = BuildStatusMessage})
                End If
            End Set
        End Property
        Dim _isBuildProgressIndeterminate As Boolean

        ''' <summary>
        ''' Gets whether or not a build is currently running.
        ''' </summary>
        ''' <returns>A boolean indicating whether or not the <see cref="BuildStatus"/> indicates a running build.</returns>
        Public Function IsBuilding() As Boolean
            Return BuildStatus = BuildStatus.Building OrElse BuildStatus = BuildStatus.Canceling
        End Function

        ''' <summary>
        ''' Gets whether or not a build cancelation has been requested.
        ''' </summary>
        ''' <returns>A boolean indicating whether or not the <see cref="BuildStatus"/> indicates the build should be canceled.</returns>
        Public Function IsCancelRequested() As Boolean
            Return BuildStatus = BuildStatus.Canceling
        End Function

        ''' <summary>
        ''' Builds the project, the project is not already building.
        ''' </summary>
        Public Overridable Function Build() As Task
            If Not IsBuilding() Then
                BuildStatus = BuildStatus.Done
            End If
            Return Task.FromResult(0)
        End Function

        ''' <summary>
        ''' Requests that the current build be canceled.
        ''' </summary>
        ''' <remarks>This only requests that the build be canceled.  It is up to the implementation of the current project whether or not the build actaully stops.</remarks>
        Public Overridable Sub CancelBuild()
            If IsBuilding() Then
                BuildStatus = BuildStatus.Canceling
            End If
        End Sub

        ''' <summary>
        ''' Gets whether or not a build can be run.
        ''' </summary>
        ''' <returns>A boolean indicating whether or not a build can be started.</returns>
        Public Overridable Function CanBuild() As Boolean
            Return Not IsBuilding()
        End Function
#End Region

#Region "Logical Filesystem"

        ''' <summary>
        ''' Standardizes a path.
        ''' </summary>
        ''' <param name="path">Path to standardize.</param>
        ''' <returns>A standardized path.</returns>
        Protected Function FixPath(path As String) As String
            Return path.Replace("\", "/").Trim("/")
        End Function

        ''' <summary>
        ''' Gets paths and objects in the logical filesystem.
        ''' </summary>
        ''' <param name="path">Path of child items.</param>
        ''' <param name="recursive">Whether or not to search child directories.</param>
        ''' <param name="getDirectories">Whether to get files or directories.</param>
        ''' <returns>An instance of <see cref="IEnumerable(Of KeyValuePair(Of String,T))"/>, where each key is the full path and each value is the corresponding object, or null if the path is a directory.</returns>
        Private Function GetItemsInternal(path As String, recursive As Boolean, getDirectories As Boolean) As IEnumerable(Of KeyValuePair(Of String, T))
            Dim fixedPath As String

            If getDirectories Then
                fixedPath = FixPath(path).ToLowerInvariant & "/"
            Else
                fixedPath = FixPath(path).ToLowerInvariant
            End If

            'Given directory structure of:
            'Test
            'Test/Ing
            'Blarg/Test
            'Test/Ing/Test
            '
            'And an path of Test

            If recursive Then
                'Should return "Test/Ing" and "Test/Ing/Test
                Return Items.Where(Function(x) x.Key.ToLowerInvariant.StartsWith(fixedPath) AndAlso
                                       ((getDirectories AndAlso x.Value Is Nothing) OrElse (Not getDirectories AndAlso x.Value IsNot Nothing))).
                            OrderBy(Function(x) x.Key, New DirectoryStructureComparer)
            Else
                'Should return "Test/Ing"
                Return Items.Where(Function(x) x.Key.ToLowerInvariant.StartsWith(fixedPath) AndAlso
                                       Not x.Key.ToLowerInvariant.Replace(fixedPath, "").Replace("\", "/").Contains("/") AndAlso 'Filters anything with a slash after the parent directory
                                       ((getDirectories AndAlso x.Value Is Nothing) OrElse (Not getDirectories AndAlso x.Value IsNot Nothing))).
                             OrderBy(Function(x) x.Key, New DirectoryStructureComparer)
            End If

        End Function

        Protected Function ItemExists(path As String) As Boolean
            Dim fixedPath = FixPath(path)
            Return Items.Any(Function(x) x.Key.ToLowerInvariant = fixedPath)
        End Function

        ''' <summary>
        ''' Gets the item at the given path.
        ''' </summary>
        ''' <param name="path">Path of the item.</param>
        ''' <returns>The item at the given path, or null if there is no item at the given path.</returns>
        Protected Function GetItem(path As String) As T
            Return GetItemsInternal(path, True, False).FirstOrDefault.Value
        End Function

        Protected Sub AddItem(path As String, item As T)
            If ItemExists(path) Then
                Throw New DuplicateItemException(path)
            Else
                Dim fixedPath = FixPath(path)
                Items.Add(path, item)
            End If
        End Sub

        ''' <summary>
        ''' Deletes a directory or item at the given path.
        ''' </summary>
        ''' <param name="path">Path of the directory or item to delete.</param>
        Protected Sub DeleteItem(path As String)
            Dim fixedPath = FixPath(path)
            Items.Remove(Items.Where(Function(x) x.Key.ToLowerInvariant = fixedPath).Select(Function(x) x.Key).FirstOrDefault)
        End Sub

#Region "Directories"
        ''' <summary>
        ''' Determines whether or not a directory at the given path exists.
        ''' </summary>
        ''' <param name="path">Path of the directory to find.</param>
        ''' <returns>A boolean indicating whether or not a directory exists at the requested path.</returns>
        Public Function DirectoryExists(path As String) As Boolean
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Dim pathFixed = FixPath(path)
            Return Items.Keys.Any(Function(x) x.ToLowerInvariant = pathFixed.ToLowerInvariant)
        End Function

        ''' <summary>
        ''' Determines whether or not a directory node can be created at the given path.
        ''' </summary>
        ''' <param name="parentPath">Path of the node in which a directory is to be created.</param>
        ''' <returns>A boolean indicating whether or not a directory is allowed to be created in the given path.</returns>
        Public Overridable Function CanCreateDirectory(parentPath As String) As Boolean
            Return True
        End Function

        ''' <summary>
        ''' Creates a directory if it does not exist.
        ''' </summary>
        ''' <param name="path">Path of the new directory.</param>
        Public Sub CreateDirectory(path As String)
            Dim fixedPath = FixPath(path)
            If Not DirectoryExists(fixedPath) Then
                Items.Add(fixedPath, Nothing)
            End If
        End Sub

        ''' <summary>
        ''' Determines whether or not a directory node located at the given path is allowed to be deleted.
        ''' </summary>
        ''' <param name="directoryPath">Path of the directory node to be deleted.</param>
        ''' <returns>A boolean indicating whether or not a directory is allowed to be deleted from the given path.</returns>
        Public Overridable Function CanDeleteDirectory(directoryPath As String) As Boolean
            Return DirectoryExists(directoryPath)
        End Function

        ''' <summary>
        ''' Gets the directories in the given directory.
        ''' </summary>
        ''' <param name="path">Parent directory of the requested directories.</param>
        ''' <returns>A list of the full logical paths of the directories.</returns>
        Public Function GetDirectories(path As String, recursive As Boolean) As IEnumerable(Of String)
            Return GetItemsInternal(path, recursive, True).Select(Function(x) x.Key)
        End Function


#End Region

#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
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
