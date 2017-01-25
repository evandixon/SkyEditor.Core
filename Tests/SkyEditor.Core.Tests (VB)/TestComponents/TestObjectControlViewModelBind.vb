Imports System.Reflection

Namespace TestComponents
    Public Class TestObjectControlViewModelBind
        Inherits TestObjectControlDirectModelBind
        Public Overrides Function GetSupportedTypes() As IEnumerable(Of Type)
            Return {GetType(TestViewModel).GetTypeInfo}
        End Function

        Public Overrides Function SupportsObject(Obj As Object) As Boolean
            Return TypeOf Obj Is TestViewModel
        End Function
    End Class
End Namespace