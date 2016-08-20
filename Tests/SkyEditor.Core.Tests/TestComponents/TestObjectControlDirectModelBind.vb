Imports System.Reflection
Imports SkyEditor.Core.UI

Namespace TestComponents
    Public Class TestObjectControlDirectModelBind
        Implements IObjectControl

        Public Property EditingObject As Object Implements IObjectControl.EditingObject
        Public Property CurrentPluginManager As PluginManager

        Public ReadOnly Property Header As String Implements IObjectControl.Header

        Public Property IsModified As Boolean Implements IObjectControl.IsModified

        Public Event HeaderUpdated As IObjectControl.HeaderUpdatedEventHandler Implements IObjectControl.HeaderUpdated
        Public Event IsModifiedChanged As IObjectControl.IsModifiedChangedEventHandler Implements IObjectControl.IsModifiedChanged

        Public Sub SetPluginManager(manager As PluginManager) Implements IObjectControl.SetPluginManager
            Me.CurrentPluginManager = manager
        End Sub

        Public Function GetSortOrder(CurrentType As Type, IsTab As Boolean) As Integer Implements IObjectControl.GetSortOrder
            Return 0
        End Function

        Public Overridable Function GetSupportedTypes() As IEnumerable(Of Type) Implements IObjectControl.GetSupportedTypes
            Return {GetType(TextFile).GetTypeInfo}
        End Function

        Public Overridable Function IsBackupControl() As Boolean Implements IObjectControl.IsBackupControl
            Return False
        End Function

        Public Overridable Function SupportsObject(Obj As Object) As Boolean Implements IObjectControl.SupportsObject
            Return TypeOf Obj Is TextFile
        End Function
    End Class
End Namespace

