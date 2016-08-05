Namespace Extensions.Online
    Public Class RootCollectionResponse
        Public Property Name As String
        Public Property ChildCollections As List(Of ExtensionCollectionModel)
        Public Property ExtensionCount As Integer
        Public Property GetExtensionListEndpoint As String
        Public Property DownloadExtensionEndpoint As String
    End Class

End Namespace
