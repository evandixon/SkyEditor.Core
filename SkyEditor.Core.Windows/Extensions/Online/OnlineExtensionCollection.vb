Imports System.Collections.Concurrent
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.Utilities

Namespace Extensions.Online
    ''' <summary>
    ''' A collection of extensions stored online.
    ''' </summary>
    Public Class OnlineExtensionCollection
        Implements IExtensionCollection

        ''' <summary>
        ''' Creates a new instance of <see cref="OnlineExtensionCollection"/>.
        ''' </summary>
        ''' <param name="rootEndpoint">The root endpoint for connecting to the collection.</param>
        Public Sub New(rootEndpoint As String)
            Client = New Net.WebClient
            rootEndpoint = rootEndpoint
            CachedInfo = New Dictionary(Of Integer, OnlineExtensionInfo)
        End Sub

        Private Property Client As Net.WebClient
        Private Property RootEndpoint As String
        Private Property GetExtensionsEndpoint As String
        Private Property CachedInfo As Dictionary(Of Integer, OnlineExtensionInfo)


        Private Async Function GetResponse() As Task(Of RootCollectionResponse)
            If _response Is Nothing Then
                _response = Json.Deserialize(Of RootCollectionResponse)(Await Client.DownloadStringTaskAsync(New Uri(RootEndpoint)))
            End If
            Return _response
        End Function
        Private _response As RootCollectionResponse

        Public Async Function GetName() As Task(Of String) Implements IExtensionCollection.GetName
            If String.IsNullOrEmpty(_name) Then
                _name = (Await GetResponse()).Name
            End If
            Return _name
        End Function
        Dim _name As String

        Public Async Function GetChildCollections(manager As PluginManager) As Task(Of IEnumerable(Of IExtensionCollection)) Implements IExtensionCollection.GetChildCollections
            If _childCollections Is Nothing Then
                For Each item In (Await GetResponse()).ChildCollectionEndpoints
                    If Not item = RootEndpoint Then
                        _childCollections.Add(New OnlineExtensionCollection(item))
                    End If
                Next
            End If
            Return _childCollections
        End Function
        Dim _childCollections As List(Of OnlineExtensionCollection)

        Public Async Function GetExtensionCount(manager As PluginManager) As Task(Of Integer) Implements IExtensionCollection.GetExtensionCount
            If Not _extensionCount.HasValue Then
                _extensionCount = (Await GetResponse()).ExtensionCount
            End If
            Return _extensionCount
        End Function
        Dim _extensionCount As Integer?

        Public Async Function GetExtensions(skip As Integer, take As Integer, manager As PluginManager) As Task(Of IEnumerable(Of ExtensionInfo)) Implements IExtensionCollection.GetExtensions
            Dim responseRaw = Await Client.DownloadStringTaskAsync((Await GetResponse()).GetExtensionListEndpoint & $"?skip={skip}&take={take}")
            Dim response = Json.Deserialize(Of List(Of OnlineExtensionInfo))(responseRaw)

            Dim i As Integer = skip
            For Each item In response
                item.IsInstalled = ExtensionHelper.IsExtensionInstalled(item, manager)

                If CachedInfo.ContainsKey(i) Then
                    CachedInfo(i) = item
                Else
                    CachedInfo.Add(i, item)
                End If
                i += 1
            Next

            Return response
        End Function

        Public Async Function InstallExtension(extensionID As Guid, manager As PluginManager) As Task(Of ExtensionInstallResult) Implements IExtensionCollection.InstallExtension
            'Download zip
            Dim tempName = manager.CurrentIOProvider.GetTempFilename
            Await Client.DownloadFileTaskAsync((Await GetResponse()).DownloadExtensionEndpoint & $"?id={extensionID}", tempName)

            'Install
            Dim result = Await ExtensionHelper.InstallExtensionZip(tempName, manager)

            'Clean up
            manager.CurrentIOProvider.DeleteFile(tempName)

            Return result
        End Function

        Public Function UninstallExtension(extensionID As Guid, manager As PluginManager) As Task(Of ExtensionUninstallResult) Implements IExtensionCollection.UninstallExtension
            Dim typeName As String = (From c In CachedInfo.Values Where c.ID = extensionID Select c.ExtensionTypeName).First
            Return ExtensionHelper.UninstallExtension(typeName, extensionID, manager)
        End Function
    End Class
End Namespace

