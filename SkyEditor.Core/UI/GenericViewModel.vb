Imports System.Reflection
Imports SkyEditor.Core.Utilities

Namespace UI
    Public MustInherit Class GenericViewModel

        Public Sub New()
        End Sub

        Public Sub New(model As Object, manager As PluginManager)
            SetPluginManager(manager)
            SetModel(model)
        End Sub

#Region "Events"
        Public Event MenuItemRefreshRequested(sender As Object, e As EventArgs)
        Protected Sub RequestMenuItemRefresh()
            RaiseEvent MenuItemRefreshRequested(Me, New EventArgs)
        End Sub
#End Region

        ''' <summary>
        ''' The underlying model
        ''' </summary>
        Public Overridable Property Model As Object

        ''' <summary>
        ''' Instance of the current plugin manager
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property CurrentPluginManager As PluginManager

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
                                             Return ReflectionHelpers.IsOfType(currentType, x)
                                         End Function)
        End Function

        Public Overridable Function GetSortOrder() As Integer
            Return 0
        End Function

        Public Overridable Sub SetPluginManager(manager As PluginManager)
            Me.CurrentPluginManager = manager
        End Sub

        Public Overridable Sub SetModel(model As Object)
            Me.Model = model
        End Sub

        ''' <summary>
        ''' Updates the model with the view model's current state
        ''' </summary>
        ''' <param name="model">Model to update</param>
        Public Overridable Sub UpdateModel(model As Object)

        End Sub
    End Class

    Public MustInherit Class GenericViewModel(Of T)
        Inherits GenericViewModel

        Public Sub New()
        End Sub

        Public Sub New(model As T, manager As PluginManager)
            SetPluginManager(manager)
            SetModel(model)
        End Sub

        Public Shadows Property Model As T
            Get
                Return MyBase.Model
            End Get
            Set(value As T)
                MyBase.Model = value
            End Set
        End Property

        Public Overrides Function GetSupportedTypes() As IEnumerable(Of TypeInfo)
            Return {GetType(T).GetTypeInfo}
        End Function
    End Class

End Namespace
