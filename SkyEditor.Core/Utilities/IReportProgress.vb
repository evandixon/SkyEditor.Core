Namespace Utilities
    Public Interface IReportProgress
        Event ProgressChanged(sender As Object, e As ProgressReportedEventArgs)
        ReadOnly Property Progress As Single
        ReadOnly Property Message As String
        ReadOnly Property IsIndeterminate As Boolean
    End Interface
End Namespace

