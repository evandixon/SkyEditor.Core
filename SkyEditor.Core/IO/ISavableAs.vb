Namespace IO

    ''' <summary>
    ''' Implemented by objects that can save themselves to disk.
    ''' </summary>
    Public Interface ISavableAs
        Inherits ISavable
        ''' <summary>
        ''' Saves the class to the given filename.
        ''' </summary>
        ''' <param name="Filename"></param>
        Overloads Sub Save(Filename As String, provider As IOProvider)

        ''' <summary>
        ''' Gets the default extension for the file.
        ''' </summary>
        ''' <returns>A string containing the default extension for the file.</returns>
        Function GetDefaultExtension() As String

        ''' <summary>
        ''' Gets the supported extensions for the file.
        ''' </summary>
        ''' <returns>An IEnumerable that contains every extension that can be used to save this file.</returns>
        Function GetSupportedExtensions() As IEnumerable(Of String)
    End Interface

End Namespace
