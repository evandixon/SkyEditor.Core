Imports System.Reflection
Imports SkyEditor.Core.Utilities

Namespace IO
    ''' <summary>
    ''' Opens files using file types that implement IOpenableFile
    ''' </summary>
    Public Class OpenableFileOpener
        Implements IFileOpener        

        Public Async Function OpenFile(fileType As TypeInfo, filename As String, provider As IIOProvider) As Task(Of Object) Implements IFileOpener.OpenFile
            Dim file As IOpenableFile = ReflectionHelpers.CreateInstance(fileType)
            Await file.OpenFile(filename, provider)
            Return file
        End Function

        Public Function SupportsType(fileType As TypeInfo) As Boolean Implements IFileOpener.SupportsType
            Return ReflectionHelpers.IsOfType(fileType, GetType(IOpenableFile).GetTypeInfo)
        End Function

        Public Function GetUsagePriority(fileType As TypeInfo) As Integer Implements IFileOpener.GetUsagePriority
            Return 0
        End Function
    End Class
End Namespace

