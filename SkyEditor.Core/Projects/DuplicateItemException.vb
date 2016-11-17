Namespace Projects
    Public Class DuplicateItemException
        Inherits ProjectException

        Public Sub New(path As String)
            MyBase.New(String.Format(My.Resources.Language.DuplicateItemException, path))
        End Sub
    End Class
End Namespace