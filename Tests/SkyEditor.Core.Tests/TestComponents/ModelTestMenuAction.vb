Imports System.Reflection
Imports SkyEditor.Core.UI

Namespace TestComponents
    ''' <summary>
    ''' A menu action that targets a model
    ''' </summary>
    Public Class ModelTestMenuAction
        Inherits MenuAction
        Public Sub New()
            MyBase.New({"Model"})
        End Sub

        Public Overrides Function SupportedTypes() As IEnumerable(Of TypeInfo)
            Return {GetType(TextFile).GetTypeInfo}
        End Function

        Public Overrides Sub DoAction(Targets As IEnumerable(Of Object))
            Throw New NotImplementedException()
        End Sub
    End Class

End Namespace
