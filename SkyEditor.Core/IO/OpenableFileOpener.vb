Imports System.Reflection
Imports SkyEditor.Core.Utilities

Namespace IO
    ''' <summary>
    ''' Opens files using file types that implement IOpenableFile
    ''' </summary>
    Public Class OpenableFileOpener
        Implements IFileOpener

        Public Async Function OpenFile(fileType As TypeInfo, filename As String, provider As IOProvider) As Task(Of Object) Implements IFileOpener.OpenFile
            Dim file As IOpenableFile = ReflectionHelpers.CreateInstance(fileType)
            Await file.OpenFile(filename, provider).ConfigureAwait(False)
            Return file
        End Function

        Public Function SupportsType(fileType As TypeInfo) As Boolean Implements IFileOpener.SupportsType
            Return ReflectionHelpers.IsOfType(fileType, GetType(IOpenableFile).GetTypeInfo)
        End Function
    End Class
End Namespace

