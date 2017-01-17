Imports SkyEditor.Core.UI

Namespace TestComponents
    Public Class TestMenuAction
        Inherits MenuAction

        Public Sub New()
            MyBase.New({"Test"})
            Me.AlwaysVisible = True
        End Sub

        Public Overrides Sub DoAction(Targets As IEnumerable(Of Object))
            Throw New NotImplementedException()
        End Sub

    End Class

End Namespace