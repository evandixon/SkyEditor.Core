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
        Protected Sub RaisePropertyChanged(e As PropertyChangedEventArgs)
            RaiseEvent PropertyChanged(Me, e)
        End Sub
#End Region

#Region "Properties"
        Public Property File As Object
            Get
                Return _file
            End Get
            Set(value As Object)
                If TypeOf _file Is ISavable Then
                    RemoveHandler DirectCast(_file, ISavable).FileSaved, AddressOf File_OnSaved
                End If
                If TypeOf _file Is INotifyPropertyChanged Then
                    RemoveHandler DirectCast(_file, INotifyPropertyChanged).PropertyChanged, AddressOf File_OnModified
                End If
                If TypeOf _file Is INotifyModified Then
                    RemoveHandler DirectCast(_file, INotifyModified).Modified, AddressOf File_OnModified
                End If
                ResetViewModels()

                _file = value

                IsFileModified = False

                If TypeOf _file Is ISavable Then
                    AddHandler DirectCast(_file, ISavable).FileSaved, AddressOf File_OnSaved
                End If
                If TypeOf _file Is INotifyPropertyChanged Then
                    AddHandler DirectCast(_file, INotifyPropertyChanged).PropertyChanged, AddressOf File_OnModified
                End If
                If TypeOf _file Is INotifyModified Then
                    AddHandler DirectCast(_file, INotifyModified).Modified, AddressOf File_OnModified
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
                End If
            End Set
        End Property
        Dim _isFileModified As Boolean

        Public Overridable ReadOnly Property Title As String
            Get
                Dim out As String
                If TypeOf File Is INamed Then
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

        Public ReadOnly Property CloseCommand As ICommand

        Private Property ViewModels As List(Of GenericViewModel)
#End Region

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
                    If TypeOf vm Is INotifyPropertyChanged Then
                        AddHandler DirectCast(vm, INotifyPropertyChanged).PropertyChanged, AddressOf File_OnModified
                    End If
                    If TypeOf vm Is INotifyModified Then
                        AddHandler DirectCast(vm, INotifyModified).Modified, AddressOf File_OnModified
                    End If
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
                    If TypeOf item Is INotifyPropertyChanged Then
                        RemoveHandler DirectCast(item, INotifyPropertyChanged).PropertyChanged, AddressOf File_OnModified
                    End If
                    If TypeOf item Is INotifyModified Then
                        RemoveHandler DirectCast(item, INotifyModified).Modified, AddressOf File_OnModified
                    End If
                Next
                ViewModels.Clear()
                ViewModels = Nothing
            End If
        End Sub

        Protected Overridable Function OnClosed() As Task
            RaiseEvent CloseCommandExecuted(Me, New EventArgs)
            Return Task.FromResult(0)
        End Function

        Private Sub File_OnSaved(sender As Object, e As EventArgs)
            IsFileModified = False
        End Sub

        Private Sub File_OnModified(sender As Object, e As EventArgs)
            IsFileModified = True
        End Sub
    End Class
End Namespace
