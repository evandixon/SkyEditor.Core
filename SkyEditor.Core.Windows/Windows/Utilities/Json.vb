Namespace Windows.Utilities
    Partial Public Class Json
        ''' <summary>
        ''' Serializes the given object into JSON and writes it to disk.
        ''' </summary>
        ''' <param name="Filename">Filename to store the JSON.</param>
        ''' <param name="ObjectToSerialize">Object to serialize.</param>
        Public Shared Sub SerializeToFile(Filename As String, ObjectToSerialize As Object)
            SkyEditor.Core.Utilities.Json.SerializeToFile(Filename, ObjectToSerialize, New Windows.Providers.WindowsIOProvider)
        End Sub

        ''' <summary>
        ''' Deserializes JSON stored on disk.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="Filename">Path to the text file containing a JSON string.</param>
        ''' <returns></returns>
        Public Shared Function DeserializeFromFile(Of T)(Filename As String) As T
            Return SkyEditor.Core.Utilities.Json.DeserializeFromFile(Of T)(Filename, New Windows.Providers.WindowsIOProvider)
        End Function
    End Class
End Namespace