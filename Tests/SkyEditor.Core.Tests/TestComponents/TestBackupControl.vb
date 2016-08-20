Imports System.Reflection

Namespace TestComponents
    Public Class TestBackupControl
        Inherits TestObjectControlDirectModelBind

        Public Overrides Function GetSupportedTypes() As IEnumerable(Of Type)
            Return {GetType(TextFile).GetTypeInfo}
        End Function

        Public Overrides Function SupportsObject(Obj As Object) As Boolean
            Return TypeOf Obj Is TextFile
        End Function

        Public Overrides Function IsBackupControl() As Boolean
            Return True
        End Function

    End Class

End Namespace
