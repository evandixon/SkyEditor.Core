Imports System.Reflection

Namespace UI
    Public MustInherit Class GenericViewModel

        ''' <summary>
        ''' The underlying model
        ''' </summary>
        Public Overridable Property Model As Object

        ''' <summary>
        ''' Instance of the current plugin manager
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property CurrentPluginManager As Object

        ''' <summary>
        ''' Returns an IEnumerable of every type that the view model is programmed to handle.
        ''' </summary>
        Public MustOverride Function GetSupportedTypes() As IEnumerable(Of TypeInfo)

        ''' <summary>
        ''' Returns whether or not the view model supports the given object.
        ''' </summary>
        Public Overridable Function SupportsObject(Obj As Object) As Boolean
            Dim currentType = Obj.GetType.GetTypeInfo
            Return GetSupportedTypes.Any(Function(x As TypeInfo) As Boolean
                                             Return currentType.Equals(x)
                                         End Function)
        End Function

        ''' <summary>
        ''' Determines whether or not this iObjectControl should be used for the given object if another control exists for it.
        ''' If false, this will be used if SupportsObject(Obj) is true.
        ''' If true, this will only be used if no other iObjectControl can edit the given object.
        ''' 
        ''' If multiple backup controls are present, GetSortOrder will be used to determine which iObjectControl is used.
        ''' </summary>
        ''' <param name="Obj"></param>
        ''' <returns></returns>
        Public Overridable Function IsBackupViewModel(Obj As Object) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Returns the sort order of this control when editing the given type.
        ''' Note: The returned value is context-specific.  Higher values make a Control more likely to be used, but lower values make tabs appear higher in the list of tabs.
        ''' Note: Negative values will result in the control not being used if there are other controls with positive values.
        ''' </summary>
        ''' <param name="CurrentType">Type of the EditingObject to get a sort order for.</param>
        ''' <param name="IsTab">Whether or not the iObjectControl is registered to behave as a Tab or a Control.</param>
        ''' <returns></returns>
        Public Overridable Function GetSortOrder(CurrentType As Type, IsTab As Boolean) As Integer
            Return 0
        End Function

        Public Overridable Sub SetPluginManager(manager As PluginManager)
            Me.CurrentPluginManager = manager
        End Sub

        Public Overridable Sub SetModel(model As Object)
            Me.Model = model
        End Sub
    End Class

End Namespace
