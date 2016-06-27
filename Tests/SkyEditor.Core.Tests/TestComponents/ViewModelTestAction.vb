Imports System.Reflection
Imports SkyEditor.Core.UI

Namespace TestComponents
    ''' <summary>
    ''' A menu action that targets a view model
    ''' </summary>
    Public Class ViewModelTestMenuAction
        Inherits MenuAction
        Public Sub New()
            MyBase.New({"Model"})
        End Sub

        Public Overrides Function SupportedTypes() As IEnumerable(Of TypeInfo)
            Return {GetType(TestViewModel).GetTypeInfo}
        End Function

        Public Overrides Sub DoAction(Targets As IEnumerable(Of Object))
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace