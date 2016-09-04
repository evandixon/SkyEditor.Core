Imports SkyEditor.Core.IO

Namespace TestComponents

    ''' <summary>
    ''' <see cref="IOProvider"/> that stores virtual files in memory.
    ''' </summary>
    Public Class MemoryIOProvider
        Inherits IOProvider

        Public Sub New()
            EnableInMemoryLoad = True
            Files = New Dictionary(Of String, Byte())
        End Sub

        Private Property Files As Dictionary(Of String, Byte())

        Public Property EnableInMemoryLoad As Boolean

#Region "IOProvider Implementation"
        Public Overrides Sub CopyFile(sourceFilename As String, destinationFilename As String)
            WriteAllBytes(destinationFilename, ReadAllBytes(sourceFilename))
        End Sub

        Public Overrides Sub CreateDirectory(path As String)
            If Not String.IsNullOrEmpty(path) Then
                'Create the parent directory
                Dim parentPath = System.IO.Path.GetDirectoryName(path)?.Replace("\", "/")
                If Not DirectoryExists(parentPath) Then
                    CreateDirectory(parentPath)
                End If

                'Create the directory
                Files.Add(path, Nothing)
            End If
        End Sub

        Public Overrides Sub DeleteDirectory(path As String)
            If DirectoryExists(path) Then
                'Delete child items
                Dim pathStart = path.ToLowerInvariant & "/"
                Dim childrenToDelete = Files.Where(Function(x) x.Key.ToLowerInvariant.StartsWith(pathStart)).ToList
                For Each item In childrenToDelete
                    Files.Remove(item.Key)
                Next

                'Delete the directory
                Dim toDelete = Files.FirstOrDefault(Function(x) x.Key.ToLowerInvariant = path.ToLowerInvariant And x.Value Is Nothing)
                Files.Remove(toDelete.Key)
            End If
        End Sub

        Public Overrides Sub DeleteFile(filename As String)
            If FileExists(filename) Then
                Dim toDelete = Files.FirstOrDefault(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing)
                Files.Remove(toDelete.Key)
            End If
        End Sub

        Public Overrides Sub WriteAllBytes(filename As String, data() As Byte)
            If FileExists(filename) Then
                DeleteFile(filename)
            End If
            Files.Add(filename, data)
        End Sub

        Public Overrides Sub WriteAllText(filename As String, Data As String)
            WriteAllBytes(filename, Text.Encoding.UTF8.GetBytes(Data))
        End Sub

        Public Overrides Function CanLoadFileInMemory(fileSize As Long) As Boolean
            Return EnableInMemoryLoad
        End Function

        Public Overrides Function DirectoryExists(path As String) As Boolean
            Return Files.Any(Function(x) x.Key.ToLowerInvariant = path.ToLowerInvariant AndAlso x.Value Is Nothing)
        End Function

        Public Overrides Function FileExists(filename As String) As Boolean
            Return Files.Any(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing)
        End Function

        Public Overrides Function GetDirectories(path As String, topDirectoryOnly As Boolean) As String()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetFileLength(filename As String) As Long
            Return Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value.Length
        End Function

        Public Overrides Function GetFiles(path As String, searchPattern As String, topDirectoryOnly As Boolean) As String()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetTempDirectory() As String
            Dim filename As String = Nothing
            While String.IsNullOrEmpty(filename) OrElse DirectoryExists(filename)
                filename = "/temp/" & Guid.NewGuid.ToString
            End While
            CreateDirectory(filename)
            Return filename
        End Function

        Public Overrides Function GetTempFilename() As String
            Dim filename As String = Nothing
            While String.IsNullOrEmpty(filename) OrElse FileExists(filename)
                filename = "/temp/" & Guid.NewGuid.ToString
            End While
            WriteAllBytes(filename, {})
            Return filename
        End Function

        Public Overrides Function OpenFile(filename As String) As Stream
            Return New MemoryStream(Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value, True)
        End Function

        Public Overrides Function OpenFileReadOnly(filename As String) As Stream
            Return New MemoryStream(Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value, False)
        End Function

        Public Overrides Function OpenFileWriteOnly(filename As String) As Stream
            Return New MemoryStream(Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value, True)
        End Function

        Public Overrides Function ReadAllBytes(filename As String) As Byte()
            Return Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value
        End Function

        Public Overrides Function ReadAllText(filename As String) As String
            Dim bytes = ReadAllBytes(filename)
            Return Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length)
        End Function
#End Region
    End Class

End Namespace
