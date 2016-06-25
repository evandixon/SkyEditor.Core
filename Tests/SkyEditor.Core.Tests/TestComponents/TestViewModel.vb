Imports System.Reflection
Imports SkyEditor.Core.UI

Namespace TestComponents
    Public Class TestViewModel
        Inherits GenericViewModel

        Public Overrides Function GetSupportedTypes() As IEnumerable(Of TypeInfo)
            Return {GetType(TextFile).GetTypeInfo}
        End Function
    End Class
End Namespace

