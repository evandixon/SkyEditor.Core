Imports System.Reflection

Namespace TestComponents
    Public Class TestObjectControlInterfaceViewModelBind
        Inherits TestObjectControlDirectModelBind
        Public Overrides Function GetSupportedTypes() As IEnumerable(Of Type)
            Return {GetType(TestInterface).GetTypeInfo}
        End Function

        Public Overrides Function SupportsObject(Obj As Object) As Boolean
            Return TypeOf Obj Is TestInterface
        End Function
    End Class

End Namespace