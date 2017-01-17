Imports SkyEditor.Core
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Windows.CoreMods

Public Class StandardPluginDefinition
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
            Return "Integration Tests"
        End Get
    End Property

    Public Overrides Sub Load(manager As PluginManager)
        MyBase.Load(manager)

        manager.LoadRequiredPlugin(New SkyEditor.Core.Tests.TestComponents.TestCoreMod, Me)
    End Sub

End Class