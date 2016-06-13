Imports SkyEditor.Core.Windows.CoreMods

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

End Class
