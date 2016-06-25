Imports SkyEditor.Core.IO
Imports SkyEditor.Core.UI
Imports SkyEditor.Core.Windows.CoreMods

Namespace TestComponents
    Public Class TestCoreMod
        Inherits WindowsCoreSkyEditorPlugin

        Public Overrides ReadOnly Property Credits As String
            Get
                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property PluginAuthor As String
            Get
                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property PluginName As String
            Get
                Return ""
            End Get
        End Property

        Public Overrides Function IsPluginLoadingEnabled() As Boolean
            Return False
        End Function

        Public Overrides Sub Load(manager As PluginManager)
            MyBase.Load(manager)

            manager.RegisterType(Of IObjectControl)(GetType(TestObjectControlDirectModelBind))
            manager.RegisterType(Of IObjectControl)(GetType(TestObjectControlViewModelBind))
            manager.RegisterType(Of GenericViewModel)(GetType(TestViewModel))
            manager.RegisterType(Of IOpenableFile)(GetType(TextFile))
        End Sub

    End Class

End Namespace