Namespace Projects
    Public Class ProjectAddedEventArgs
        Inherits EventArgs

        ''' <summary>
        ''' Path of the project in the solution
        ''' </summary>
        ''' <returns></returns>
        Public Property Path As String

        ''' <summary>
        ''' The project that was added
        ''' </summary>
        ''' <returns></returns>
        Public Property Project As Project
    End Class

End Namespace