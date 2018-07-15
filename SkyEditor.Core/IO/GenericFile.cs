using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    public class GenericFile : INamed, ICreatableFile, IOpenableFile, IOnDisk, ISavableAs, IDisposable
    {

        /// <summary>
        /// Creates a new instance of <see cref="GenericFile"/>
        /// </summary>
        public GenericFile()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="GenericFile"/> using the data at the given file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="provider"></param>
        public GenericFile(string filename, IIOProvider provider)
        {
            OpenFileInternal(filename, provider);
        }

        /// <summary>
        /// Creates a new, read-only instance of <see cref="GenericFile"/> using the given file as the backing source
        /// </summary>
        /// <param name="file"></param>
        public GenericFile(GenericFile file)
        {
            IsReadOnly = true; // Read-only to avoid modifying the other file
            if (file.InMemoryFile != null)
            {
                this.InMemoryFile = file.InMemoryFile;
            }
            else
            {
                this.FileReader = file.FileReader;
            }
        }


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
        protected IIOProvider CurrentIOProvider { get; set; }

        /// <summary>
        /// The raw data of the file, if the file has been loaded in memory.  Null if <see cref="FileReader"/> is in use.
        /// </summary>
        private byte[] InMemoryFile;

        /// <summary>
        /// The underlying stream used to read and write to the file.  Null if <see cref="InMemoryFile"/> is in use.
        /// </summary>
        private Stream FileReader
        {
            get
            {
                if (InMemoryFile != null)
                {
                    return null;
                }
                else
                {
                    if (_fileReader == null)
                    {
                        if (IsReadOnly)
                        {
                            _fileReader = CurrentIOProvider.OpenFileReadOnly(PhysicalFilename);
                        }
                        else
                        {
                            _fileReader = CurrentIOProvider.OpenFile(PhysicalFilename);
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

        /// <summary>
        /// Used to ensure thread safety with <see cref="FileReader"/>
        /// </summary>
        private object _fileAccessLock = new object();

        /// <summary>
        /// The length of the file
        /// </summary>
        /// <exception cref="IOException">Thrown if the value is changed when <see cref="IsReadOnly"/> is true, or if the file is not loaded in memory and the length set fails.</exception>
        /// <exception cref="OverflowException">Thrown if the value is greater than <see cref="int.MaxValue"/> when the file is loaded in memory.</exception>
        public long Length
        {
            get
            {
                if (InMemoryFile != null)
                {
                    return InMemoryFile.Length;
                }
                else
                {
                    return FileReader.Length;
                }
            }
            set
            {
                if (IsReadOnly)
                {
                    throw new IOException(Properties.Resources.IO_ErrorReadOnly);
                }

                if (InMemoryFile != null)
                {
                    if (value > int.MaxValue)
                    {
                        throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                    }
                    else
                    {
                        Array.Resize(ref InMemoryFile, (int)value);
                    }
                }
                else
                {
                    FileReader.SetLength(value);
                }
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
                return InMemoryFile != null;
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
            CreateFileInternal(otherFile.Name, Array.Empty<byte>(), otherFile.EnableInMemoryLoad, otherFile.CurrentIOProvider);
            this.Length = otherFile.Length;

            if (otherFile.EnableInMemoryLoad)
            {
                this.InMemoryFile = otherFile.InMemoryFile.Clone() as byte[];
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
        private void CreateFileInternal(string name, byte[] contents, bool enableInMemoryLoad, IIOProvider provider)
        {
            this.Name = name;
            this.EnableInMemoryLoad = enableInMemoryLoad;

            if (!enableInMemoryLoad && provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (provider != null)
            {
                this.CurrentIOProvider = provider;
            }

            if (enableInMemoryLoad)
            {
                InMemoryFile = contents;
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
        public virtual Task OpenFile(string filename, IIOProvider provider)
        {
            OpenFileInternal(filename, provider);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the class to represent the data stored in the file at the given path
        /// </summary>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">Instance of the I/O provider that stores the file</param>
        private void OpenFileInternal(string filename, IIOProvider provider)
        {
            if (EnableInMemoryLoad)
            {
                byte[] contents;

                // Try to load file in memory
                try
                {
                    contents = provider.ReadAllBytes(filename);
                }
                catch (OutOfMemoryException)
                {
                    // File is too large.  Use stream instead.
                    OpenFileInternalStream(filename, provider);
                    return;
                }

                // Load succeeded
                InMemoryFile = contents;
                this.Filename = filename;
                this.PhysicalFilename = filename;
                this.CurrentIOProvider = provider;
            }
            else
            {
                OpenFileInternalStream(filename, provider);
            }
        }

        /// <summary>
        /// Initializes the class to represent the data stored in the file at the given path, without loading the file into memory
        /// </summary>
        /// <param name="filename">Path of the file</param>
        /// <param name="provider">Instance of the I/O provider that stores the file</param>
        private void OpenFileInternalStream(string filename, IIOProvider provider)
        {
            this.CurrentIOProvider = provider;
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
        public virtual async Task Save(string filename, IIOProvider provider)
        {
            FileSaving?.Invoke(this, new EventArgs());
            if (InMemoryFile != null)
            {
                provider.WriteAllBytes(filename, InMemoryFile);
            }
            else
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
            this.Filename = filename;
            FileSaved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Saves the file to the current path
        /// </summary>
        /// <param name="provider">Instance of the I/O provider that will the file</param>
        /// <exception cref="NullReferenceException">Thrown if <see cref="Filename"/> is null.</exception>
        public async Task Save(IIOProvider provider)
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
            if (InMemoryFile != null)
            {
                return InMemoryFile;
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
            if (InMemoryFile != null)
            {
                if (InMemoryFile.Length > index)
                {
                    return InMemoryFile[index];
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format(Properties.Resources.IO_GenericFile_OutOfRange, index, InMemoryFile.Length));
                }
            }
            else
            {
                FileReader.Seek(index, SeekOrigin.Begin);
                var b = FileReader.ReadByte();
                if (b > -1 && b < 256)
                {
                    return (byte)b;
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format(Properties.Resources.IO_GenericFile_OutOfRange, index, InMemoryFile.Length));
                }
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
            if (InMemoryFile != null)
            {
                if (index > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                var buffer = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = InMemoryFile[index + i];
                }
                return buffer;
            }
            else
            {
                byte[] buffer = new byte[length];
                FileReader.Seek(index, SeekOrigin.Begin);
                FileReader.Read(buffer, 0, length);
                return buffer;
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

            if (InMemoryFile != null)
            {
                InMemoryFile = value;
            }
            else
            {
                if (Length > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                FileReader.Seek(0, SeekOrigin.Begin);
                FileReader.Write(value, 0, (int)Length);
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

            if (InMemoryFile != null)
            {
                InMemoryFile[index] = value;
            }
            else
            {
                FileReader.Seek(index, SeekOrigin.Begin);
                FileReader.WriteByte(value);
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

            if (InMemoryFile != null)
            {
                if (index > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }
                value.Take(length).ToArray().CopyTo(InMemoryFile, (int)index);
            }
            else
            {
                FileReader.Seek(index, SeekOrigin.Begin);
                FileReader.Write(value, 0, length);
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
        /// Writes a range of bytes to the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="value">The data to write</param>
        public void Write(long index, byte[] value)
        {
            Write(index, value.Length, value);
        }

        /// <summary>
        /// Writes a range of bytes to the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index from which to retrieve the byte.</param>
        /// <param name="value">The data to write</param>
        public async Task WriteAsync(long index, byte[] value)
        {
            if (IsThreadSafe)
            {
                Write(index, value);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        Write(index, value);
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

            if (InMemoryFile == null)
            {
                if (index > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }

                destination.Write(InMemoryFile, (int)index, (int)length);
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

            if (InMemoryFile == null)
            {
                if (fileIndex > int.MaxValue)
                {
                    throw new OverflowException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
                }

                buffer.CopyTo(InMemoryFile, (int)fileIndex);
            }
            else
            {
                FileReader.Seek(fileIndex, SeekOrigin.Begin);
                FileReader.Write(buffer, 0, length);
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
        /// Reads a signed 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public Int16 ReadInt16(long offset)
        {
            return BitConverter.ToInt16(Read(offset, 2), 0);
        }

        /// <summary>
        /// Reads a signed 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public async Task<Int16> ReadInt16Async(long offset)
        {
            return BitConverter.ToInt16(await ReadAsync(offset, 2), 0);
        }

        /// <summary>
        /// Reads the signed 16 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public Int16 ReadInt16()
        {
            var output = ReadInt16(Position);
            Position += 2;
            return output;
        }

        /// <summary>
        /// Reads a signed 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public Int32 ReadInt32(long offset)
        {
            return BitConverter.ToInt32(Read(offset, 4), 0);
        }

        /// <summary>
        /// Reads a signed 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public async Task<Int32> ReadInt32Async(long offset)
        {
            return BitConverter.ToInt32(await ReadAsync(offset, 4), 0);
        }

        /// <summary>
        /// Reads the signed 16 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public Int32 ReadInt32()
        {
            var output = ReadInt32(Position);
            Position += 4;
            return output;
        }

        /// <summary>
        /// Reads a signed 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public Int64 ReadInt64(long offset)
        {
            return BitConverter.ToInt64(Read(offset, 8), 0);
        }

        /// <summary>
        /// Reads a signed 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public async Task<Int64> ReadInt64Async(long offset)
        {
            return BitConverter.ToInt64(await ReadAsync(offset, 8), 0);
        }

        /// <summary>
        /// Reads the signed 64 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public Int64 ReadInt64()
        {
            var output = ReadInt64(Position);
            Position += 8;
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public UInt16 ReadUInt16(long offset)
        {
            return BitConverter.ToUInt16(Read(offset, 2), 0);
        }

        /// <summary>
        /// Reads an unsigned 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public async Task<UInt16> ReadUInt16Async(long offset)
        {
            return BitConverter.ToUInt16(await ReadAsync(offset, 2), 0);
        }

        /// <summary>
        /// Reads the unsigned 16 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public UInt16 ReadUInt16()
        {
            var output = ReadUInt16(Position);
            Position += 2;
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public UInt32 ReadUInt32(long offset)
        {
            return BitConverter.ToUInt32(Read(offset, 4), 0);
        }

        /// <summary>
        /// Reads an unsigned 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public async Task<UInt32> ReadUInt32Async(long offset)
        {
            return BitConverter.ToUInt32(await ReadAsync(offset, 4), 0);
        }

        /// <summary>
        /// Reads the unsigned 32 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public UInt32 ReadUInt32()
        {
            var output = ReadUInt32(Position);
            Position += 4;
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public UInt64 ReadUInt64(long offset)
        {
            return BitConverter.ToUInt64(Read(offset, 8), 0);
        }

        /// <summary>
        /// Reads an unsigned 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to read.</param>
        /// <returns>The integer from the given location</returns>
        public async Task<UInt64> ReadUInt64Async(int offset)
        {
            return BitConverter.ToUInt64(await ReadAsync(offset, 8), 0);
        }

        /// <summary>
        /// Reads the unsigned 64 bit little endian integer from the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <returns>The integer from the current position</returns>
        public UInt64 ReadUInt64()
        {
            var output = ReadUInt64(Position);
            Position += 8;
            return output;
        }

        /// <summary>
        /// Writes a signed 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public void WriteInt16(long offset, Int16 value)
        {
            Write(offset, 2, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public async Task WriteInt16Async(long offset, Int16 value)
        {
            await WriteAsync(offset, 2, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the signed 16 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteInt16(Int16 value)
        {
            WriteInt16(Position, value);
            Position += 2;
        }

        /// <summary>
        /// Writes a signed 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public void WriteInt32(long offset, Int32 value)
        {
            Write(offset, 4, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public async Task WriteInt32Async(long offset, Int32 value)
        {
            await WriteAsync(offset, 4, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the signed 32 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        public void WriteInt32(Int32 value)
        {
            WriteInt32(Position, value);
            Position += 4;
        }

        /// <summary>
        /// Writes a signed 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public void WriteInt64(long offset, Int64 value)
        {
            Write(offset, 8, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public async Task WriteInt64Async(long offset, Int64 value)
        {
            await WriteAsync(offset, 8, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the signed 64 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        public void WriteInt64(Int64 value)
        {
            WriteInt64(Position, value);
            Position += 8;
        }

        /// <summary>
        /// Writes an unsigned 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public void WriteUInt16(long offset, UInt16 value)
        {
            Write(offset, 2, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 16 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public async Task WriteUInt16Async(long offset, UInt16 value)
        {
            await WriteAsync(offset, 2, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the unsigned 16 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteUInt16(UInt16 value)
        {
            WriteUInt16(Position, value);
            Position += 2;
        }

        /// <summary>
        /// Writes an unsigned 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public void WriteUInt32(long offset, UInt32 value)
        {
            Write(offset, 4, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 32 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public async Task WriteUInt32Async(long offset, UInt32 value)
        {
            await WriteAsync(offset, 4, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the unsigned 32 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteUInt32(UInt32 value)
        {
            WriteUInt32(Position, value);
            Position += 4;
        }

        /// <summary>
        /// Writes an unsigned 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public void WriteUInt64(long offset, UInt64 value)
        {
            Write(offset, 8, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 64 bit little endian integer.  This function is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the integer to write.</param>
        /// <param name="value">The integer to write</param>
        public async Task WriteUInt64Async(long offset, UInt64 value)
        {
            await WriteAsync(offset, 8, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the unsigned 64 bit little endian integer to the current position (<see cref="Position"/>), then advances the current position.  This function is not thread-safe.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void WriteUInt64(UInt64 value)
        {
            WriteUInt64(Position, value);
            Position += 8;
        }
        #endregion

        #region String Interaction

        /// <summary>
        /// Reads a UTF-16 string.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <param name="length">Length in characters of the string</param>
        /// <returns>The UTF-16 string at the given offset</returns>
        public string ReadUnicodeString(long index, int length)
        {
            return Encoding.Unicode.GetString(Read(index, length * 2), 0, length * 2);
        }

        /// <summary>
        /// Reads a UTF-16 string.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <param name="length">Length in characters of the string</param>
        /// <returns>The UTF-16 string at the given offset</returns>
        public async Task<string> ReadUnicodeStringAsync(long index, int length)
        {
            return Encoding.Unicode.GetString(await ReadAsync(index, length * 2), 0, length * 2);
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <returns>The UTF-16 string</returns>
        public string ReadNullTerminatedUnicodeString(long index)
        {
            int length = 0;
            while (Read(index + length * 2) != 0 || Read(index + length * 2 + 1) != 0)
            {
                length += 1;
            }
            return ReadUnicodeString(index, length);
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <returns>The UTF-16 string</returns>
        public async Task<string> ReadNullTerminatedUnicodeStringAsync(long index)
        {
            int length = 0;
            while (await ReadAsync(index + length * 2) != 0 || await ReadAsync(index + length * 2 + 1) != 0)
            {
                length += 1;
            }
            return ReadUnicodeString(index, length);
        }

        /// <summary>
        /// Reads a string using the given encoding.  This method is not thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <param name="length">Length in characters of the string</param>
        /// <returns>The UTF-16 string at the given offset</returns>
        public string ReadString(long index, int length, Encoding e)
        {
            return e.GetString(Read(index, length), 0, length);
        }

        /// <summary>
        /// Reads a string using the given encoding.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <param name="length">Length in characters of the string</param>
        /// <returns>The UTF-16 string at the given offset</returns>
        public async Task<string> ReadStringAsync(long index, int length, Encoding e)
        {
            return e.GetString(await ReadAsync(index, length), 0, length);
        }

        /// <summary>
        /// Writes a string with the given encoding to the given offset of the file.  Thread-safe
        /// </summary>
        /// <param name="index">Index of the file to write</param>
        /// <param name="e">The encoding to use</param>
        /// <param name="value">The string to write.  The entire string will be written without an ending null character.</param>
        public void WriteString(long index, Encoding e, string value)
        {
            Write(index, e.GetBytes(value));
        }

        /// <summary>
        /// Writes a string with the given encoding to the given offset of the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index of the file to write</param>
        /// <param name="e">The encoding to use</param>
        /// <param name="value">The string to write.  The entire string will be written without an ending null character.</param>
        public async Task WriteStringAsync(long index, Encoding e, string value)
        {
            await WriteAsync(index, e.GetBytes(value));
        }

        /// <summary>
        /// Reads a null-terminated string using the given encoding.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <returns>The string at the given location</returns>
        public string ReadNullTerminatedString(long index, Encoding e)
        {
            // The null character we're looking for
            var nullCharSequence = e.GetBytes(Convert.ToChar(0x0).ToString());

            // Find the length of the string as determined by the location of the null-char sequence
            int length = 0;
            while (!Read(index + length * nullCharSequence.Length, nullCharSequence.Length).All(x => x == 0))
            {
                length += 1;
            }

            return ReadString(index, length, e);
        }

        /// <summary>
        /// Reads a null-terminated using the given encoding.  This method is thread-safe.
        /// </summary>
        /// <param name="offset">Offset of the string</param>
        /// <returns>The string at the given location</returns>
        public async Task<string> ReadNullTerminatedStringAsync(long index, Encoding e)
        {
            // The null character we're looking for
            var nullCharSequence = e.GetBytes(Convert.ToChar(0x0).ToString());

            // Find the length of the string as determined by the location of the null-char sequence
            int length = 0;
            while (!(await ReadAsync(index + length * nullCharSequence.Length, nullCharSequence.Length)).All(x => x == 0))
            {
                length += 1;
            }

            return ReadString(index, length, e);
        }

        /// <summary>
        /// Writes a string with the given encoding to the given offset of the file.  Not thread-safe
        /// </summary>
        /// <param name="index">Index of the file to write</param>
        /// <param name="e">The encoding to use</param>
        /// <param name="value">The string to write.  The entire string will be written with an ending null character.</param>
        private void WriteNullTerminatedStringInternal(long index, Encoding e, string value)
        {
            var nullChar = e.GetBytes(new[] { Convert.ToChar(0) });
            var data = e.GetBytes(value);
            WriteInternal(index, data.Length, data);
            WriteInternal(index + data.Length, nullChar.Length, nullChar);
        }

        /// <summary>
        /// Writes a string with the given encoding to the given offset of the file.  Not thread-safe
        /// </summary>
        /// <param name="index">Index of the file to write</param>
        /// <param name="e">The encoding to use</param>
        /// <param name="value">The string to write.  The entire string will be written with an ending null character.</param>
        public void WriteNullTerminatedString(long index, Encoding e, string value)
        {
            if (IsThreadSafe)
            {
                WriteNullTerminatedStringInternal(index, e, value);
            }
            else
            {                
                lock (_fileAccessLock)
                {
                    WriteNullTerminatedStringInternal(index, e, value);
                }   
            }
        }

        /// <summary>
        /// Writes a string with the given encoding to the given offset of the file.  Thread-safe.
        /// </summary>
        /// <param name="index">Index of the file to write</param>
        /// <param name="e">The encoding to use</param>
        /// <param name="value">The string to write.  The entire string will be written with an ending null character.</param>
        public async Task WriteNullTerminatedStringAsync(long index, Encoding e, string value)
        {
            if (IsThreadSafe)
            {
                WriteNullTerminatedStringInternal(index, e, value);
            }
            else
            {
                await Task.Run(() =>
                {
                    lock (_fileAccessLock)
                    {
                        WriteNullTerminatedStringInternal(index, e, value);
                    }
                });
            }
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
                if (disposing)
                {
                    // Dispose of the file reader
                    if (_fileReader != null)
                    {
                        _fileReader.Dispose();
                    }

                    // Delete the temporary file if shadow copy is enabled, the current I/O provider is not null, the physical filename exists, and the physical filename is different than the logical one
                    if (EnableShadowCopy && InMemoryFile == null && CurrentIOProvider != null && !string.IsNullOrEmpty(PhysicalFilename) && CurrentIOProvider.FileExists(PhysicalFilename) && Filename != PhysicalFilename)
                    {
                        CurrentIOProvider.DeleteFile(PhysicalFilename);
                    }                    
                }

                InMemoryFile = null;

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
