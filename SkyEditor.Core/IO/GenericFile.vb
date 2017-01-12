Imports System.Text
Imports SkyEditor.Core.Utilities

Namespace IO
    Public Class GenericFile
        Implements IDisposable
        Implements INamed
        'Implements iCreatableFile 'Excluded because this might not apply to children
        Implements IOpenableFile
        Implements IOnDisk
        Implements ISavableAs

        Private _fileLock As New Object


#Region "Constructors"
        ''' <summary>
        ''' Creates a new instance of GenericFile for use with either GenericFile.OpenFile or GenericFile.CreateFile
        ''' </summary>
        Public Sub New()
            IsReadOnly = False
            EnableInMemoryLoad = False 'This is an opt-in setting
            _enableShadowCopy = Nothing
        End Sub

        ''' <summary>
        ''' Creates a new instance of GenericFile for use with either GenericFile.OpenFile or GenericFile.CreateFile
        ''' </summary>
        Public Sub New(provider As IIOProvider)
            Me.FileProvider = provider
            IsReadOnly = False
            EnableInMemoryLoad = False 'This is an opt-in setting
            _enableShadowCopy = Nothing
        End Sub

        ''' <summary>
        ''' Creates a new instance of GenericFile using the given data.
        ''' </summary>
        ''' <param name="RawData"></param>
        Public Sub New(provider As IIOProvider, rawData As Byte())
            Me.FileProvider = provider
            IsReadOnly = False
            EnableInMemoryLoad = True
            CreateFileInternal("", rawData, EnableInMemoryLoad, provider)
        End Sub

        ''' <summary>
        ''' Creates a new instance of GenericFile from the given file.
        ''' </summary>
        ''' <param name="Filename">Full path of the file to load.</param>
        Public Sub New(provider As IIOProvider, filename As String)
            Me.FileProvider = provider
            Me.IsReadOnly = False
            Me.EnableInMemoryLoad = False
            OpenFileInternal(filename)
        End Sub

        ''' <summary>
        ''' Creates a new instance of GenericFile from the given file.
        ''' </summary>
        ''' <param name="Filename">Full path of the file to load.</param>
        ''' <param name="IsReadOnly">Whether or not to allow altering the file.  If True, an IOException will be thrown when attempting to alter the file.</param>
        Public Sub New(provider As IIOProvider, filename As String, isReadOnly As Boolean)
            Me.FileProvider = provider
            Me.IsReadOnly = isReadOnly
            Me.EnableInMemoryLoad = False
            OpenFileInternal(filename)
        End Sub

        ''' <summary>
        ''' Creates a new instance of GenericFile from the given file.
        ''' </summary>
        ''' <param name="Filename">Full path of the file to load.</param>
        ''' <param name="IsReadOnly">Whether or not to allow altering the file.  If True, an IOException will be thrown when attempting to alter the file, regardless of whether LoadToMemory is true.</param>
        ''' <param name="LoadToMemory">True to load the file into memory, False to use a FileStream.  If loading the file into memory would leave the system with less than 500MB, a FileStream will be used instead.</param>
        Public Sub New(provider As IIOProvider, filename As String, isReadOnly As Boolean, loadToMemory As Boolean)
            Me.FileProvider = provider
            Me.IsReadOnly = isReadOnly
            Me.EnableInMemoryLoad = loadToMemory
            OpenFileInternal(filename)
        End Sub

#End Region

#Region "Properties"

        ''' <summary>
        ''' Platform dependant abstraction layer for the file system.
        ''' </summary>
        ''' <returns></returns>
        Protected Property FileProvider As IIOProvider

#Region "File Loading/Management"
        ''' <summary>
        ''' Whether or not to allow altering the file.
        ''' </summary>
        ''' <returns></returns>
        Public Property IsReadOnly As Boolean

        ''' <summary>
        ''' Determines whether or not the file will be loaded into memory completely, or located on disk.
        ''' </summary>
        ''' <returns></returns>
        Public Property EnableInMemoryLoad As Boolean
            Get
                Return (_enableInMemoryLoad.HasValue AndAlso _enableInMemoryLoad.Value) OrElse (Not _enableInMemoryLoad.HasValue AndAlso PhysicalFilename Is Nothing)
            End Get
            Set(value As Boolean)
                _enableInMemoryLoad = value
            End Set
        End Property
        Dim _enableInMemoryLoad As Boolean?

        ''' <summary>
        ''' The logical location of the file.
        ''' Null if the current file was created and has never been saved.
        ''' </summary>
        ''' <returns></returns>
        Public Property OriginalFilename As String Implements IOnDisk.Filename

        ''' <summary>
        ''' The location of the file being accessed by the internal FileReader.
        ''' If EnableShadowCopy is True, will be the location of a temporary file.  If False, it is equal to OriginalFilename.
        ''' Null if EnableInMemoryLoad is True.
        ''' </summary>
        ''' <returns></returns>
        Public Property PhysicalFilename As String
            Get
                Return _physicalFilename
            End Get
            Private Set(value As String)
                _physicalFilename = value
            End Set
        End Property
        Dim _physicalFilename As String

        ''' <summary>
        ''' Name of the file.
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String Implements INamed.Name
            Get
                If _name Is Nothing Then
                    Return Path.GetFileName(OriginalFilename)
                Else
                    Return _name
                End If
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property
        Dim _name As String

        ''' <summary>
        ''' Whether or not to make a shadow copy of the file before loading it.
        ''' </summary>
        ''' <returns></returns>
        Public Property EnableShadowCopy As Boolean
            Get
                If _enableShadowCopy.HasValue Then
                    Return _enableShadowCopy
                Else
                    Return Not IsReadOnly
                End If
            End Get
            Set(value As Boolean)
                _enableShadowCopy = value
            End Set
        End Property
        Dim _enableShadowCopy As Boolean?

        Public ReadOnly Property IsThreadSafe As Boolean
            Get
                Return InMemoryFile IsNot Nothing
            End Get
        End Property
#End Region

#Region "File Interaction"
        ''' <summary>
        ''' The raw data of the file, if EnableInMemoryLoad is True.
        ''' </summary>
        ''' <returns></returns>
        Private Property InMemoryFile As Byte()

        Protected ReadOnly Property FileReader As Stream
            Get
                If _fileReader Is Nothing Then
                    If IsReadOnly Then
                        _fileReader = FileProvider.OpenFileReadOnly(PhysicalFilename)
                    Else
                        _fileReader = FileProvider.OpenFile(PhysicalFilename)
                    End If
                End If

                Return _fileReader
            End Get
        End Property
        Dim _fileReader As Stream

        ''' <summary>
        ''' Gets or sets the byte at the given index.
        ''' </summary>
        ''' <param name="Index">Index of the byte.</param>
        ''' <returns></returns>
        ''' <remarks>This property is not thread safe.  For a thread-safe equivalent, see <see cref="Read(Long)"/> or <see cref="Write(Long, Byte)"/>.</remarks>
        Public Property RawData(Index As Long) As Byte
            Get
                If InMemoryFile IsNot Nothing Then
                    If InMemoryFile.Length > Index Then
                        Return InMemoryFile(Index)
                    Else
                        Throw New IndexOutOfRangeException("Index " & Index.ToString & " is out of range.  Length of file: " & InMemoryFile.Length.ToString)
                    End If
                Else
                    FileReader.Seek(Index, SeekOrigin.Begin)
                    Dim b = FileReader.ReadByte
                    If b > -1 AndAlso b < 256 Then
                        Return b
                    Else
                        Throw New IndexOutOfRangeException("Index " & Index.ToString & " is out of range.  Length of file: " & FileReader.Length.ToString)
                    End If
                End If
            End Get
            Set(value As Byte)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                If InMemoryFile IsNot Nothing Then
                    InMemoryFile(Index) = value
                Else
                    FileReader.Seek(Index, SeekOrigin.Begin)
                    FileReader.WriteByte(value)
                End If
            End Set
        End Property

        ''' <remarks>This property is not thread safe.  For a thread-safe equivalent, see <see cref="Read(Long, Long)"/> or <see cref="Write(Long, Long, Byte())"/>.</remarks>
        Public Property RawData(Index As Long, Length As Long) As Byte()
            Get
                Dim output(Length - 1) As Byte
                If InMemoryFile IsNot Nothing Then
                    For i = 0 To Length - 1
                        output(i) = RawData(Index + i)
                    Next
                Else
                    FileReader.Seek(Index, SeekOrigin.Begin)
                    FileReader.Read(output, 0, Length)
                End If
                Return output
            End Get
            Set(value As Byte())
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                If InMemoryFile IsNot Nothing Then
                    For i = 0 To Length - 1
                        RawData(Index + i) = value(i)
                    Next
                Else
                    FileReader.Seek(Index, SeekOrigin.Begin)
                    FileReader.Write(value, 0, Length)
                End If
            End Set
        End Property

        ''' <remarks>This property is not thread safe.  For a thread-safe equivalent, see <see cref="Read()"/> or <see cref="Write(Byte())"/>.</remarks>
        Public Property RawData() As Byte()
            Get
                If InMemoryFile IsNot Nothing Then
                    Return InMemoryFile
                Else
                    Return RawData(0, Length)
                End If
            End Get
            Set(value As Byte())
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                If InMemoryFile IsNot Nothing Then
                    InMemoryFile = value
                Else
                    RawData(0, Length) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets a 16 bit signed little endian int starting at the given index.
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Property Int16(Index As Long) As Short
            Get
                Return BitConverter.ToInt16(RawData(Index, 2), 0)
            End Get
            Set(value As Short)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                Dim bytes = BitConverter.GetBytes(value)
                RawData(Index, 2) = bytes
            End Set
        End Property

        ''' <summary>
        ''' Gets a 16 bit unsigned little endian int starting at the given index.
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Property UInt16(Index As Long) As UShort
            Get
                Return BitConverter.ToUInt16(RawData(Index, 2), 0)
            End Get
            Set(value As UShort)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                Dim bytes = BitConverter.GetBytes(value)
                RawData(Index, 2) = bytes
            End Set
        End Property

        ''' <summary>
        ''' Gets a 32 bit signed little endian int starting at the given index.
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Property Int32(Index As Long) As Integer
            Get
                Return BitConverter.ToInt32(RawData(Index, 4), 0)
            End Get
            Set(value As Integer)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                Dim bytes = BitConverter.GetBytes(value)
                RawData(Index, 4) = bytes
            End Set
        End Property

        ''' <summary>
        ''' Gets a 32 bit unsingned little endian int starting at the given index.
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Property UInt32(Index As Long) As UInteger
            Get
                Return BitConverter.ToUInt32(RawData(Index, 4), 0)
            End Get
            Set(value As UInteger)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                Dim bytes = BitConverter.GetBytes(value)
                RawData(Index, 4) = bytes
            End Set
        End Property

        ''' <summary>
        ''' Gets a 64 bit signed little endian int starting at the given index.
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Property Int64(Index As Long) As Long
            Get
                Return BitConverter.ToInt64(RawData(Index, 8), 0)
            End Get
            Set(value As Long)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                Dim bytes = BitConverter.GetBytes(value)
                RawData(Index, 8) = bytes
            End Set
        End Property

        ''' <summary>
        ''' Gets a 64 bit unsingned little endian int starting at the given index.
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        Public Property UInt64(Index As Long) As ULong
            Get
                Return BitConverter.ToUInt64(RawData(Index, 8), 0)
            End Get
            Set(value As ULong)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                Dim bytes = BitConverter.GetBytes(value)
                RawData(Index, 8) = bytes
            End Set
        End Property

        Public Property Length As Long
            Get
                If InMemoryFile IsNot Nothing Then
                    Return InMemoryFile.Length
                Else
                    Return FileReader.Length
                End If
            End Get
            Set(value As Long)
                If IsReadOnly Then
                    Throw New IOException(My.Resources.Language.ErrorWrittenReadonly)
                End If
                If InMemoryFile IsNot Nothing Then
                    Array.Resize(InMemoryFile, value)
                Else
                    FileReader.SetLength(value)
                End If
            End Set
        End Property

        Public Property Position As ULong
#End Region

#End Region

#Region "Events"

        ''' <summary>
        ''' Raised when the file has been saved to disk.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Public Event FileSaved(sender As Object, e As EventArgs) Implements ISavable.FileSaved

        ''' <summary>
        ''' Raised when the file is being saved, but before any changes have been written to disk.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Public Event FileSaving(sender As Object, e As EventArgs)

        Protected Sub RaiseFileSaved(sender As Object, e As EventArgs)
            RaiseEvent FileSaved(sender, e)
        End Sub

#End Region

#Region "Functions"

#Region "IO"
        ''' <summary>
        ''' Creates a new file with the given name.
        ''' </summary>
        ''' <param name="Name">Name (not path) of the file.  Include the extension if applicable.</param>
        Public Sub CreateFile(Name As String) 'Implements iCreatableFile.CreateFile
            CreateFile(Name, {})
        End Sub

        ''' <summary>
        ''' Creates a new file with the given contents.
        ''' </summary>
        ''' <param name="FileContents">Contents of the new file.</param>
        Public Sub CreateFile(FileContents As Byte())
            CreateFile("", FileContents)
        End Sub

        Public Overridable Sub CreateFile(Name As String, FileContents As Byte())
            CreateFileInternal(Name, FileContents, True, FileProvider)
        End Sub

        ''' <summary>
        ''' Creates a new <see cref="GenericFile"/> using the data from the given <paramref name="file"/>.
        ''' </summary>
        ''' <param name="file">File containing the data to create a new file.</param>
        Public Sub CreateFile(file As GenericFile, provider As IIOProvider)
            CreateFileInternal(file.Name, {}, file.EnableInMemoryLoad, provider) 'Initializes properties needed by FileReader, if EnableInMemoryLoad is false
            Me.Length = file.Length

            If file.EnableInMemoryLoad Then
                InMemoryFile = file.InMemoryFile.Clone
            Else
                CopyFrom(file.FileReader, 0, 0, Me.Length)
            End If
        End Sub

        Private Sub CreateFileInternal(name As String, fileContents As Byte(), enableInMemoryLoad As Boolean, provider As IIOProvider)
            'Load the file
            Me.EnableInMemoryLoad = enableInMemoryLoad
            If enableInMemoryLoad Then
                'Set the in-memory file to the given contents
                Me.InMemoryFile = fileContents
            Else
                'Save the file to a temporary filename
                Me.PhysicalFilename = provider.GetTempFilename
                Me.OriginalFilename = Me.PhysicalFilename
                provider.WriteAllBytes(Me.PhysicalFilename, fileContents)
                'The file reader will be initialized when it's first needed
            End If

            Me.Name = name
            Me.FileProvider = provider
        End Sub

        ''' <summary>
        ''' Opens a file from the given filename.  If it does not exists, a blank one will be created.
        ''' </summary>
        ''' <param name="Filename"></param>
        Public Overridable Function OpenFile(filename As String, provider As IIOProvider) As Task Implements IOpenableFile.OpenFile
            Me.FileProvider = provider
            OpenFileInternal(filename)
            Return Task.FromResult(0)
        End Function

        Private Sub OpenFileInternal(filename As String)
            Dim fileSize As Long = FileProvider.GetFileLength(filename)
            If (EnableInMemoryLoad AndAlso FileProvider.CanLoadFileInMemory(fileSize)) Then
                'Load the file into memory if it's enabled and it will fit into RAM, with 500MB left over, just in case.
                Me.OriginalFilename = filename
                Me.PhysicalFilename = filename
                InMemoryFile = FileProvider.ReadAllBytes(filename)
            Else
                'The file will be read from disk.  The only concern is whether or not we want to make a shadow copy.
                If EnableShadowCopy Then
                    Me.OriginalFilename = filename
                    Me.PhysicalFilename = FileProvider.GetTempFilename
                    If FileProvider.FileExists(filename) Then
                        FileProvider.CopyFile(filename, Me.PhysicalFilename)
                    Else
                        'If the file doesn't exist, we'll create a file.
                        FileProvider.WriteAllBytes(Me.PhysicalFilename, {})
                    End If
                Else
                    Me.OriginalFilename = filename
                    Me.PhysicalFilename = filename
                End If
                'The file stream will be initialized when it's needed.
            End If
        End Sub

        ''' <summary>
        ''' Saves the file to the given destination.
        ''' </summary>
        ''' <param name="filename">Full path of where the file should be saved to.</param>
        Public Overridable Function Save(filename As String, provider As IIOProvider) As Task Implements ISavableAs.Save
            RaiseEvent FileSaving(Me, New EventArgs)
            If InMemoryFile IsNot Nothing Then
                provider.WriteAllBytes(filename, InMemoryFile)
            Else
                FileReader.Seek(0, SeekOrigin.Begin)
                FileReader.Flush()
                If Not String.IsNullOrEmpty(filename) Then
                    Using dest = provider.OpenFileWriteOnly(filename)
                        FileReader.CopyTo(dest)
                    End Using
                End If
            End If

            If String.IsNullOrEmpty(OriginalFilename) Then
                OriginalFilename = filename
            End If
            RaiseEvent FileSaved(Me, New EventArgs)
            Return Task.FromResult(0)
        End Function

        ''' <summary>
        ''' Saves the file to the Original Filename.
        ''' Throws a NullReferernceException if the Original Filename is null.
        ''' </summary>
        Public Async Function Save(provider As IIOProvider) As Task Implements ISavable.Save
            If String.IsNullOrEmpty(Me.OriginalFilename) Then
                Throw New NullReferenceException(My.Resources.Language.ErrorNoSaveFilename)
            End If
            Await Save(Me.OriginalFilename, provider)
        End Function
#End Region

#Region "Data Interaction"

        ''' <summary>
        ''' Reads all the data in the file.
        ''' </summary>
        ''' <returns>An array of byte containing the contents of the file.</returns>
        ''' <remarks>Not recommended for larger files.  This function is thread-safe.</remarks>
        Public Async Function Read() As Task(Of Byte())
            If IsThreadSafe Then
                Return RawData
            Else
                Return Await Task.Run(Function() As Byte()
                                          SyncLock _fileLock
                                              Return RawData
                                          End SyncLock
                                      End Function)
            End If
        End Function

        ''' <summary>
        ''' Reads a byte from the file.
        ''' </summary>
        ''' <param name="index">Index from which to retrieve the byte.</param>
        ''' <returns>A byte equal to the byte at the given index in the file.</returns>
        ''' <remarks>This function is thread-safe.</remarks>
        Public Async Function Read(index As Long) As Task(Of Byte)
            If IsThreadSafe Then
                Return RawData(index)
            Else
                Return Await Task.Run(Function() As Byte
                                          SyncLock _fileLock
                                              Return RawData(index)
                                          End SyncLock
                                      End Function)
            End If
        End Function

        ''' <summary>
        ''' Reads a range of bytes from the file.
        ''' </summary>
        ''' <param name="index">Index from which to retrieve the range.</param>
        ''' <param name="length">Length of the range.</param>
        ''' <returns>An array of byte containing the data in the requested range.</returns>
        ''' <remarks>This function is thread-safe.</remarks>
        Public Async Function Read(index As Long, length As Long) As Task(Of Byte())
            If IsThreadSafe Then
                Return RawData(index, length)
            Else
                Return Await Task.Run(Function() As Byte()
                                          SyncLock _fileLock
                                              Return RawData(index, length)
                                          End SyncLock
                                      End Function)
            End If
        End Function

        ''' <summary>
        ''' Writes a range of bytes to the file.
        ''' </summary>
        ''' <param name="index">Index in the file to write the <paramref name="data"/>.</param>
        ''' <param name="length">Length of the data, in bytes, to copy.</param>
        ''' <param name="data">Array of bytes to write.</param>
        ''' ''' <remarks>This function is thread-safe.</remarks>
        Public Async Function Write(index As Long, length As Long, data As Byte()) As Task
            If IsThreadSafe Then
                RawData(index, length) = data
            Else
                Await Task.Run(Sub()
                                   SyncLock _fileLock
                                       RawData(index, length) = data
                                   End SyncLock
                               End Sub)
            End If
        End Function

        ''' <summary>
        ''' Writes a single to the file.
        ''' </summary>
        ''' <param name="index">Index in the file to write the <paramref name="data"/>.</param>
        ''' <param name="data">Array of bytes to write.</param>
        ''' <remarks>This function is thread-safe.</remarks>
        Public Async Function Write(index As Long, data As Byte) As Task
            If IsThreadSafe Then
                RawData(index) = data
            Else
                Await Task.Run(Sub()
                                   SyncLock _fileLock
                                       RawData(index) = data
                                   End SyncLock
                               End Sub)
            End If
        End Function

        ''' <summary>
        ''' Replaces the contents of the file with the given data.
        ''' </summary>
        ''' <param name="data">Array of bytes to write.</param>
        ''' <remarks>This function is thread-safe.</remarks>
        Public Async Function Write(data As Byte()) As Task
            If IsThreadSafe Then
                RawData() = data
            Else
                Await Task.Run(Sub()
                                   SyncLock _fileLock
                                       RawData() = data
                                   End SyncLock
                               End Sub)
            End If
        End Function

        ''' <summary>
        ''' Copies data into the given stream.
        ''' </summary>
        ''' <param name="destination">Stream to which to copy data.</param>
        ''' <param name="index">Index of the data to start reading from the <see cref="GenericFile"/>.</param>
        ''' <param name="length">Number of bytes to copy into the destination stream.</param>
        ''' <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null.</exception>
        ''' <remarks>Currently, the data of size <paramref name="length"/> is buffered in memory, and will error if there is insufficient memory.
        ''' 
        ''' To avoid threading issues, this function will synchronously block using SyncLock until the operation is complete.</remarks>
        Public Sub CopyTo(destination As Stream, index As Long, length As Long)
            If IsThreadSafe Then
                CopyToInternal(destination, index, length)
            Else
                SyncLock _fileLock
                    CopyToInternal(destination, index, length)
                End SyncLock
            End If
        End Sub

        Private Sub CopyToInternal(destination As Stream, index As Long, length As Long)
            If destination Is Nothing Then
                Throw New ArgumentNullException(NameOf(destination))
            End If

            If InMemoryFile IsNot Nothing Then
                destination.Write(InMemoryFile, index, length)
            Else
                Dim buffer(length) As Byte
                FileReader.Seek(index, SeekOrigin.Begin)
                FileReader.Read(buffer, 0, length)
                destination.Write(buffer, 0, length)
            End If
        End Sub

        ''' <summary>
        ''' Reads data from the given stream.
        ''' </summary>
        ''' <param name="source">Stream from which to read data.</param>
        ''' <param name="sourceIndex">Index of the stream to read data.</param>
        ''' <param name="fileIndex">Index of the file where data should be written.</param>
        ''' <param name="length">Length in bytes of the data to write.</param>
        Public Sub CopyFrom(source As Stream, sourceIndex As Long, fileIndex As Long, length As Long)
            If IsThreadSafe Then
                CopyFromInternal(source, sourceIndex, fileIndex, length)
            Else
                SyncLock _fileLock
                    CopyFromInternal(source, sourceIndex, fileIndex, length)
                End SyncLock
            End If
        End Sub

        Private Sub CopyFromInternal(source As Stream, sourceIndex As Long, fileIndex As Long, length As Long)
            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If

            If InMemoryFile IsNot Nothing Then
                Dim buffer(length) As Byte
                source.Seek(sourceIndex, SeekOrigin.Begin)
                source.Read(buffer, 0, length)
                InMemoryFile = buffer
            Else
                Dim buffer(length) As Byte
                source.Seek(sourceIndex, SeekOrigin.Begin)
                source.Read(buffer, 0, length)
                FileReader.Seek(fileIndex, SeekOrigin.Begin)
                FileReader.Write(buffer, 0, length)
            End If
        End Sub

        ''' <summary>
        ''' Reads a UTF-16 string from the file.
        ''' </summary>
        ''' <param name="Offset">Location of the string in the file.</param>
        ''' <param name="Length">Length, in characters, of the string.</param>
        ''' <returns></returns>
        Public Function ReadUnicodeString(Offset As Integer, Length As Integer) As String
            Dim u = Text.Encoding.Unicode
            Return u.GetString(RawData(Offset, Length * 2), 0, Length)
        End Function

        ''' <summary>
        ''' Reads a null-terminated UTF-16 string from the file.
        ''' </summary>
        ''' <param name="Offset">Location of the string in the file.</param>
        ''' <returns></returns>
        Public Function ReadUnicodeString(Offset As Integer) As String
            'Parse the null-terminated UTF-16 string
            Dim s As New StringBuilder
            Dim e = Text.Encoding.Unicode
            Dim j As Integer = 0
            Dim cRaw As Byte()
            Dim c As String
            Do
                cRaw = RawData(Offset + j * 2, 2)
                c = e.GetString(cRaw, 0, 2)

                If Not c = vbNullChar Then
                    s.Append(c)
                End If

                j += 1
            Loop Until c = vbNullChar
            Return s.ToString
        End Function

        ''' <summary>
        ''' Reads a null-terminated string from the file using the given encoding.
        ''' Currently only supports static 1 and 2 byte character formats.
        ''' </summary>
        ''' <param name="Offset">Location of the string in the file.</param>
        ''' <param name="e">Character encoding to use.  Currently only supports static 1 and 2 byte character formats (like ASCII and UTF-16, but not UTF-8 or UTF-32).</param>
        ''' <returns></returns>
        Public Function ReadNullTerminatedString(Offset As Integer, e As Text.Encoding) As String
            If e Is Text.Encoding.Unicode Then
                Dim out As New Text.StringBuilder
                Dim pos = Offset
                Dim c As Char
                Do
                    c = e.GetString(RawData(pos, 2), 0, 2)
                    If Not c = vbNullChar Then
                        out.Append(c)
                    End If
                    pos += 2
                Loop Until c = vbNullChar
                Return out.ToString
            Else
                Dim out As New Text.StringBuilder
                Dim pos = Offset
                Dim c As Byte
                Do
                    c = RawData(pos)
                    If Not c = 0 Then
                        out.Append(e.GetString({c}, 0, 1))
                    End If
                    pos += 1
                Loop Until c = 0
                Return out.ToString
            End If
        End Function

        ''' <summary>
        ''' Reads an unsigned 16 bit integer at the current position, then increments the current position by 2.
        ''' </summary>
        ''' <returns></returns>
        Public Function NextUInt16() As UInt16
            Dim out = UInt16(Position)
            Position += 2
            Return out
        End Function
#End Region

        ''' <summary>
        ''' Default file extension for this kind of file.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function GetDefaultExtension() As String Implements ISavableAs.GetDefaultExtension
            Return Nothing
        End Function

        Public Overridable Function GetSupportedExtensions() As IEnumerable(Of String) Implements ISavableAs.GetSupportedExtensions
            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return Me.Name
        End Function

#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If _fileReader IsNot Nothing Then
                        _fileReader.Dispose()
                    End If
                    If EnableShadowCopy Then
                        If FileProvider IsNot Nothing AndAlso
                            Not String.IsNullOrEmpty(Me.PhysicalFilename) AndAlso
                            FileProvider.FileExists(Me.PhysicalFilename) AndAlso
                            Me.OriginalFilename <> Me.PhysicalFilename Then

                            FileProvider.DeleteFile(Me.PhysicalFilename)
                        End If
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class
End Namespace
