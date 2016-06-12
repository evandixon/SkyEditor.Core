Imports System.Windows.Input

Namespace UI
    Public Class RelayCommand
        Implements ICommand

        Private ReadOnly _Execute As Action(Of Object)

        Public Sub New(execute As Action(Of Object))
            If execute Is Nothing Then
                Throw New ArgumentNullException(NameOf(execute))
            End If
            _Execute = execute
            _isEnabled = True
        End Sub

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Property IsEnabled() As Boolean
            Get
                Return _isEnabled
            End Get
            Set(ByVal value As Boolean)
                If (value <> _isEnabled) Then
                    _isEnabled = value
                    RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
                End If
            End Set
        End Property
        Dim _isEnabled As Boolean

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return IsEnabled
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Execute.Invoke(parameter)
        End Sub

    End Class
End Namespace
