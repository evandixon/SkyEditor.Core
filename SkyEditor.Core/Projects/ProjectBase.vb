Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace Projects
    ''' <summary>
    ''' Defines the common functionality of both projects and solutions.
    ''' </summary>
    Public MustInherit Class ProjectBase
        Implements INotifyModified
        Implements IDisposable
        Implements IReportProgress

        Public Sub New()
            Items = New Dictionary(Of String, Object)
        End Sub

#Region "Events"
        Public Event Modified(sender As Object, e As EventArgs) Implements INotifyModified.Modified
        Public Event BuildStatusChanged(sender As Object, e As ProgressReportedEventArgs) Implements IReportProgress.ProgressChanged

        ''' <summary>
        ''' Raised when a directory is created.
        ''' </summary>
        Public Event DirectoryCreated(sender As Object, e As DirectoryCreatedEventArgs)

        ''' <summary>
        ''' Raised when a directory is deleted.
        ''' </summary>
        ''' <remarks>Event will only be raised for directories that are directly requested to be deleted.  Events will not be raised for child directories or items.</remarks>
        Public Event DirectoryDeleted(sender As Object, e As DirectoryDeletedEventArgs)

        ''' <summary>
        ''' Raised when an item, such as a file or a project, has been added.
        ''' </summary>
        Public Event ItemAdded(sender As Object, e As ItemAddedEventArgs)

        ''' <summary>
        ''' Raised when an item, such as a file or a project, has been added.
        ''' </summary>
        Public Event ItemRemoved(sender As Object, e As ItemRemovedEventArgs)
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
        ''' ""/null - Represents the root directory
        ''' "Test"/null - directory
        ''' "Test/Ing"/null - directory
        ''' "Test/File"/[GenericFile] - File of type GenericFile, named "File", in directory "Test"</remarks>
        Private Property Items As Dictionary(Of String, Object)

        'While it would work to simply make Items protected, a function has the context that the result is calculated.
        'ProjectBase(Of T) will shadow this function to return a different object type (for low-level access during saving), so this context is beneficial.
        Protected Function GetItemDictionary() As Dictionary(Of String, Object)
            Return Items
        End Function

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
            Return path.Replace("\", "/").TrimEnd("/")
        End Function

        ''' <summary>
        ''' Gets paths and objects in the logical filesystem.
        ''' </summary>
        ''' <param name="path">Path of child items.</param>
        ''' <param name="recursive">Whether or not to search child directories.</param>
        ''' <param name="getDirectories">Whether to get files or directories.</param>
        ''' <returns>An instance of <see cref="IEnumerable(Of KeyValuePair(Of String,T))"/>, where each key is the full path and each value is the corresponding object, or null if the path is a directory.</returns>
        Private Function GetItemsInternal(path As String, recursive As Boolean, getDirectories As Boolean) As IEnumerable(Of KeyValuePair(Of String, Object))
            Dim fixedPath As String = FixPath(path).ToLowerInvariant

            'Given directory structure of:
            '/Test
            '/Test/Ing
            '/Blarg/Test
            '/Test/Ing/Test
            '
            'And an path of Test

            If recursive Then
                'Should return "Test/Ing" and "Test/Ing/Test
                Return Items.Where(Function(x) x.Key.ToLowerInvariant.StartsWith(fixedPath) AndAlso
                                        Not x.Key.ToLowerInvariant = fixedPath AndAlso 'Filters the same direcctory (/Test is not a child of /Test)
                                       ((getDirectories AndAlso x.Value Is Nothing) OrElse (Not getDirectories AndAlso x.Value IsNot Nothing))).
                            OrderBy(Function(x) x.Key, New DirectoryStructureComparer)
            Else
                'Should return "Test/Ing"
                Return Items.Where(Function(x)
                                       Dim relativePath As String
                                       If String.IsNullOrEmpty(fixedPath) Then
                                           relativePath = x.Key.ToLowerInvariant.Replace("\", "/").TrimStart("/")
                                       Else
                                           relativePath = x.Key.ToLowerInvariant.Replace(fixedPath, "").Replace("\", "/").TrimStart("/")
                                       End If

                                       Return x.Key.ToLowerInvariant.StartsWith(fixedPath) AndAlso
                                            Not x.Key.ToLowerInvariant = fixedPath AndAlso 'Filters the same direcctory (/Test is not a child of /Test)
                                            Not relativePath.Contains("/") AndAlso 'Filters anything with a slash after the parent directory                                            
                                            ((getDirectories AndAlso x.Value Is Nothing) OrElse (Not getDirectories AndAlso x.Value IsNot Nothing))
                                   End Function).
                             OrderBy(Function(x) x.Key, New DirectoryStructureComparer)
            End If

        End Function

        Public Function GetItems(path As String, recursive As Boolean) As Dictionary(Of String, Object)
            Return GetItemsInternal(path, recursive, False).ToDictionary(Function(x) x.Key, Function(y) y.Value)
        End Function

        Protected Function ItemExists(path As String) As Boolean
            Dim fixedPath = FixPath(path).ToLower
            Return Items.Any(Function(x) x.Key.ToLowerInvariant = fixedPath)
        End Function

        ''' <summary>
        ''' Gets the item at the given path.
        ''' </summary>
        ''' <param name="path">Path of the item.</param>
        ''' <returns>The item at the given path, or null if there is no item at the given path.</returns>
        Protected Function GetItem(path As String) As Object
            Dim fixedPathLower = FixPath(path).ToLowerInvariant
            Return Items.Where(Function(x) x.Key.ToLowerInvariant = fixedPathLower).FirstOrDefault.Value
        End Function

        Protected Sub AddItem(path As String, item As Object)
            If ItemExists(path) Then
                Throw New DuplicateItemException(path)
            Else
                Dim parentDirectory = FixPath(System.IO.Path.GetDirectoryName(path))
                If Not String.IsNullOrEmpty(parentDirectory) AndAlso Not DirectoryExists(parentDirectory) Then
                    CreateDirectory(parentDirectory)
                End If

                Dim fixedPath = FixPath(path)
                Items.Add(fixedPath, item)
                RaiseEvent ItemAdded(Me, New ItemAddedEventArgs With {.FullPath = fixedPath})
            End If
        End Sub

        ''' <summary>
        ''' Deletes a directory or item at the given path, if it exists.
        ''' </summary>
        ''' <param name="path">Path of the directory or item to delete.</param>
        ''' <returns>A boolean indicating whether or not the item was deleted.</returns>
        Protected Function DeleteItem(path As String) As Boolean
            Dim fixedPath = FixPath(path)
            Dim fixedPathLower = fixedPath.ToLowerInvariant
            Dim toRemove = Items.Where(Function(x) x.Key.ToLowerInvariant = fixedPathLower).Select(Function(x) x.Key).FirstOrDefault
            If toRemove IsNot Nothing Then
                Items.Remove(toRemove)
                RaiseEvent ItemRemoved(Me, New ItemRemovedEventArgs With {.FullPath = fixedPath})
                Return True
            Else
                Return False
            End If
        End Function

#Region "Directories"
        ''' <summary>
        ''' Determines whether or not a directory at the given path exists.
        ''' </summary>
        ''' <param name="path">Path of the directory to find.</param>
        ''' <returns>A boolean indicating whether or not a directory exists at the requested path.</returns>
        Public Function DirectoryExists(path As String) As Boolean
            Dim pathFixed = FixPath(path)
            Return String.IsNullOrEmpty(pathFixed) OrElse 'Root directory ("") should always exist
                Items.Any(Function(x) x.Key.ToLowerInvariant = pathFixed.ToLowerInvariant AndAlso x.Value Is Nothing) 'Check to see if directory exists
        End Function

        ''' <summary>
        ''' Gets the directories in the given directory.
        ''' </summary>
        ''' <param name="path">Parent directory of the requested directories.</param>
        ''' <returns>A list of the full logical paths of the directories.</returns>
        Public Function GetDirectories(path As String, recursive As Boolean) As IEnumerable(Of String)
            Return GetItemsInternal(path, recursive, True).Select(Function(x) x.Key)
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

            'Ensure parent directory exists
            If Not String.IsNullOrEmpty(path) Then 'But only if this isn't the root.
                CreateDirectory(FixPath(System.IO.Path.GetDirectoryName(path)))
            End If

            'Create directory
            If Not DirectoryExists(fixedPath) Then
                Items.Add(fixedPath, Nothing)
                RaiseEvent DirectoryCreated(Me, New DirectoryCreatedEventArgs(fixedPath))
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
        ''' Deletes the directory with the given path, along with any child items.
        ''' </summary>
        ''' <param name="path"></param>
        Public Sub DeleteDirectory(path As String)
            'Delete items
            For Each item In GetItemsInternal(path, True, False)
                DeleteItem(item.Key)
            Next

            'Delete child directories
            For Each item In GetDirectories(path, True)
                DeleteItem(item)
            Next

            'Delete directory
            If DeleteItem(path) Then
                RaiseEvent DirectoryDeleted(Me, New DirectoryDeletedEventArgs(path))
            End If
        End Sub

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

    ''' <summary>
    ''' Defines the common functionality of both projects and solutions.
    ''' </summary>
    ''' <typeparam name="T">Type of project items.</typeparam>
    Public MustInherit Class ProjectBase(Of T)
        Inherits ProjectBase

        Protected Shadows Function GetItems() As Dictionary(Of String, T)
            Return MyBase.GetItemDictionary.ToDictionary(Function(x) x.Key, Function(y) DirectCast(y.Value, T))
        End Function

        ''' <summary>
        ''' Gets the item at the given path.
        ''' </summary>
        ''' <param name="path">Path of the item.</param>
        ''' <returns>The item at the given path, or null if there is no item at the given path.</returns>
        Protected Shadows Function GetItem(path As String) As T
            'Todo: add more safety here.
            'Chances are the item will always be of type T, but it's possible that it won't be.
            Return MyBase.GetItem(path)
        End Function

        Protected Shadows Sub AddItem(path As String, item As T)
            MyBase.AddItem(path, item)
        End Sub
    End Class

End Namespace
