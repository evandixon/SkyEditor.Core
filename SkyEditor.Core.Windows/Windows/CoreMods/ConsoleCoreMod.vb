Namespace Windows.CoreMods
    Public Class ConsoleCoreMod
        Inherits WindowsCoreSkyEditorPlugin

        Public Overrides ReadOnly Property Credits As String
            Get
                Return My.Resources.Language.ConsoleCoreCredits
            End Get
        End Property

        Public Overrides ReadOnly Property PluginAuthor As String
            Get
                Return My.Resources.Language.ConsoleCoreAuthor
            End Get
        End Property

        Public Overrides ReadOnly Property PluginName As String
            Get
                Return My.Resources.Language.ConsoleCorePluginName
            End Get
        End Property
    End Class
End Namespace

