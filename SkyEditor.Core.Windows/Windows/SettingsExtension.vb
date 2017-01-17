Imports System.Runtime.CompilerServices

Namespace Windows
    Public Module SettingsExtension

        <Extension> Function GetOnlineExtensionCollections(provider As ISettingsProvider) As IList(Of String)
            Dim setting As IList(Of String) = provider.GetSetting(My.Resources.SettingNames.OnlineExtensionCollections)

            If setting Is Nothing OrElse TypeOf setting IsNot IList(Of String) Then
                setting = New List(Of String)
            End If

            Return setting
        End Function

        <Extension> Sub SetOnlineExtensionCollections(provider As ISettingsProvider, value As IList(Of String))
            provider.SetSetting(My.Resources.SettingNames.OnlineExtensionCollections, value)
        End Sub

        <Extension> Sub AddOnlineExtensionCollection(provider As ISettingsProvider, url As String)
            Dim collections = GetOnlineExtensionCollections(provider)
            collections.Add(url)
            SetOnlineExtensionCollections(provider, collections)
        End Sub

        <Extension> Sub RemoveOnlineExtensionCollection(provider As ISettingsProvider, url As String)
            Dim collections = GetOnlineExtensionCollections(provider)
            collections.Remove(url)
            SetOnlineExtensionCollections(provider, collections)
        End Sub
    End Module
End Namespace