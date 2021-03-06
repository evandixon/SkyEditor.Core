﻿using SkyEditor.Core.IO.PluginInfrastructure;
using SkyEditor.Core.Utilities;
using SkyEditor.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    [Obsolete("Use SkyEditor.IO.Binary.BinaryFile where possible.")]
    public class GenericFile : INamed, ICreatableFile, IOpenableFile, IOnDisk, IBinaryDataAccessor, ISavableAs, IDisposable
    {

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="GenericFile"/>
        /// </summary>
        public GenericFile()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="GenericFile"/> using the data at the given file
        /// </summary>
        public GenericFile(string filename, IFileSystem provider)
        {
            OpenFileInternal(filename, provider);
        }

        /// <summary>
        /// Creates a new, read-only instance of <see cref="GenericFile"/> using the given file as the backing source
        /// </summary>
        /// <param name="file"></param>
        public GenericFile(GenericFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            IsReadOnly = true; // Read-only to avoid modifying the other file
            if (file._inMemoryFile != null)
            {
                this._inMemoryFile = file._inMemoryFile;
            }
            else
            {
                this.FileReader = file.FileReader;
                DisableDispose = true; // We don't want to dispose a file stream that doesn't belong to us
            }
        }

        /// <summary>
        /// Creates a new, read-only instance of <see cref="GenericFile"/> using the given data as the backing source
        /// </summary>
        /// <param name="file"></param>
        public GenericFile(byte[] data)
        {
            IsReadOnly = true; // Read-only to avoid modifying the other file
            this._inMemoryFile = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Creates a new, read-only instance of <see cref="GenericFile"/> using the given stream as the backing source
        /// </summary>
        /// <param name="file"></param>
        public GenericFile(Stream stream)
        {
            IsReadOnly = true; // Read-only to avoid modifying the other file
            this.FileReader = stream ?? throw new ArgumentNullException(nameof(stream));
            DisableDispose = true; // We don't want to dispose a file stream that doesn't belong to us
        }

        #endregion
        
        #region Events
        /// <summary>
        /// Raised when the file is being saved
        /// </summary>
        public event EventHandler FileSaving;

        /// <summary>
        /// Raised when the file has been saved
        /// </summary>
        public event EventHandler FileSaved;
        #endregion

        #region Properties

        /// <summary>
        /// Platform dependant abstraction layer for the file system.
        /// </summary>
        protected IFileSystem CurrentFileSystem { get; set; }

        /// <summary>
        /// A memory mapped file object representing the loaded file. This is the preferred way to load data.
        /// </summary>
        private MemoryMappedFile MemoryMappedFile { get; set; }

        private string MemoryMappedFilename { get; set; }

        private IMemoryMappedFileSystem MemoryMappedProvider { get; set; }

        /// <summary>
        /// The raw data of the file, if the file has been loaded in memory.  Null if <see cref="FileReader"/> is in use.
        /// </summary>
        private byte[] _inMemoryFile;

        /// <summary>
        /// The underlying stream used to read and write to the file.  Null if <see cref="InMemoryFile"/> is in use.
        /// </summary>
        private Stream FileReader
        {
            get
            {
                if (_inMemoryFile != null || MemoryMappedFile != null)
                {
                    return null;
                }
                else
                {
                    if (_fileReader == null)
                    {
                        if (IsReadOnly)
                        {
                            _fileReader = CurrentFileSystem.OpenFileReadOnly(PhysicalFilename);
                        }
                        else
                        {
                            _fileReader = CurrentFileSystem.OpenFile(PhysicalFilename);
                        }
                    }
                    return _fileReader;
                }
            }
            set
            {
                _fileReader = value;
            }
        }
        private Stream _fileReader;

        private bool DisableDispose { get; set; }

        /// <summary>
        /// Used to ensure thread safety with <see cref="FileReader"/>
        /// </summary>
        private object _fileAccessLock = new object();

        /// <summary>
        /// The length of the file.
        /// </summary>
        public long Length
        {
            get
            {
                if (MemoryMappedFile != null)
                {
                    return MemoryMappedFile.CreateViewAccessor().Capacity;
                }
                else if (_inMemoryFile != null)
                {
                    return _inMemoryFile.Length;
                }
                else
                {
                    return FileReader.Length;
                }
            }
        }

        /// <summary>
        /// Sets the length of the file. This function is not thread safe.
        /// </summary>
        /// <param name="value">New length of the file</param>
        /// <exception cref="IOException">Thrown if the value is changed when <see cref="IsReadOnly"/> is true, or if the file is not loaded in memory and the length set fails.</exception>
        /// <exception cref="OverflowException">Thrown if the value is greater than <see cref="int.MaxValue"/> when the file is loaded in memory.</exception>

        public void SetLength(long value)
        {
            if (IsReadOnly)
            {
                throw new IOException(Properties.Resources.IO_ErrorReadOnly);
            }

            if (MemoryMappedFile != null)
            {
                // We can't change the size of memory mapped files directly.
                // What we can do is abandon our current file, resize it with a stream, then reopen it.
                // So... (takes deep breath)... here we go

                MemoryMappedFile.Dispose();
                using (var tempFilestream = MemoryMappedProvider.OpenFile(MemoryMappedFilename))
                {
                    tempFilestream.SetLength(value);
                }
                MemoryMappedFile = MemoryMappedProvider.OpenMemoryMappedFile(MemoryMappedFilename);
            }
            else if (_inMemoryFile != null)
            {
                if (value > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                else
                {
                    Array.Resize(ref _inMemoryFile, (int)value);
                }
            }
            else
            {
                FileReader.SetLength(value);
            }
        }

        /// <summary>
        /// The position of operations where index is not specified
        /// </summary>
        public long Position { get; set; }

        #region File Loading and Management

        /// <summary>
        /// The logical location of the file.  Null if the file has never been saved.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The location of the physical file.  Null if the file has been loaded into memory.
        /// </summary>
        public string PhysicalFilename { get; set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    return Path.GetFileName(Filename);
                }
                else
                {
                    return _name;
                }
            }
            set
            {
                _name = value;
            }
        }
        private string _name;

        /// <summary>
        /// Whether or not altering the file is allowed.
        /// </summary>
        /// <remarks>Changes to this property only take effect before the file is opened or created</remarks>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Whether or not to attempt to use a memory mapped file to load data. This is an experimental feature.
        /// </summary>
        public bool EnableMemoryMappedFileLoading { get; set; } = false;

        /// <summary>
        /// Whether or not to make a shadow copy of the file before loading it.
        /// </summary>
        public bool EnableShadowCopy
        {
            get
            {
                if (_enableShadowCopy.HasValue)
                {
                    return _enableShadowCopy.Value;
                }
                else
                {
                    return !IsReadOnly;
                }
            }
            set
            {
                _enableShadowCopy = value;
            }
        }
        private bool? _enableShadowCopy;

        /// <summary>
        /// Whether or not opening a file should attempt to load the file into memory
        /// </summary>
        public bool EnableInMemoryLoad
        {
            get
            {
                return _enableInMemoryLoad ?? PhysicalFilename == null;
            }
            set
            {
                _enableInMemoryLoad = value;
            }
        }
        private bool? _enableInMemoryLoad;

        /// <summary>
        /// Whether or not it is safe for function calls not marked as thread-safe to be called at the same time by different threads.
        /// </summary>
        public bool IsThreadSafe
        {
            get
            {
                return MemoryMappedFile != null || _inMemoryFile != null;
            }
        }
        #endregion

        #endregion

        #region Functions

        #region I/O

        /// <summary>
        /// Initializes the class to model a new, empty file
        /// </summary>
        /// <param name="name">Name of the new file</param>
        public void CreateFile(string name)
        {
            CreateFile("", Array.Empty<byte>());
        }

        /// <summary>
        /// Initializes the class to model a file with the given contents
        /// </summary>
        /// <param name="contents">Raw data of the new file</param>
        public void CreateFile(byte[] contents)
        {
            CreateFile("", contents);
        }

        /// <summary>
        /// Initializes the class to model a file with the given contents
        /// </summary>
        /// <param name="name">Name of the new file</param>
        /// <param name="contents">Raw data of the new file</param>
        public virtual void CreateFile(string name, byte[] contents)
        {
            CreateFileInternal(name, contents, true, null);
        }

        /// <summary>
        /// Initializes the class to model a copy of the given file
        /// </summary>
        /// <param name="otherFile">File to copy</param>
        public void CreateFile(GenericFile otherFile)
        {
            CreateFileInternal(otherFile.Name, Array.Empty<byte>(), otherFile.EnableInMemoryLoad, otherFile.CurrentFileSystem);
            SetLength(otherFile.Length);

            if (otherFile.MemoryMappedFile != null)
            {
                // We don't want to change the source file
                // To-do: figure out how to use a temp file to store the memory mapped data
                var otherViewAccessor = otherFile.MemoryMappedFile.CreateViewAccessor();
                if (otherViewAccessor.Capacity < int.MaxValue)
                {
                    this._inMemoryFile = otherFile.Read();
                }
                else
                {
                    var otherReader = otherFile.MemoryMappedFile.CreateViewStream();
                    otherReader.Seek(0, SeekOrigin.Begin);
                    FileReader.Seek(0, SeekOrigin.Begin);
                    otherReader.CopyTo(FileReader);
                }
            }
            else if (otherFile._inMemoryFile != null)
            {
                this._inMemoryFile = otherFile._inMemoryFile.Clone() as byte[];
            }
            else
            {
                otherFile.FileReader.Seek(0, SeekOrigin.Begin);
                FileReader.Seek(0, SeekOrigin.Begin);
                otherFile.FileReader.CopyTo(FileReader);
            }
        }

        /// <summary>
        /// Initializes the class to model a file with the given contents
        /// </summary>
        /// <param name="name">Name of the new file</param>
        /// <param name="contents">Contents of the new file</param>
        /// <param name="enableInMemoryLoad">Whether or not to store the contents in memory</param>
        /// <param name="provider">Instance of the current I/O provider.  Not required if <paramref name="enableInMemoryLoad"/> is true.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null when <paramref name="enableInMemoryLoad"/> is false</exception>
        private void CreateFileInternal(string name, byte[] contents, bool enableInMemoryLoad, IFileSystem provider)
        {
            this.Name = name;
            this.EnableInMemoryLoad = enableInMemoryLoad;

            if (!enableInMemoryLoad && provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (provider != null)
            {
                this.CurrentFileSystem = provider;
            }
            
            // Don't try creating a memory mapped file since we're starting with an array

            if (enableInMemoryLoad)
            {
                _inMemoryFile = contents;
            }
            else
            {
                // Create a temporary file
                this.PhysicalFilename = provider.GetTempFilename();
                this.Filename = this.PhysicalFilename;
                provider.WriteAllBytes(PhysicalFilename, contents);
                // The file reader will be initialized when it's first requested
            }
        }

        /// <summary>
        /// Initializes the class to represent the data stored in the file at the given path
        /// </summary>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">Instance of the I/O provider that stores the file</param>
        public virtual Task OpenFile(string filename, IFileSystem provider)
        {
            OpenFileInternal(filename, provider);
            return Task.CompletedTask;
        }

        public virtual Task OpenFile(GenericFile file)
        {
            OpenFileInternal(file);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the class to represent the data stored in the file at the given path
        /// </summary>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">Instance of the I/O provider that stores the file</param>
        private void OpenFileInternal(string filename, IFileSystem provider)
        {
            if (EnableMemoryMappedFileLoading && provider is IMemoryMappedFileSystem memoryMappedProvider)
            {
                this.MemoryMappedFile = memoryMappedProvider.OpenMemoryMappedFile(filename);                
                try
                {
                    // Sometimes we might not have enough memory.
                    // When that happens, we get an IOException saying something "There are not enough memory resources available"
                    if (MemoryMappedFile.CreateViewAccessor().Capacity > -1) // Compare the capacity to -1 just to see if we get an IOException
                    {
                        this.MemoryMappedFilename = filename;
                        this.MemoryMappedProvider = memoryMappedProvider;
                        this.Filename = filename;
                        return;
                    }
                }
                catch (IOException)
                {
                    // We can't use a MemoryMapped file.
                    this.MemoryMappedFile = null;
                }
            }

            if (EnableInMemoryLoad)
            {
                byte[] contents;

                // Try to load file in memory
                try
                {                    
                    contents = provider.ReadAllBytes(filename);
                }
                catch (IOException)
                {
                    if (provider is PhysicalFileSystem)
                    {
                        if (File.Exists(filename) && (new FileInfo(filename)).Length > int.MaxValue)
                        {
                            // File is too large.  Use stream instead.
                            OpenFileInternalStream(filename, provider);
                            return;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        // Got an I/O exception and we're not sure if we're using the PhysicalFileSystem or not
                        // Let's assume it's because of the 2GB limit and use a stream
                        OpenFileInternalStream(filename, provider);
                        return;
                    }
                }
                catch (OutOfMemoryException)
                {
                    // File is too large.  Use stream instead.
                    OpenFileInternalStream(filename, provider);
                    return;
                }

                // Load succeeded
                _inMemoryFile = contents;
                this.Filename = filename;
                this.PhysicalFilename = filename;
                this.CurrentFileSystem = provider;
            }
            else
            {
                OpenFileInternalStream(filename, provider);
            }
        }

        private void OpenFileInternal(GenericFile file)
        {
            this.Name = file.Name;
            if (IsReadOnly)
            {
                // Since we're not going to change anything,
                // It's safe to share the same references as the other file
                if (file.MemoryMappedFile != null)
                {
                    this.MemoryMappedFile = file.MemoryMappedFile;
                    this.MemoryMappedFilename = file.MemoryMappedFilename;
                    this.MemoryMappedProvider = file.MemoryMappedProvider;
                }
                if (file._inMemoryFile != null)
                {
                    this._inMemoryFile = file._inMemoryFile;
                }
                else
                {
                    this.FileReader = file.FileReader;
                    DisableDispose = true; // We don't want to dispose a file stream that doesn't belong to us
                }
            }
            else
            {
                this._inMemoryFile = file.Read().Clone() as byte[];
            }
        }

        /// <summary>
        /// Initializes the class to represent the data stored in the file at the given path, without loading the file into memory
        /// </summary>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">Instance of the I/O provider that stores the file</param>
        private void OpenFileInternalStream(string filename, IFileSystem provider)
        {
            this.CurrentFileSystem = provider;
            this.Filename = filename;
            if (EnableShadowCopy)
            {
                this.PhysicalFilename = provider.GetTempFilename();
                if (provider.FileExists(filename))
                {
                    provider.CopyFile(filename, this.PhysicalFilename);
                }
                else
                {
                    // Create a file if the source doesn't exist
                    provider.WriteAllBytes(this.PhysicalFilename, Array.Empty<byte>());
                }
            }
            else
            {
                this.PhysicalFilename = filename;
            }
            // The stream will be initialized when needed
        }

        /// <summary>
        /// Saves the file to the given path
        /// </summary>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">Instance of the I/O provider that will the file</param>
        public virtual async Task Save(string filename, IFileSystem provider)
        {
            FileSaving?.Invoke(this, new EventArgs());
            if (MemoryMappedFile != null)
            {
                if (MemoryMappedFilename == filename)
                {
                    // Trying to save to the current file
                    // To-do: determine if the file flushes automatically
                }
                else
                {
                    var memoryMappedStream = MemoryMappedFile.CreateViewStream();
                    if (!string.IsNullOrEmpty(filename) && filename != PhysicalFilename)
                    {
                        using (var dest = provider.OpenFileWriteOnly(filename))
                        {
                            await memoryMappedStream.CopyToAsync(dest);
                        }
                    }
                }
            }
            else if (_inMemoryFile != null)
            {
                provider.WriteAllBytes(filename, _inMemoryFile);
            }
            else if (FileReader != null)
            {
                FileReader.Seek(0, SeekOrigin.Begin);
                FileReader.Flush();
                if (!string.IsNullOrEmpty(filename) && filename != PhysicalFilename)
                {
                    using (var dest = provider.OpenFileWriteOnly(filename))
                    {
                        await FileReader.CopyToAsync(dest);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }

            this.Filename = filename;
            FileSaved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Saves the file to the current path
        /// </summary>
        /// <param name="provider">Instance of the I/O provider that will the file</param>
        /// <exception cref="NullReferenceException">Thrown if <see cref="Filename"/> is null.</exception>
        public async Task Save(IFileSystem provider)
        {
            if (string.IsNullOrEmpty(this.Filename))
            {
                throw new NullReferenceException(Properties.Resources.IO_GenericFile_ErrorNoSaveFilename);
            }
            await Save(this.Filename, provider);
        }

        #endregion

        #region Data Interaction

        #region Low-level Read/Write

        /// <summary>
        /// Reads all the data in the file.  Not thread-safe.
        /// </summary>
        /// <returns>An array of byte containing the contents of the file.</returns>
        private byte[] ReadInternal()
        {
            if (MemoryMappedFile != null)
            {
                var buffer = new byte[Length];
                MemoryMappedFile.CreateViewAccessor().ReadArray(0, buffer, 0, buffer.Length);
                return buffer;
            }
            else if (_inMemoryFile != null)
            {
                return _inMemoryFile;
            }
            else
            {
                if (Length > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                return Read(0, (int)Length);
            }
        }

        /// <summary>
        /// Reads all the data in the file.  Thread-safe.
        /// </summary>
        /// <returns>An array of byte containing the contents of the file.</returns>
        public byte[] Read()
        {
            if (IsThreadSafe)
            {
                return ReadInternal();
            }
            else
            {
                lock (_fileAccessLock)
                {
                    return ReadInternal();
                }
            }
        }

        /// <summary>
        /// Reads all the data in the file.  Thread-safe.
        /// </summary>
        /// <returns>An array of byte containing the contents of the file.</returns>
        public async Task<byte[]> ReadAsync()
        {
            if (IsThreadSafe)
            {
                return ReadInternal();
            }
            else
            {
                return await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        return ReadInternal();
                    }
                });
            }
        }

        /// <summary>
        /// Reads a byte from the file.  Not thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <returns>A byte equal to the byte at the given index in the file.</returns>
        private byte ReadInternal(long index)
        {
            if (MemoryMappedFile != null)
            {
                return MemoryMappedFile.CreateViewAccessor(index, 1).ReadByte(0);
            }
            else if (_inMemoryFile != null)
            {
                if (_inMemoryFile.Length > index)
                {
                    return _inMemoryFile[index];
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format(Properties.Resources.IO_GenericFile_OutOfRange, index, _inMemoryFile.Length));
                }
            }
            else if (FileReader != null)
            {
                FileReader.Seek(index, SeekOrigin.Begin);
                var b = FileReader.ReadByte();
                if (b > -1 && b < 256)
                {
                    return (byte)b;
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format(Properties.Resources.IO_GenericFile_OutOfRange, index, FileReader.Length));
                }
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }
        }

        /// <summary>
        /// Reads a byte from the file.  Thread-safe
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <returns>A byte equal to the byte at the given index in the file.</returns>
        public byte Read(long index)
        {
            if (IsThreadSafe)
            {
                return ReadInternal(index);
            }
            else
            {
                lock (_fileAccessLock)
                {
                    return ReadInternal(index);
                }
            }
        }

        /// <summary>
        /// Reads a byte from the file.  Thread-safe
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <returns>A byte equal to the byte at the given index in the file.</returns>
        public async Task<byte> ReadAsync(long index)
        {
            if (IsThreadSafe)
            {
                return ReadInternal(index);
            }
            else
            {
                return await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        return ReadInternal(index);
                    }
                });
            }
        }

        /// <summary>
        /// Reads a range of bytes from the file.  Not thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="length">Lengt of the range to read.</param>
        /// <returns>A byte equal to the byte at the given index in the file.</returns>
        private byte[] ReadInternal(long index, int length)
        {
            if (MemoryMappedFile != null)
            {
                var buffer = new byte[length];
                MemoryMappedFile.CreateViewAccessor(index, length).ReadArray(0, buffer, 0, buffer.Length);
                return buffer;
            }
            else if (_inMemoryFile != null)
            {
                if (index > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                var buffer = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = _inMemoryFile[index + i];
                }
                return buffer;
            }
            else if (FileReader != null)
            {
                byte[] buffer = new byte[length];
                FileReader.Seek(index, SeekOrigin.Begin);
                FileReader.Read(buffer, 0, length);
                return buffer;
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }
        }

        /// <summary>
        /// Reads a range of bytes from the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="length">Lengt of the range to read.</param>
        /// <returns>A byte equal to the byte at the given index in the file.</returns>
        public byte[] Read(long index, int length)
        {
            if (IsThreadSafe)
            {
                return ReadInternal(index, length);
            }
            else
            {          
                lock (_fileAccessLock)
                {
                    return ReadInternal(index, length);
                }
            }
        }

        /// <summary>
        /// Reads a range of bytes from the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="length">Lengt of the range to read.</param>
        /// <returns>A byte equal to the byte at the given index in the file.</returns>
        public async Task<byte[]> ReadAsync(long index, int length)
        {
            if (IsThreadSafe)
            {
                return ReadInternal(index, length);
            }
            else
            {
                return await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        return ReadInternal(index, length);
                    }
                });
            }
        }

        /// <summary>
        /// Writes over all the data in the file.  Not thread-safe.
        /// </summary>
        private void WriteInternal(byte[] value)
        {
            if (IsReadOnly)
            {
                throw new IOException(Properties.Resources.IO_ErrorReadOnly);
            }

            if (MemoryMappedFile != null)
            {
                MemoryMappedFile.CreateViewAccessor().WriteArray(0, value, 0, value.Length);
            }
            else if (_inMemoryFile != null)
            {
                _inMemoryFile = value;
            }
            else if (FileReader != null)
            {
                if (Length > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                FileReader.Seek(0, SeekOrigin.Begin);
                FileReader.Write(value, 0, (int)Length);
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }
        }

        /// <summary>
        /// Writes over all the data in the file.  Not thread-safe.
        /// </summary>
        /// <returns>An array of byte containing the contents of the file.</returns>
        public void Write(byte[] value)
        {
            if (IsThreadSafe)
            {
                WriteInternal(value);
            }
            else
            {                
                lock (_fileAccessLock)
                {
                    WriteInternal(value);
                }
            }
        }

        /// <summary>
        /// Writes over all the data in the file.  Not thread-safe.
        /// </summary>
        /// <returns>An array of byte containing the contents of the file.</returns>
        public async Task WriteAsync(byte[] value)
        {
            if (IsThreadSafe)
            {
                WriteInternal(value);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        WriteInternal(value);
                    }
                });
            }
        }

        /// <summary>
        /// Writes a byte to file.  Not thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="value">The data to write</param>
        private void WriteInternal(long index, byte value)
        {
            if (IsReadOnly)
            {
                throw new IOException(Properties.Resources.IO_ErrorReadOnly);
            }

            if (MemoryMappedFile != null)
            {
                MemoryMappedFile.CreateViewAccessor().Write(0, value);
            }
            else if (_inMemoryFile != null)
            {
                _inMemoryFile[index] = value;
            }
            else if (FileReader != null)
            {
                FileReader.Seek(index, SeekOrigin.Begin);
                FileReader.WriteByte(value);
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }
        }

        /// <summary>
        /// Writes a byte to the file.  Thread-safe
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="value">The data to write</param>
        public void Write(long index, byte value)
        {
            if (IsThreadSafe)
            {
                WriteInternal(index, value);
            }
            else
            {
                lock (_fileAccessLock)
                {
                    WriteInternal(index, value);
                }
            }
        }

        /// <summary>
        /// Writes a byte to the file.  Thread-safe
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="value">The data to write</param>
        public async Task WriteAsync(long index, byte value)
        {
            if (IsThreadSafe)
            {
                WriteInternal(index, value);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        WriteInternal(index, value);
                    }
                });
            }
        }

        /// <summary>
        /// Writes a range of bytes to the file.  Not thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="length">Length of the range to read.</param>
        /// <param name="value">The data to write</param>
        private void WriteInternal(long index, int length, byte[] value)
        {
            if (IsReadOnly)
            {
                throw new IOException(Properties.Resources.IO_ErrorReadOnly);
            }

            if (MemoryMappedFile != null)
            {
                MemoryMappedFile.CreateViewAccessor(index, length).WriteArray(0, value, 0, length);
            }
            else if (_inMemoryFile != null)
            {
                if (index > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                Array.Copy(value, 0, _inMemoryFile, index, length);
            }
            else if (FileReader != null)
            {
                FileReader.Seek(index, SeekOrigin.Begin);
                FileReader.Write(value, 0, length);
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }
        }

        /// <summary>
        /// Writes a range of bytes to the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="length">Length of the range to read.</param>
        /// <param name="value">The data to write</param>
        public void Write(long index, int length, byte[] value)
        {
            if (IsThreadSafe)
            {
                WriteInternal(index, length, value);
            }
            else
            {
                lock (_fileAccessLock)
                {
                    WriteInternal(index, length, value);
                }
            }
        }

        /// <summary>
        /// Writes a range of bytes to the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="length">Length of the range to read.</param>
        /// <param name="value">The data to write</param>
        public async Task WriteAsync(long index, int length, byte[] value)
        {
            if (IsThreadSafe)
            {
                WriteInternal(index, length, value);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        WriteInternal(index, length, value);
                    }
                });
            }
        }

        /// <summary>
        /// Reads an unsigned byte from the current position (<see cref="Position"/>), then advances the current position.  This function is thread-safe.
        /// </summary>
        /// <returns>The integer from the given location</returns>
        public byte ReadByte()
        {
            var output = Read(Position);
            Position += 1;
            return output;
        }

        /// <summary>
        /// Reads an unsigned byte from the current position (<see cref="Position"/>), then advances the current position.  This function is thread-safe.
        /// </summary>
        /// <returns>The integer from the given location</returns>
        public async Task<byte> ReadByteAsync()
        {
            var output = await ReadAsync(Position);
            Position += 1;
            return output;
        }

        /// <summary>
        /// Writes a byte to the current position (<see cref="Position"/>), then advances the current position.  This function is thread-safe.
        /// </summary>
        public void WriteByte(byte value)
        {
            Write(Position, value);
            Position += 1;
        }

        /// <summary>
        /// Writes a byte to the current position (<see cref="Position"/>), then advances the current position.  This function is thread-safe.
        /// </summary>
        public async Task WriteByteAsync(byte value)
        {
            await WriteAsync(Position, value);
            Position += 1;
        }

        /// <summary>
        /// Copies data into the given stream.  Not thread-safe.
        /// </summary>
        /// <param name="destination">Stream to which to copy data.</param>
        /// <param name="index">Index of the data to start reading from the <see cref="GenericFile"/>.</param>
        /// <param name="length">Number of bytes to copy into the destination stream.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null.</exception>
        /// <remarks>Currently, the data of size <paramref name="length"/> is buffered in memory, and will error if there is insufficient memory.
        ///
        /// To avoid threading issues, this function will synchronously block using SyncLock until the operation is complete.</remarks>
        private void CopyToInternal(Stream destination, long index, int length)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (MemoryMappedFile != null)
            {
                var buffer = new byte[length];
                MemoryMappedFile.CreateViewAccessor(index, length).ReadArray(0, buffer, 0, length);
                destination.Write(buffer, 0, length);
            }
            else if (_inMemoryFile != null)
            {
                if (index > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }

                destination.Write(_inMemoryFile, (int)index, length);
            }
            else
            {
                var buffer = Read(index, length);
                destination.Write(buffer, 0, length);
            }
        }

        /// <summary>
        /// Copies data into the given stream.  Thread-safe.
        /// </summary>
        /// <param name="destination">Stream to which to copy data.</param>
        /// <param name="index">Index of the data to start reading from the <see cref="GenericFile"/>.</param>
        /// <param name="length">Number of bytes to copy into the destination stream.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null.</exception>
        /// <remarks>Currently, the data of size <paramref name="length"/> is buffered in memory, and will error if there is insufficient memory.
        ///
        /// To avoid threading issues, this function will synchronously block using SyncLock until the operation is complete.</remarks>
        public void CopyTo(Stream destination, long index, int length)
        {
            if (IsThreadSafe)
            {
                CopyToInternal(destination, index, length);
            }
            else
            {                
                lock (_fileAccessLock)
                {
                    CopyToInternal(destination, index, length);
                }
            }
        }

        /// <summary>
        /// Copies data into the given stream.  Thread-safe.
        /// </summary>
        /// <param name="destination">Stream to which to copy data.</param>
        /// <param name="index">Index of the data to start reading from the <see cref="GenericFile"/>.</param>
        /// <param name="length">Number of bytes to copy into the destination stream.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null.</exception>
        /// <remarks>Currently, the data of size <paramref name="length"/> is buffered in memory, and will error if there is insufficient memory.
        ///
        /// To avoid threading issues, this function will synchronously block using SyncLock until the operation is complete.</remarks>
        public async Task CopyToAsync(Stream destination, long index, int length)
        {
            if (IsThreadSafe)
            {
                CopyToInternal(destination, index, length);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        CopyToInternal(destination, index, length);
                    }
                });
            }
        }

        /// <summary>
        /// Reads data from the given stream.
        /// </summary>
        /// <param name="source">Stream from which to read data.</param>
        /// <param name="sourceIndex">Index of the stream to read data.</param>
        /// <param name="fileIndex">Index of the file where data should be written.</param>
        /// <param name="length">Length in bytes of the data to write.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        private void CopyFromInternal(Stream source, long sourceIndex, long fileIndex, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var buffer = new byte[length];
            source.Seek(sourceIndex, SeekOrigin.Begin);
            source.Read(buffer, 0, length);

            if (MemoryMappedFile != null)
            {
                MemoryMappedFile.CreateViewAccessor(fileIndex, length).WriteArray(0, buffer, 0, length);
            }
            else if (_inMemoryFile == null)
            {
                if (fileIndex > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }

                buffer.CopyTo(_inMemoryFile, (int)fileIndex);
            }
            else if (FileReader != null)
            {
                FileReader.Seek(fileIndex, SeekOrigin.Begin);
                FileReader.Write(buffer, 0, length);
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.IO_GenericFile_IOWithoutHavingOpened);
            }
        }

        /// <summary>
        /// Reads data from the given stream.
        /// </summary>
        /// <param name="source">Stream from which to read data.</param>
        /// <param name="sourceIndex">Index of the stream to read data.</param>
        /// <param name="fileIndex">Index of the file where data should be written.</param>
        /// <param name="length">Length in bytes of the data to write.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        public void CopyFrom(Stream source, long sourceIndex, long fileIndex, int length)
        {
            if (IsThreadSafe)
            {
                CopyFromInternal(source, sourceIndex, fileIndex, length);
            }
            else
            {                
                lock (_fileAccessLock)
                {
                    CopyFromInternal(source, sourceIndex, fileIndex, length);
                }
            }
        }

        /// <summary>
        /// Reads data from the given stream.
        /// </summary>
        /// <param name="source">Stream from which to read data.</param>
        /// <param name="sourceIndex">Index of the stream to read data.</param>
        /// <param name="fileIndex">Index of the file where data should be written.</param>
        /// <param name="length">Length in bytes of the data to write.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        public async Task CopyFromAsync(Stream source, long sourceIndex, long fileIndex, int length)
        {
            if (IsThreadSafe)
            {
                CopyFromInternal(source, sourceIndex, fileIndex, length);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        CopyFromInternal(source, sourceIndex, fileIndex, length);
                    }
                });
            }
        }

        #endregion

        #region Integer Read/Write        

        /// <summary>
        /// Reads the signed 16 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public Int16 ReadInt16()
        {
            var output = this.ReadInt16(Position);
            Position += 2;
            return output;
        }

        /// <summary>
        /// Reads the signed 16 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public Int32 ReadInt32()
        {
            var output = this.ReadInt32(Position);
            Position += 4;
            return output;
        }

        /// <summary>
        /// Reads the signed 64 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public Int64 ReadInt64()
        {
            var output = this.ReadInt64(Position);
            Position += 8;
            return output;
        }

        /// <summary>
        /// Reads the unsigned 16 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public UInt16 ReadUInt16()
        {
            var output = this.ReadUInt16(Position);
            Position += 2;
            return output;
        }

        /// <summary>
        /// Reads the unsigned 32 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public UInt32 ReadUInt32()
        {
            var output = this.ReadUInt32(Position);
            Position += 4;
            return output;
        }        

        /// <summary>
        /// Reads the unsigned 64 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public UInt64 ReadUInt64()
        {
            var output = this.ReadUInt64(Position);
            Position += 8;
            return output;
        }

        /// <summary>
        /// Writes the signed 16 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteInt16(Int16 value)
        {
            this.WriteInt16(Position, value);
            Position += 2;
        }

        /// <summary>
        /// Writes the signed 32 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        public void WriteInt32(Int32 value)
        {
            this.WriteInt32(Position, value);
            Position += 4;
        }
        
        /// <summary>
        /// Writes the signed 64 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        public void WriteInt64(Int64 value)
        {
            this.WriteInt64(Position, value);
            Position += 8;
        }        

        /// <summary>
        /// Writes the unsigned 16 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteUInt16(UInt16 value)
        {
            this.WriteUInt16(Position, value);
            Position += 2;
        }

        /// <summary>
        /// Writes the unsigned 32 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteUInt32(UInt32 value)
        {
            this.WriteUInt32(Position, value);
            Position += 4;
        }

        
        /// <summary>
        /// Writes the unsigned 64 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteUInt64(UInt64 value)
        {
            this.WriteUInt64(Position, value);
            Position += 8;
        }
        #endregion

        #endregion

        public virtual string GetDefaultExtension()
        {
            return null;
        }

        public virtual IEnumerable<string> GetSupportedExtensions()
        {
            return null;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && !DisableDispose)
                {
                    if (MemoryMappedFile != null)
                    {
                        MemoryMappedFile.Dispose();
                    }

                    if (_fileReader != null)
                    {
                        _fileReader.Dispose();
                    }

                    // Delete the temporary file if shadow copy is enabled, the current I/O provider is not null, the physical filename exists, and the physical filename is different than the logical one
                    if (EnableShadowCopy && _inMemoryFile == null && CurrentFileSystem != null && !string.IsNullOrEmpty(PhysicalFilename) && CurrentFileSystem.FileExists(PhysicalFilename) && Filename != PhysicalFilename)
                    {
                        CurrentFileSystem.DeleteFile(PhysicalFilename);
                    }                    
                }

                _inMemoryFile = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GenericFile() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
