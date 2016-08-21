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

        ''' <summary>
        ''' Sets the <see cref="GenericViewModel"/>'s model.
        ''' </summary>
        ''' <param name="model">Model to set</param>
        Public Overridable Sub SetModel(model As Object)
            Me.Model = model
        End Sub

        ''' <summary>
        ''' Updates the model with the view model's current state
        ''' </summary>
        ''' <param name="model">Model to update</param>
        Public Overridable Sub UpdateModel(model As Object)

        End Sub

        ''' <summary>
        ''' Gets whether or not a view model of the given type is loaded for the same model.
        ''' </summary>
        ''' <typeparam name="T">Type of the view model of which to select.</typeparam>
        ''' <returns>Whether or not there is a view model of type <typeparamref name="T"/> for the same model.</returns>
        Public Function HasSiblingViewModel(Of T)() As Boolean
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' Gets a view model for the same model.
        ''' </summary>
        ''' <typeparam name="T">Type of the view model of which to select.</typeparam>
        ''' <returns>The view model of type <typeparamref name="T"/> for the same model.</returns>
        Public Function GetSiblingViewModel(Of T)() As T
            Throw New NotImplementedException
        End Function

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
