Imports System.Reflection
Imports System.Text
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.UI
Imports SkyEditor.Core.Utilities
Imports SkyEditor.Core.Settings
Imports System.Windows.Input
Imports System.Collections.Specialized
''' <summary>
''' Class that manages open files, solutions, and projects, and helps with the UI display them.
''' </summary>
Public Class IOUIManager
    Implements IDisposable
    Implements INotifyPropertyChanged

    Public Sub New(manager As PluginManager)
        Me.CurrentPluginManager = manager
        Me.CurrentSolution = Nothing
        Me.OpenedProjectFiles = New Dictionary(Of Object, Project)
        Me.FileDisposalSettings = New Dictionary(Of Object, Boolean)
        Me.OpenFiles = New ObservableCollection(Of FileViewModel)
        Me.RunningTasks = New ObservableCollection(Of Task)
        Me.AnchorableViewModels = New ObservableCollection(Of AnchorableViewModel)
    End Sub

#Region "Events"
    Public Event SolutionChanged(sender As Object, e As EventArgs)
    Public Event CurrentProjectChanged(sender As Object, e As EventArgs)
    Public Event FileOpened(sender As Object, e As FileOpenedEventArguments)
    Public Event FileClosing(sender As Object, e As FileClosingEventArgs)
    Public Event FileClosed(sender As Object, e As FileClosedEventArgs)
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
#End Region

#Region "Event Handlers"

    Private Sub IOUIManager_FileOpened(sender As Object, e As FileOpenedEventArguments) Handles Me.FileOpened
        'Make sure there's an open file
        If SelectedFile Is Nothing AndAlso OpenFiles.Count > 0 Then
            SelectedFile = OpenFiles.First
        End If
    End Sub

    Private Sub IOUIManager_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles Me.PropertyChanged
        For Each item In RootMenuItems
            UpdateMenuItemVisibility(item)
        Next
    End Sub

    Private Sub _selectedFile_MenuItemRefreshRequested(sender As Object, e As EventArgs) Handles _selectedFile.MenuItemRefreshRequested
        For Each item In RootMenuItems
            UpdateMenuItemVisibility(item)
        Next
    End Sub

    Private Sub _openFiles_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs) Handles _openFiles.CollectionChanged
        If e.NewItems IsNot Nothing Then
            For Each item As FileViewModel In e.NewItems
                AddHandler item.CloseCommandExecuted, AddressOf File_OnClosed
            Next
        End If

        If e.OldItems IsNot Nothing Then
            For Each item As FileViewModel In e.OldItems
                RemoveHandler item.CloseCommandExecuted, AddressOf File_OnClosed
            Next
        End If
    End Sub

    Private Sub File_OnClosed(sender As Object, e As EventArgs)
        Dim args As New FileClosingEventArgs
        args.File = sender

        RaiseEvent FileClosing(Me, args)

        If Not args.Cancel Then
            'Doing the directcast again in case something changed args
            CloseFile(DirectCast(sender, FileViewModel))
        End If
    End Sub
#End Region

#Region "Properties"

    ''' <summary>
    ''' Instance of the current plugin manager.
    ''' </summary>
    ''' <returns>The instance of the current plugin manager.</returns>
    Public Property CurrentPluginManager As PluginManager

    ''' <summary>
    ''' The files that are currently open
    ''' </summary>
    ''' <returns></returns>
    Public Property OpenFiles As ObservableCollection(Of FileViewModel)
        Get
            Return _openFiles
        End Get
        Private Set(value As ObservableCollection(Of FileViewModel))
            _openFiles = value
        End Set
    End Property
    Private WithEvents _openFiles As ObservableCollection(Of FileViewModel)

    ''' <summary>
    ''' The view models for anchorable views.
    ''' </summary>
    ''' <returns>The view models for anchorable views.</returns>
    Public Property AnchorableViewModels As ObservableCollection(Of AnchorableViewModel)
        Get
            Return _anchorableViewModels
        End Get
        Private Set(value As ObservableCollection(Of AnchorableViewModel))
            _anchorableViewModels = value
        End Set
    End Property
    Dim _anchorableViewModels As ObservableCollection(Of AnchorableViewModel)

    ''' <summary>
    ''' Gets or sets the selected file
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectedFile As FileViewModel
        Get
            Return _selectedFile
        End Get
        Set(value As FileViewModel)
            If _selectedFile IsNot value Then
                _selectedFile = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(SelectedFile)))
            End If
        End Set
    End Property
    Private WithEvents _selectedFile As FileViewModel

    ''' <summary>
    ''' Gets or sets the currently selected view model (Anchorable or File)
    ''' </summary>
    ''' <returns>The currently selected view model (Anchorable or File)</returns>
    Public Property ActiveContent As Object
        Get
            Return _activeContent
        End Get
        Set(value As Object)
            'Only update if we changed something
            If _activeContent IsNot value Then
                _activeContent = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(ActiveContent)))
            End If

            'If the active content is a file, update the active file
            If TypeOf value Is FileViewModel Then
                SelectedFile = value
            End If
        End Set
    End Property
    Dim _activeContent As Object

    ''' <summary>
    ''' Stores whether or not to dispose of files on close
    ''' </summary>
    ''' <returns></returns>
    Private Property FileDisposalSettings As Dictionary(Of Object, Boolean)

    ''' <summary>
    ''' Matches opened files to their parent projects
    ''' </summary>
    ''' <returns></returns>
    Private Property OpenedProjectFiles As Dictionary(Of Object, Project)

    ''' <summary>
    ''' Dictionary of (Extension, Friendly Name) used in the Open and Save file dialogs.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IOFilters As New Dictionary(Of String, String)

    Public Property CurrentSolution As Solution
        Get
            Return _currentSolution
        End Get
        Set(value As Solution)
            If _currentSolution IsNot value Then
                'If we're actually changing values...

                'Dispose of the old one
                If _currentSolution IsNot Nothing Then
                    _currentSolution.Dispose()
                End If

                'Update the current
                _currentSolution = value

                'And report that we changed something
                RaiseEvent SolutionChanged(Me, New EventArgs)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(CurrentSolution)))
            End If
        End Set
    End Property
    Private WithEvents _currentSolution As Solution

    Public Property CurrentProject As Project
        Get
            Return _currentProject
        End Get
        Set(value As Project)
            If _currentProject IsNot value Then
                'If we're actually changing values...

                'Update the current
                _currentProject = value

                'And report that we changed something
                RaiseEvent CurrentProjectChanged(Me, New EventArgs)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(CurrentProject)))
            End If
        End Set
    End Property
    Private WithEvents _currentProject As Project

    Public Property RootMenuItems As ObservableCollection(Of ActionMenuItem)
        Get
            If _rootMenuItems Is Nothing Then
                _rootMenuItems = New ObservableCollection(Of ActionMenuItem)
                'Generate the menu items
                For Each item In UIHelper.GenerateLogicalMenuItems(UIHelper.GetMenuItemInfo(CurrentPluginManager, CurrentPluginManager.CurrentSettingsProvider.GetIsDevMode), Me, Nothing)
                    _rootMenuItems.Add(item)
                Next
                'Update their visibility now that all of them have been created
                'Doing this before they're all created will cause unintended behavior
                For Each item In _rootMenuItems
                    UpdateMenuItemVisibility(item)
                Next
            End If
            Return _rootMenuItems
        End Get
        Protected Set(value As ObservableCollection(Of ActionMenuItem))
            _rootMenuItems = value
        End Set
    End Property
    Dim _rootMenuItems As ObservableCollection(Of ActionMenuItem)

    Public Property RunningTasks As ObservableCollection(Of Task)

#End Region

#Region "Functions"

#Region "IO Filters"

    ''' <summary>
    ''' Gets the IO filter string for use with an OpenFileDialog or a SaveFileDialog.
    ''' </summary>
    ''' <param name="filters">A collection containing the extensions to put in the string.</param>
    ''' <param name="addSupportedFilesEntry">Whether or not to add a "Supported Files" entry to the filter.</param>
    ''' <param name="allowAllFiles">Whether or not to add an "All Files" entry to the filters.</param>
    ''' <returns>A string that can be used directly with the filter of an OpenFileDialog or a SaveFileDialog.</returns>
    Public Function GetIOFilter(filters As ICollection(Of String), addSupportedFilesEntry As Boolean, allowAllFiles As Boolean) As String
        Dim fullFilter As New StringBuilder
        Dim usableFilters = (From i In IOFilters Where filters.Contains(i.Key)).ToDictionary(Function(x) x.Key, Function(y) y.Value)

        If addSupportedFilesEntry Then
            fullFilter.Append(My.Resources.Language.SupportedFiles & " (" &
                                    String.Join(", ", From i In usableFilters Select i.Value) & ")|" &
                                    String.Join(";", From i In usableFilters Select "*." & i.Key.Trim("*").Trim(".")) & "|")
        End If

        fullFilter.Append(String.Join("|", From i In usableFilters Select String.Format("{0} ({1})|{1}", i.Value, "*." & i.Key.Trim("*").Trim("."))))

        If allowAllFiles Then
            fullFilter.Append("|" & My.Resources.Language.AllFiles & " (*.*)|*.*")
        End If

        Return fullFilter.ToString
    End Function

    ''' <summary>
    ''' Gets the IO filter string for use with an OpenFileDialog or a SaveFileDialog.
    ''' </summary>
    ''' <returns>A string that can be used directly with the filter of an OpenFileDialog or a SaveFileDialog.</returns>
    Public Function GetIOFilter() As String
        Return GetIOFilter(IOFilters.Keys, True, True)
    End Function

    ''' <summary>
    ''' Gets the IO filter string for use with an OpenFileDialog or a SaveFileDialog.
    ''' </summary>
    ''' <param name="filters">A collection containing the extensions to put in the string.</param>
    ''' <returns>A string that can be used directly with the filter of an OpenFileDialog or a SaveFileDialog.</returns>
    Public Function GetIOFilter(filters As ICollection(Of String)) As String
        Return GetIOFilter(filters, True, True)
    End Function

    ''' <summary>
    ''' Registers a filter for use in open and save file dialogs.
    ''' </summary>
    ''' <param name="FileExtension">Filter for the dialog.  If this is by extension, should be *.extension</param>
    ''' <param name="FileFormatName">Name of the file format</param>
    Public Sub RegisterIOFilter(FileExtension As String, FileFormatName As String)
        Dim TempIOFilters As Dictionary(Of String, String) = IOFilters
        If TempIOFilters Is Nothing Then
            TempIOFilters = New Dictionary(Of String, String)
        End If
        If Not TempIOFilters.ContainsKey(FileExtension) Then
            TempIOFilters.Add(FileExtension, FileFormatName)
        End If
        IOFilters = TempIOFilters
    End Sub

    ''' <summary>
    ''' Gets the filter for a Windows Forms Open or Save File Dialog for use with Sky Editor Projects.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetProjectIOFilter() As String
        Return $"{My.Resources.Language.SkyEditorProjects} (*.skyproj)|*.skyproj|{My.Resources.Language.AllFiles} (*.*)|*.*"
    End Function
#End Region

#Region "Open/Close File"

    ''' <summary>
    ''' Opens the given file
    ''' </summary>
    ''' <param name="file">File to open</param>
    ''' <param name="DisposeOnClose">True to call the file's dispose method (if IDisposable) when closed.</param>
    ''' <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is null.</exception>
    Public Sub OpenFile(file As Object, DisposeOnClose As Boolean)
        If file Is Nothing Then
            Throw New ArgumentNullException(NameOf(file))
        End If

        If Not (From o In OpenFiles Where o.File Is file).Any Then
            Dim wrapper = CreateViewModel(file)
            OpenFiles.Add(wrapper)
            FileDisposalSettings.Add(file, DisposeOnClose)
            RaiseEvent FileOpened(Nothing, New FileOpenedEventArguments With {.File = file, .DisposeOnExit = DisposeOnClose})
        End If
    End Sub

    ''' <summary>
    ''' Opens the givenfile
    ''' </summary>
    ''' <param name="file">File to open</param>
    ''' <param name="parentProject">Project the file belongs to.  If the file does not belong to a project, don't use this overload.</param>
    ''' <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> or <paramref name="parentProject"/> is null.</exception>
    Public Sub OpenFile(file As Object, parentProject As Project)
        If file Is Nothing Then
            Throw New ArgumentNullException(NameOf(file))
        End If
        If parentProject Is Nothing Then
            Throw New ArgumentNullException(NameOf(parentProject))
        End If

        If Not (From o In OpenFiles Where o.File Is file).Any Then
            Dim wrapper = CreateViewModel(file)
            OpenFiles.Add(wrapper)
            OpenedProjectFiles.Add(file, parentProject)
            RaiseEvent FileOpened(Nothing, New FileOpenedEventArguments With {.File = file, .DisposeOnExit = False, .ParentProject = parentProject})
        End If
    End Sub

    ''' <summary>
    ''' Opens a file from the given filename.
    ''' </summary>
    ''' <param name="filename">Full path of the file to open.</param>
    ''' <param name="autoDetectSelector">Delegate function used to resolve duplicate auto-detection results.</param>
    ''' <remarks>This overload is intended to open files on disk that are not associated with a project, automatically determining the file type.
    ''' To open a project file, use <see cref="OpenFile(Object, Project)"/>.
    ''' To open a file that is not necessarily on disk, use <see cref="OpenFile(Object, Boolean)"/>.
    ''' To open a file using a specific type as the model, use <see cref="OpenFile(String, TypeInfo)"/>.
    ''' 
    ''' When the file is closed, the underlying model will be disposed.</remarks>
    Public Async Function OpenFile(filename As String, autoDetectSelector As IOHelper.DuplicateMatchSelector) As Task
        Dim model = Await IOHelper.OpenObject(filename, autoDetectSelector, CurrentPluginManager)

        If Not (From o In OpenFiles Where o.File Is model).Any Then
            Dim wrapper = CreateViewModel(model)
            wrapper.Filename = filename
            OpenFiles.Add(wrapper)
            FileDisposalSettings.Add(model, True)
            RaiseEvent FileOpened(Nothing, New FileOpenedEventArguments With {.File = model, .DisposeOnExit = True})
        End If
    End Function

    ''' <summary>
    ''' Opens a file from the given filename.
    ''' </summary>
    ''' <param name="filename">Full path of the file to open.</param>
    ''' <param name="modelType">Type of the model of the file.</param>
    ''' <remarks>This overload is intended to open files on disk, using a specific file type, that are not associated with a project.
    ''' To open a project file, use <see cref="OpenFile(Object, Project)"/>.
    ''' To open a file that is not necessarily on disk, use <see cref="OpenFile(Object, Boolean)"/>.
    ''' To open a file, auto-detecting the file type, use <see cref="OpenFile(String, IOHelper.DuplicateMatchSelector)"/>.
    ''' 
    ''' When the file is closed, the underlying model will be disposed.</remarks>
    Public Async Function OpenFile(filename As String, modelType As TypeInfo) As Task
        Dim model = Await IOHelper.OpenFile(filename, modelType, CurrentPluginManager)

        If Not (From o In OpenFiles Where o.File Is model).Any Then
            Dim wrapper = CreateViewModel(model)
            wrapper.Filename = filename
            OpenFiles.Add(wrapper)
            FileDisposalSettings.Add(model, True)
            RaiseEvent FileOpened(Nothing, New FileOpenedEventArguments With {.File = model, .DisposeOnExit = True})
        End If
    End Function

    ''' <summary>
    ''' Closes the file
    ''' </summary>
    ''' <param name="File">File to close</param>
    Public Sub CloseFile(file As FileViewModel)
        If file IsNot Nothing Then
            For count = OpenFiles.Count - 1 To 0 Step -1
                If OpenFiles(count) Is file Then
                    OpenFiles.RemoveAt(count)
                End If
            Next

            If file Is SelectedFile Then
                SelectedFile = Nothing
            End If

            Dim didDispose As Boolean = False
            If FileDisposalSettings.ContainsKey(file.File) Then
                If FileDisposalSettings(file.File) Then
                    If TypeOf file.File Is IDisposable Then
                        DirectCast(file.File, IDisposable).Dispose()
                        didDispose = True
                    End If
                End If
                FileDisposalSettings.Remove(file.File)
            End If
            RaiseEvent FileClosed(Me, New FileClosedEventArgs With {.File = file.File, .Disposed = didDispose})
        End If
    End Sub
#End Region

    ''' <summary>
    ''' Gets the current view models for the model, creating them if necessary.
    ''' </summary>
    ''' <param name="model">Model for which to get the view models.</param>
    ''' <returns>An IEnumerable of view models that support the given model, or null if the model is not an open file.</returns>
    Public Function GetViewModelsForModel(model As Object) As IEnumerable(Of GenericViewModel)
        Dim file = (From f In OpenFiles Where f.File Is model).FirstOrDefault
        Return file?.GetViewModels(CurrentPluginManager)
    End Function

    ''' <summary>
    ''' Returns the file's parent project, if it exists.
    ''' </summary>
    ''' <param name="File">File of which to get the parent project.  Must be an open file, otherwise the function will return Nothing.</param>
    ''' <returns></returns>
    Public Function GetOpenedFileProject(File As Object) As Project
        If Me.OpenedProjectFiles.ContainsKey(File) Then
            Return Me.OpenedProjectFiles(File)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Gets the possible targets for a menu action.
    ''' </summary>
    ''' <returns></returns>
    Private Function GetMenuActionTargets() As IEnumerable(Of Object)
        Dim out As New List(Of Object)

        If CurrentSolution IsNot Nothing Then
            out.Add(CurrentSolution)
        End If

        If CurrentProject IsNot Nothing Then
            out.Add(CurrentProject)
        End If

        If SelectedFile IsNot Nothing Then
            out.Add(SelectedFile)
            out.Add(SelectedFile.File)

            If TypeOf SelectedFile.File Is GenericViewModel AndAlso DirectCast(SelectedFile.File, GenericViewModel).Model IsNot Nothing Then
                out.Add(DirectCast(SelectedFile.File, GenericViewModel).Model)
            End If
        End If

        Return out
    End Function

    ''' <summary>
    ''' Gets the targets for the given menu action
    ''' </summary>
    ''' <param name="action">The action for which to retrieve the targets</param>
    ''' <returns></returns>
    Public Function GetMenuActionTargets(action As MenuAction) As IEnumerable(Of Object)
        Dim targets As New List(Of Object)

        'Add the current project to the targets if supported
        If CurrentSolution IsNot Nothing AndAlso action.SupportsObject(CurrentSolution) Then
            targets.Add(CurrentSolution)
        End If

        'Add the current project if supported
        If CurrentProject IsNot Nothing AndAlso action.SupportsObject(CurrentProject) Then
            targets.Add(CurrentProject)
        End If

        'Add the selected file if supported
        If SelectedFile IsNot Nothing Then
            'Add the file's view model if supported
            If action.SupportsObject(SelectedFile) Then
                targets.Add(SelectedFile)
            End If

            'Add the model if supported
            If action.SupportsObject(SelectedFile.File) Then
                targets.Add(SelectedFile.File)
            End If

            'Add a view model for the current file if available
            For Each item In From vm In SelectedFile.GetViewModels(CurrentPluginManager) Where action.SupportsObject(vm)
                targets.Add(item)
            Next
        End If

        Return targets
    End Function

    ''' <summary>
    ''' Lets the IOUIManager keep track of the current task
    ''' </summary>
    ''' <param name="task">Task to keep track of.</param>
    Public Sub LogTask(task As Task)
        Dim watchTask = Task.Run(Async Function() As Task
                                     Await task
                                     RemoveTask(task)
                                 End Function)
    End Sub

    Private Sub RemoveTask(task As Task)
        RunningTasks.Remove(task)
    End Sub

    ''' <summary>
    ''' Updates the visibility for the given menu item and its children, and returns the updated visibility
    ''' </summary>
    ''' <param name="menuItem"></param>
    ''' <returns></returns>
    Private Function UpdateMenuItemVisibility(menuItem As ActionMenuItem) As Boolean
        Dim possibleTargets = GetMenuActionTargets() 'Note: Excludes view models for the selected file

        'Default to not visible
        Dim isVisible = False

        If menuItem.Actions IsNot Nothing Then
            'Visibility is determined by every available action
            'If any one of those actions is applicable, then this menu item is visible
            For Each item In menuItem.Actions
                If Not isVisible Then
                    If item.AlwaysVisible Then
                        'Then this action is always visible
                        isVisible = True

                        'And don't bother checking the rest
                        Exit For
                    Else
                        For Each target In possibleTargets
                            'Check to see if this target is supported
                            If item.SupportsObject(target) Then
                                'If it is, then this menu item should be visible
                                isVisible = True

                                'And don't bother checking the rest
                                Exit For
                            End If
                        Next

                        If Not isVisible AndAlso SelectedFile?.File IsNot Nothing Then
                            'Check to see if the action supports any view models
                            'If there are any view models that support the selected file, 
                            isVisible = (From vm In SelectedFile.GetViewModels(CurrentPluginManager) Where item.SupportsObject(vm)).Any
                        End If
                    End If
                Else
                    'Then this menu item is visible, and don't bother checking the rest
                    Exit For
                End If
            Next
        End If

        'Update children
        For Each item In menuItem.Children
            If UpdateMenuItemVisibility(item) Then
                isVisible = True
            End If
        Next

        'Set the visibility to the value we calculated
        menuItem.IsVisible = isVisible

        'Set this item to visible if there's a visible
        Return isVisible
    End Function

    Public Sub ShowAnchorable(model As AnchorableViewModel)
        Dim targetType = model.GetType
        If Not (From m In AnchorableViewModels Where ReflectionHelpers.IsOfType(m, targetType.GetTypeInfo, False)).Any Then
            model.CurrentIOUIManager = Me
            AnchorableViewModels.Add(model)
        End If
    End Sub

    ''' <summary>
    ''' Creates a new FileViewModel wrapper for the given model.
    ''' </summary>
    ''' <param name="model">Model for which to create the FileViewModel wrapper.</param>
    ''' <returns>A new FileViewModel wrapper.</returns>
    Protected Overridable Function CreateViewModel(model As Object) As FileViewModel
        Dim out As New FileViewModel
        out.File = model
        Return out
    End Function

#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If CurrentSolution IsNot Nothing Then
                    CurrentSolution.Dispose()
                End If
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
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
