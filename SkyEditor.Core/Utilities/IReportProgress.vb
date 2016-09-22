Namespace Utilities
    Public Interface IReportProgress
        Event ProgressChanged(sender As Object, e As ProgressReportedEventArgs)
        Event Completed(sender As Object, e As EventArgs)
        ReadOnly Property Progress As Single
        ReadOnly Property Message As String
        ReadOnly Property IsIndeterminate As Boolean
        ReadOnly Property IsCompleted As Boolean
    End Interface
End Namespace

