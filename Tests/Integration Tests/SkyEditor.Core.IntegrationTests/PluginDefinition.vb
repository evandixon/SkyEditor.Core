﻿Imports SkyEditor.Core
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Windows.CoreMods

Public Class PluginDefinition
    Inherits WindowsCoreSkyEditorPlugin

    Public Const IntegrationTestCategory = "Integration Tests"

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

End Class
