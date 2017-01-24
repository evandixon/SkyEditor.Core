Namespace Projects
    Public Class DirectoryCreatedEventArgs
        Inherits EventArgs

        Public Sub New(path As String)
            Me.FullPath = path
        End Sub

        Public Property FullPath As String
    End Class
End Namespace