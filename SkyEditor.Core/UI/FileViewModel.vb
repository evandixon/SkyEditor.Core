Imports System.Reflection
Imports System.Windows.Input
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace UI
    ''' <summary>
    ''' The view model for an open file
    ''' </summary>
    Public Class FileViewModel
        Implements INotifyPropertyChanged
        Implements INotifyModified

        Public Sub New()
            IsFileModified = False
            CloseCommand = New RelayCommand(AddressOf OnClosed)
        End Sub
        Public Sub New(file As Object)
            Me.New
            Me.File = file
        End Sub

#Region "Events"
        Public Event CloseCommandExecuted(sender As Object, e As EventArgs)
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Event Modified(sender As Object, e As EventArgs) Implements INotifyModified.Modified
        Public Event MenuItemRefreshRequested(sender As Object, e As EventArgs)

        Protected Sub RaisePropertyChanged(e As PropertyChangedEventArgs)
            RaiseEvent PropertyChanged(Me, e)
        End Sub

        Private Sub OnMenuItemRefreshRequested(sender As Object, e As EventArgs)
            RaiseEvent MenuItemRefreshRequested(sender, e)
        End Sub
#End Region

#Region "Event Handlers"
        Private Sub File_OnSaved(sender As Object, e As EventArgs)
            IsFileModified = False
        End Sub

        Private Sub File_OnModified(sender As Object, e As EventArgs)
            IsFileModified = True
        End Sub
#End Region

#Region "Properties"
        Public Property File As Object
            Get
                Return _file
            End Get
            Set(value As Object)
                If value IsNot _file Then
                    If TypeOf _file Is ISavable Then
                        RemoveHandler DirectCast(_file, ISavable).FileSaved, AddressOf File_OnSaved
                    End If
                    If TypeOf _file Is INotifyModified Then
                        RemoveHandler DirectCast(_file, INotifyModified).Modified, AddressOf File_OnModified
                    End If
                    ResetViewModels()

                    Dim originallyNull = (_file Is Nothing)

                    _file = value

                    If originallyNull Then
                        'If we're loading the file, it hasn't been modified
                        IsFileModified = False
                    Else
                        'If the file is being replaced, it has been modified
                        IsFileModified = True
                    End If

                    If TypeOf _file Is ISavable Then
                        AddHandler DirectCast(_file, ISavable).FileSaved, AddressOf File_OnSaved
                    End If
                    If TypeOf _file Is INotifyModified Then
                        AddHandler DirectCast(_file, INotifyModified).Modified, AddressOf File_OnModified
                    End If

                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(File)))
                End If
            End Set
        End Property
        Dim _file As Object

        Public Property IsFileModified As Boolean
            Get
                Return _isFileModified
            End Get
            Set(value As Boolean)
                If Not _isFileModified = value Then
                    _isFileModified = value
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(IsFileModified)))

                    'Title is dependant on this property, so notify that it changed too
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Title)))

                    RaiseEvent Modified(Me, New EventArgs)
                End If
            End Set
        End Property
        Dim _isFileModified As Boolean

        Public Overridable ReadOnly Property Title As String
            Get
                Dim out As String
                If Not String.IsNullOrEmpty(Filename) Then
                    out = Path.GetFileName(Filename)
                ElseIf TypeOf File Is INamed Then
                    out = DirectCast(File, INamed).Name
                Else
                    out = ReflectionHelpers.GetTypeFriendlyName(File.GetType)
                End If
                If IsFileModified Then
                    Return "* " & out
                Else
                    Return out
                End If
            End Get
        End Property

        Public Property Filename As String

        Public ReadOnly Property CloseCommand As ICommand

        Private Property ViewModels As List(Of GenericViewModel)
#End Region

        ''' <summary>
        ''' Tells the view models to update the underlying model
        ''' </summary>
        Private Sub ForceViewModelRefresh()
            For Each item In ViewModels
                item.UpdateModel(File)
            Next
        End Sub

        ''' <summary>
        ''' Saves the current file.
        ''' </summary>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        Public Sub Save(manager As PluginManager)
            ForceViewModelRefresh()
            Dim saver = (From s In manager.GetRegisteredObjects(Of IFileSaver) Where s.SupportsSave(File)).FirstOrDefault
            If saver Is Nothing Then
                'If we can't find a saver that supports saving without a filename, use the explicit overload.
                Save(Filename, manager)
            Else
                'We have a saver that works without a filename
                saver.Save(File, manager.CurrentIOProvider)
            End If
            IsFileModified = False
        End Sub

        ''' <summary>
        ''' Saves the file to the given filename.
        ''' </summary>
        ''' <param name="filename">Full path of the destination file.</param>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        Public Sub Save(filename As String, manager As PluginManager)
            ForceViewModelRefresh()
            Dim saver = (From s In manager.GetRegisteredObjects(Of IFileSaver) Where s.SupportsSaveAs(File)).First
            saver.Save(File, filename, manager.CurrentIOProvider)
            IsFileModified = False
        End Sub

        ''' <summary>
        ''' Determines whether <see cref="Save(PluginManager)"/> can be called.
        ''' </summary>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        ''' <returns>A boolean indicating if <see cref="Save(PluginManager)"/> can be called.</returns>
        Public Function CanSave(manager As PluginManager) As Boolean
            Return (From s In manager.GetRegisteredObjects(Of IFileSaver) Where s.SupportsSave(File) OrElse (Me.Filename IsNot Nothing AndAlso s.SupportsSaveAs(File))).Any
        End Function

        ''' <summary>
        ''' Determines whether <see cref="Save(String, PluginManager)"/> can be called.
        ''' </summary>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        ''' <returns>A boolean indicating if <see cref="Save(String, PluginManager)"/> can be called.</returns>
        Public Function CanSaveAs(manager As PluginManager) As Boolean
            Return (From s In manager.GetRegisteredObjects(Of IFileSaver) Where s.SupportsSaveAs(File)).Any
        End Function

        ''' <summary>
        ''' Gets the default extension for the file when using <see cref="Save(String, PluginManager)"/>.
        ''' </summary>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        ''' <returns>The default extension for the file when using <see cref="Save(String, PluginManager)"/>, or null if either the file does not support Save As, or there is no default extension.</returns>
        Public Function GetDefaultExtension(manager As PluginManager) As String
            Dim saver = (From s In manager.GetRegisteredObjects(Of IFileSaver) Where s.SupportsSaveAs(File)).FirstOrDefault
            If saver Is Nothing Then
                Return Nothing
            Else
                Return saver.GetDefaultExtension(File)
            End If
        End Function

        ''' <summary>
        ''' Gets the supported extensions for the file when using <see cref="Save(String, PluginManager)"/>.
        ''' </summary>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        ''' <returns>An IEnumerable of extensions that can be used for the file when using <see cref="Save(String, PluginManager)"/>, or null if either the file does not support Save As, or there are no supported extensions.</returns>
        Public Function GetSupportedExtensions(manager As PluginManager) As IEnumerable(Of String)
            Dim saver = (From s In manager.GetRegisteredObjects(Of IFileSaver) Where s.SupportsSaveAs(File)).FirstOrDefault
            If saver Is Nothing Then
                Return Nothing
            Else
                Return saver.GetSupportedExtensions(File)
            End If
        End Function

        ''' <summary>
        ''' Gets the current view models for the given file, creating them if necessary.
        ''' </summary>
        ''' <returns>An IEnumerable of view models that support the given file's model.</returns>
        Public Function GetViewModels(manager As PluginManager) As IEnumerable(Of GenericViewModel)
            If ViewModels Is Nothing Then
                ViewModels = New List(Of GenericViewModel)
                'do something
                For Each viewModel In From vm In manager.GetRegisteredObjects(Of GenericViewModel) Where vm.SupportsObject(File)
                    Dim vm As GenericViewModel = ReflectionHelpers.CreateNewInstance(viewModel)
                    vm.SetPluginManager(manager)
                    vm.SetModel(File)
                    ViewModels.Add(vm)

                    If TypeOf vm Is ISavable Then
                        AddHandler DirectCast(vm, ISavable).FileSaved, AddressOf File_OnSaved
                    End If
                    If TypeOf vm Is INotifyModified Then
                        AddHandler DirectCast(vm, INotifyModified).Modified, AddressOf File_OnModified
                    End If
                    AddHandler vm.MenuItemRefreshRequested, AddressOf OnMenuItemRefreshRequested
                Next
            End If
            Return ViewModels
        End Function

        ''' <summary>
        ''' Clears the current view models and removes event handlers
        ''' </summary>
        Private Sub ResetViewModels()
            If ViewModels IsNot Nothing Then
                'Remove handlers first
                For Each item In ViewModels
                    If TypeOf item Is ISavable Then
                        RemoveHandler DirectCast(item, ISavable).FileSaved, AddressOf File_OnSaved
                    End If
                    If TypeOf item Is INotifyModified Then
                        RemoveHandler DirectCast(item, INotifyModified).Modified, AddressOf File_OnModified
                    End If
                    RemoveHandler item.MenuItemRefreshRequested, AddressOf OnMenuItemRefreshRequested
                Next
                ViewModels.Clear()
                ViewModels = Nothing
            End If
        End Sub

        Protected Overridable Function OnClosed() As Task
            RaiseEvent CloseCommandExecuted(Me, New EventArgs)
            Return Task.FromResult(0)
        End Function

    End Class
End Namespace
