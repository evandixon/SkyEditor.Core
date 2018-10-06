using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.IO
{
    /// <summary>
    /// Provides a view to a subset of a <see cref="IBinaryDataAccessor"/> or other <see cref="BinaryDataAccessorReference"/>
    /// </summary>
    public class BinaryDataAccessorReference : IBinaryDataAccessor
    {
        public BinaryDataAccessorReference(IBinaryDataAccessor data, long offset, long length)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Data = data ?? throw new ArgumentNullException(nameof(data));
            Offset = offset;
            Length = length;
        }

        public BinaryDataAccessorReference(BinaryDataAccessorReference reference, long offset, long length)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Data = reference.Data;
            Offset = reference.Offset + offset;
            Length = length;
        }

        private IBinaryDataAccessor Data { get; }

        private long Offset { get; set; }

        public long Length { get; private set; }

        public byte[] Read()
        {
            if (Length > int.MaxValue)
            {
                throw new ArgumentException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
            }

            return Data.Read(Offset, (int)Length);
        }

        public async Task<byte[]> ReadAsync()
        {
            if (Length > int.MaxValue)
            {
                throw new ArgumentException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
            }

            return await Data.ReadAsync(Offset, (int)Length);
        }

        public byte Read(long index)
        {
            return Data.Read(Offset + index);
        }

        public async Task<byte> ReadAsync(long index)
        {
            return await Data.ReadAsync(Offset + index);
        }

        public byte[] Read(long index, int length)
        {
            return Data.Read(Offset + index, (int)Math.Min(Length, length));
        }

        public async Task<byte[]> ReadAsync(long index, int length)
        {
            return await Data.ReadAsync(Offset + index, (int)Math.Min(Length, length));
        }

        public void Write(byte[] value)
        {
            if (Length > int.MaxValue)
            {
                throw new ArgumentException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
            }

            Data.Write(Offset, (int)Length, value);
        }

        public async Task WriteAsync(byte[] value)
        {
            if (Length > int.MaxValue)
            {
                throw new ArgumentException(Properties.Resources.IO_GenericFile_ErrorLengthTooLarge);
            }

            await Data.WriteAsync(Offset, (int)Length, value);
        }

        public void Write(long index, byte value)
        {
            Data.Write(Offset + index, value);
        }

        public async Task WriteAsync(long index, byte value)
        {
            await Data.WriteAsync(Offset + index, value);
        }

        public void Write(long index, int length, byte[] value)
        {
            Data.Write(Offset + index, (int)Math.Min(length, Length), value);
        }

        public async Task WriteAsync(long index, int length, byte[] value)
        {
            await Data.WriteAsync(Offset + index, (int)Math.Min(length, Length), value);
        }

    }

    public static class IBinaryDataAccessorExtensions
    {
        /// <summary>
        /// Gets a view on top of the current data
        /// </summary>
        /// <param name="data">Data to reference</param>
        /// <param name="offset">Offset of the view</param>
        /// <param name="length">Maximum length of the view</param>
        /// <returns>A view on top of the data</returns>
        public static BinaryDataAccessorReference GetDataReference(this IBinaryDataAccessor data, long offset, long length)
        {
            return new BinaryDataAccessorReference(data, offset, length);
        }
    }
}