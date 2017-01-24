﻿Namespace IO
    Public Interface IIOProvider
        ''' <summary>
        ''' Gets the length, in bytes, of the file at the given path.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns>The length, in bytes, of the file</returns>
        Function GetFileLength(filename As String) As Long

        ''' <summary>
        ''' Determines whether the specified file exists.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns></returns>
        Function FileExists(filename As String) As Boolean

        ''' <summary>
        ''' Determines whether the specified directory exists.
        ''' </summary>
        ''' <param name="path">Full path of the directory.</param>
        ''' <returns></returns>
        Function DirectoryExists(path As String) As Boolean

        ''' <summary>
        ''' Creates a directory at the specified path.
        ''' </summary>
        ''' <param name="path"></param>
        Sub CreateDirectory(path As String)

        ''' <summary>
        ''' Gets the full paths of the files in the directory at the given path.
        ''' </summary>
        ''' <param name="path">Full path of the directory from which to get the files.</param>
        ''' <param name="searchPattern">The search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but doesn't support regular expressions.</param>
        ''' <param name="topDirectoryOnly">True to search only the top directory.  False to search all child directories too.</param>
        ''' <returns></returns>
        Function GetFiles(path As String, searchPattern As String, topDirectoryOnly As Boolean) As String()

        ''' <summary>
        ''' Gets the full paths of the directories in the directory at the given path
        ''' </summary>
        ''' <param name="path">Full path of the directory from which to get the files.</param>
        ''' <param name="topDirectoryOnly">True to search only the top directory.  False to search all child directories too.</param>
        ''' <returns></returns>
        Function GetDirectories(path As String, topDirectoryOnly As Boolean) As String()

        ''' <summary>
        ''' Reads a file from disk, and returns its contents as a byte array.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns></returns>
        Function ReadAllBytes(filename As String) As Byte()

        ''' <summary>
        ''' Writes the given text to disk.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <param name="data">File contents to be written.</param>
        Sub WriteAllText(filename As String, Data As String)

        ''' <summary>
        ''' Reads a file from disk, and returns its contents as a string.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns></returns>
        Function ReadAllText(filename As String) As String

        ''' <summary>
        ''' Writes the given byte array to disk.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <param name="data">File contents to be written.</param>
        Sub WriteAllBytes(filename As String, data As Byte())

        ''' <summary>
        ''' Copies a file, overwriting the destination file if it exists.
        ''' </summary>
        ''' <param name="sourceFilename"></param>
        ''' <param name="destinationFilename"></param>
        Sub CopyFile(sourceFilename As String, destinationFilename As String)

        ''' <summary>
        ''' Deletes the file at the given path.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        Sub DeleteFile(filename As String)

        ''' <summary>
        ''' Deletes the directory at the given path, and all of its contents.
        ''' </summary>
        ''' <param name="path"></param>
        Sub DeleteDirectory(path As String)

        ''' <summary>
        ''' Creates a temporary, blank file and returns its full path.
        ''' </summary>
        ''' <returns></returns>
        Function GetTempFilename() As String

        ''' <summary>
        ''' Creates a temporary empty directory and returns its full path.
        ''' </summary>
        ''' <returns></returns>
        Function GetTempDirectory() As String

        ''' <summary>
        ''' Determines whether or not a file of the given size will fit in memory.
        ''' </summary>
        ''' <param name="fileSize">Full path of the file.</param>
        ''' <returns></returns>
        Function CanLoadFileInMemory(fileSize As Long) As Boolean

        ''' <summary>
        ''' Opens a file stream with Read/Write privilages.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns></returns>
        Function OpenFile(filename As String) As Stream

        ''' <summary>
        ''' Opens a file stream with Read privilages.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns></returns>
        Function OpenFileReadOnly(filename As String) As Stream

        ''' <summary>
        ''' Opens a file stream with Write privilages.
        ''' </summary>
        ''' <param name="filename">Full path of the file.</param>
        ''' <returns></returns>
        Function OpenFileWriteOnly(filename As String) As Stream
    End Interface

End Namespace