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
        ''' <remarks>The api expects something like the following endpoints:
        ''' api/ExtensionCollection
        ''' api/ExtensionCollection/5
        ''' The endpoint name can vary, but the "/&lt;parentCollectionID&gt;" part must hold true.</remarks>
        Public Sub New(rootEndpoint As String)
            Client = New Net.WebClient
            rootEndpoint = rootEndpoint
            CachedInfo = New Dictionary(Of Integer, OnlineExtensionInfo)
        End Sub

        Public Sub New(rootEndpoint As String, parentCollectionId As Integer)
            Me.New(rootEndpoint)
            Me.ParentCollectionId = parentCollectionId
        End Sub

        Private Property Client As Net.WebClient
        Private Property RootEndpoint As String
        Private Property ParentCollectionId As Integer?
        Private Property GetExtensionsEndpoint As String
        Private Property CachedInfo As Dictionary(Of Integer, OnlineExtensionInfo)


        Private Async Function GetResponse() As Task(Of RootCollectionResponse)
            If _response Is Nothing Then
                Dim endpoint = RootEndpoint
                If ParentCollectionId.HasValue Then
                    endpoint &= "/" & ParentCollectionId.Value
                End If
                _response = Json.Deserialize(Of RootCollectionResponse)(Await Client.DownloadStringTaskAsync(New Uri(endpoint)).ConfigureAwait(False))
            End If
            Return _response
        End Function
        Private _response As RootCollectionResponse

        Public Async Function GetName() As Task(Of String) Implements IExtensionCollection.GetName
            If String.IsNullOrEmpty(_name) Then
                _name = (Await GetResponse.ConfigureAwait(False)).Name
            End If
            Return _name
        End Function
        Dim _name As String

        Public Async Function GetChildCollections(manager As PluginManager) As Task(Of IEnumerable(Of IExtensionCollection)) Implements IExtensionCollection.GetChildCollections
            If _childCollections Is Nothing Then
                For Each item In (Await GetResponse.ConfigureAwait(False)).ChildCollections
                    _childCollections.Add(New OnlineExtensionCollection(Me.RootEndpoint, item.ID))
                Next
            End If
            Return _childCollections
        End Function
        Dim _childCollections As List(Of OnlineExtensionCollection)

        Public Async Function GetExtensionCount(manager As PluginManager) As Task(Of Integer) Implements IExtensionCollection.GetExtensionCount
            If Not _extensionCount.HasValue Then
                _extensionCount = (Await GetResponse.ConfigureAwait(False)).ExtensionCount
            End If
            Return _extensionCount
        End Function
        Dim _extensionCount As Integer?

        Public Async Function GetExtensions(skip As Integer, take As Integer, manager As PluginManager) As Task(Of IEnumerable(Of ExtensionInfo)) Implements IExtensionCollection.GetExtensions
            Dim responseRaw = Await Client.DownloadStringTaskAsync((Await GetResponse.ConfigureAwait(False)).GetExtensionListEndpoint & $"/{skip}/{take}")
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

        Public Async Function InstallExtension(extensionID As String, version As String, manager As PluginManager) As Task(Of ExtensionInstallResult) Implements IExtensionCollection.InstallExtension
            'Download zip
            Dim tempName = manager.CurrentIOProvider.GetTempFilename
            Await Client.DownloadFileTaskAsync((Await GetResponse.ConfigureAwait(False)).DownloadExtensionEndpoint & $"/{extensionID}/{version}", tempName)

            'Install
            Dim result = Await ExtensionHelper.InstallExtensionZip(tempName, manager).ConfigureAwait(False)

            'Clean up
            manager.CurrentIOProvider.DeleteFile(tempName)

            Return result
        End Function

        Public Function UninstallExtension(extensionID As String, manager As PluginManager) As Task(Of ExtensionUninstallResult) Implements IExtensionCollection.UninstallExtension
            Dim typeName As String = (From c In CachedInfo.Values Where c.ID = extensionID Select c.ExtensionTypeName).First
            Return ExtensionHelper.UninstallExtension(typeName, extensionID, manager)
        End Function
    End Class
End Namespace

