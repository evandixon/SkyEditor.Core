Imports SkyEditor.Core.IO

Namespace TestComponents

    ''' <summary>
    ''' <see cref="IIOProvider"/> that stores virtual files in memory.
    ''' </summary>
    Public Class MemoryIOProvider
        Implements IIOProvider

        Public Sub New()
            EnableInMemoryLoad = True
            Files = New Dictionary(Of String, Byte())
        End Sub

        Private Property Files As Dictionary(Of String, Byte())

        Public Property EnableInMemoryLoad As Boolean

#Region "IOProvider Implementation"
        Public Overridable Sub CopyFile(sourceFilename As String, destinationFilename As String) Implements IIOProvider.CopyFile
            WriteAllBytes(destinationFilename, ReadAllBytes(sourceFilename))
        End Sub

        Public Overridable Sub CreateDirectory(path As String) Implements IIOProvider.CreateDirectory
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

        Public Overridable Sub DeleteDirectory(path As String) Implements IIOProvider.DeleteDirectory
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

        Public Overridable Sub DeleteFile(filename As String) Implements IIOProvider.DeleteFile
            If FileExists(filename) Then
                Dim toDelete = Files.FirstOrDefault(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing)
                Files.Remove(toDelete.Key)
            End If
        End Sub

        Public Overridable Sub WriteAllBytes(filename As String, data() As Byte) Implements IIOProvider.WriteAllBytes
            If FileExists(filename) Then
                DeleteFile(filename)
            End If
            Files.Add(filename, data)
        End Sub

        Public Overridable Sub WriteAllText(filename As String, Data As String) Implements IIOProvider.WriteAllText
            WriteAllBytes(filename, Text.Encoding.UTF8.GetBytes(Data))
        End Sub

        Public Overridable Function CanLoadFileInMemory(fileSize As Long) As Boolean Implements IIOProvider.CanLoadFileInMemory
            Return EnableInMemoryLoad
        End Function

        Public Overridable Function DirectoryExists(path As String) As Boolean Implements IIOProvider.DirectoryExists
            Return Files.Any(Function(x) x.Key.ToLowerInvariant = path.ToLowerInvariant AndAlso x.Value Is Nothing)
        End Function

        Public Overridable Function FileExists(filename As String) As Boolean Implements IIOProvider.FileExists
            Return Files.Any(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing)
        End Function

        Public Overridable Function GetDirectories(path As String, topDirectoryOnly As Boolean) As String() Implements IIOProvider.GetDirectories
            Throw New NotImplementedException()
        End Function

        Public Overridable Function GetFileLength(filename As String) As Long Implements IIOProvider.GetFileLength
            Return Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value.Length
        End Function

        Public Overridable Function GetFiles(path As String, searchPattern As String, topDirectoryOnly As Boolean) As String() Implements IIOProvider.GetFiles
            Throw New NotImplementedException()
        End Function

        Public Overridable Function GetTempDirectory() As String Implements IIOProvider.GetTempDirectory
            Dim filename As String = Nothing
            While String.IsNullOrEmpty(filename) OrElse DirectoryExists(filename)
                filename = "/temp/" & Guid.NewGuid.ToString
            End While
            CreateDirectory(filename)
            Return filename
        End Function

        Public Overridable Function GetTempFilename() As String Implements IIOProvider.GetTempFilename
            Dim filename As String = Nothing
            While String.IsNullOrEmpty(filename) OrElse FileExists(filename)
                filename = "/temp/" & Guid.NewGuid.ToString
            End While
            WriteAllBytes(filename, {})
            Return filename
        End Function

        Public Overridable Function OpenFile(filename As String) As Stream Implements IIOProvider.OpenFile
            Return New MemoryStream(Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value, True)
        End Function

        Public Overridable Function OpenFileReadOnly(filename As String) As Stream Implements IIOProvider.OpenFileReadOnly
            Return New MemoryStream(Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value, False)
        End Function

        Public Overridable Function OpenFileWriteOnly(filename As String) As Stream Implements IIOProvider.OpenFileWriteOnly
            Return New MemoryStream(Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value, True)
        End Function

        Public Overridable Function ReadAllBytes(filename As String) As Byte() Implements IIOProvider.ReadAllBytes
            Return Files.First(Function(x) x.Key.ToLowerInvariant = filename.ToLowerInvariant AndAlso x.Value IsNot Nothing).Value
        End Function

        Public Overridable Function ReadAllText(filename As String) As String Implements IIOProvider.ReadAllText
            Dim bytes = ReadAllBytes(filename)
            Return Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length)
        End Function
#End Region
    End Class

End Namespace