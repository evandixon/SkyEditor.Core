Imports System.Reflection

Namespace IO
    ''' <summary>
    ''' Creates a model from a file
    ''' </summary>
    Public Interface IFileOpener
        ''' <summary>
        ''' Creates an instance of fileType from the given filename.
        ''' </summary>
        ''' <param name="fileType">Type of the file to open</param>
        ''' <param name="filename">Full path of the file to open</param>
        ''' <param name="provider">Instance of the current IO provider</param>
        ''' <returns></returns>
        Function OpenFile(fileType As TypeInfo, filename As String, provider As IOProvider) As Task(Of Object)

        ''' <summary>
        ''' Determines whether or not the IFileOpener supports opening a file of the given type
        ''' </summary>
        ''' <param name="fileType">Type of the file to open</param>
        ''' <returns></returns>
        Function SupportsType(fileType As TypeInfo) As Boolean
    End Interface

End Namespace