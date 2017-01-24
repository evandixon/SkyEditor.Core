Imports SkyEditor.Core.UI

Namespace Projects
    Public Class FileClosingEventArgs
        Public Property Cancel As Boolean
        Public Property File As FileViewModel
        Public Sub New()
            Me.Cancel = False
        End Sub
    End Class
End Namespace