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
        ''' <returns>An object representing the requested file</returns>
        Function OpenFile(fileType As TypeInfo, filename As String, provider As IIOProvider) As Task(Of Object)

        ''' <summary>
        ''' Determines whether or not the IFileOpener supports opening a file of the given type
        ''' </summary>
        ''' <param name="fileType">Type of the file to open</param>
        ''' <returns>A boolean indicating whether or not the current <see cref="IFileOpener"/> supports the given file type.</returns>
        Function SupportsType(fileType As TypeInfo) As Boolean

        ''' <summary>
        ''' Gets the priority of this <see cref="IFileOpener"/> to be used for the given type.
        ''' </summary>
        ''' <param name="fileType">Type of the file to open</param>
        ''' <returns>An integer indicating the usage priority for the given file type.  Higher numbers give higher priority.</returns>
        Function GetUsagePriority(fileType As TypeInfo) As Integer
    End Interface

End Namespace