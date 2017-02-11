﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.IO
{
    [TestClass]
    public class GenericFileTests
    {
        public const string TestCategory = "I/O - GenericFile";

        #region Low-level Access
        [TestMethod]
        [TestCategory(TestCategory)]
        public void Read_Sync_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.IsTrue(testData.SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void Read_Sync_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.IsTrue(testData.SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task Read_Async_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.IsTrue(testData.SequenceEqual(await f.ReadAsync()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task Read_Async_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.IsTrue(testData.SequenceEqual(await f.ReadAsync()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadByte_Sync_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.AreEqual(testData[0], f.Read(0));
                Assert.AreEqual(testData[1], f.Read(1));
                Assert.AreEqual(testData[2], f.Read(2));
                Assert.AreEqual(testData[3], f.Read(3));
                Assert.AreEqual(testData[4], f.Read(4));
                Assert.AreEqual(testData[5], f.Read(5));
                Assert.AreEqual(testData[6], f.Read(6));
                Assert.AreEqual(testData[7], f.Read(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadByte_Sync_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.AreEqual(testData[0], f.Read(0));
                Assert.AreEqual(testData[1], f.Read(1));
                Assert.AreEqual(testData[2], f.Read(2));
                Assert.AreEqual(testData[3], f.Read(3));
                Assert.AreEqual(testData[4], f.Read(4));
                Assert.AreEqual(testData[5], f.Read(5));
                Assert.AreEqual(testData[6], f.Read(6));
                Assert.AreEqual(testData[7], f.Read(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadByte_Async_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.AreEqual(testData[0], await f.ReadAsync(0));
                Assert.AreEqual(testData[1], await f.ReadAsync(1));
                Assert.AreEqual(testData[2], await f.ReadAsync(2));
                Assert.AreEqual(testData[3], await f.ReadAsync(3));
                Assert.AreEqual(testData[4], await f.ReadAsync(4));
                Assert.AreEqual(testData[5], await f.ReadAsync(5));
                Assert.AreEqual(testData[6], await f.ReadAsync(6));
                Assert.AreEqual(testData[7], await f.ReadAsync(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadByte_Async_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(testData.Clone() as byte[]);
                Assert.AreEqual(testData[0], await f.ReadAsync(0));
                Assert.AreEqual(testData[1], await f.ReadAsync(1));
                Assert.AreEqual(testData[2], await f.ReadAsync(2));
                Assert.AreEqual(testData[3], await f.ReadAsync(3));
                Assert.AreEqual(testData[4], await f.ReadAsync(4));
                Assert.AreEqual(testData[5], await f.ReadAsync(5));
                Assert.AreEqual(testData[6], await f.ReadAsync(6));
                Assert.AreEqual(testData[7], await f.ReadAsync(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadSequence_Sync_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                for (int start = 0; start < 7; start++)
                {
                    for (int length = 1; length < 7 - start;length++)
                    {
                        var value = f.Read(start, length);
                        Assert.IsTrue(testData.Skip(start).Take(length).SequenceEqual(value), "Failed to execute Read(" + start.ToString() + ", " + length.ToString() + ").");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadSequence_Sync_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                for (int start = 0; start < 7; start++)
                {
                    for (int length = 1; length < 7 - start; length++)
                    {
                        var value = f.Read(start, length);
                        Assert.IsTrue(testData.Skip(start).Take(length).SequenceEqual(value), "Failed to execute Read(" + start.ToString() + ", " + length.ToString() + ").");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadSequence_Async_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                for (int start = 0; start < 7; start++)
                {
                    for (int length = 1; length < 7 - start; length++)
                    {
                        var value = await f.ReadAsync(start, length);
                        Assert.IsTrue(testData.Skip(start).Take(length).SequenceEqual(value), "Failed to execute ReadAsync(" + start.ToString() + ", " + length.ToString() + ").");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadSequence_Async_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(testData.Clone() as byte[]);
                for (int start = 0; start < 7; start++)
                {
                    for (int length = 1; length < 7 - start; length++)
                    {
                        var value = await f.ReadAsync(start, length);
                        Assert.IsTrue(testData.Skip(start).Take(length).SequenceEqual(value), "Failed to execute ReadAsync(" + start.ToString() + ", " + length.ToString() + ").");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void Write_Sync_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(new byte[8]);
                f.Write(testData);
                Assert.IsTrue(testData.SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void Write_Sync_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(new byte[8]);
                f.Write(testData);
                Assert.IsTrue(testData.SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task Write_Async_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(new byte[8]);
                f.Write(testData);
                Assert.IsTrue(testData.SequenceEqual(await f.ReadAsync()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task Write_Async_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(new byte[8]);
                f.Write(testData);
                Assert.IsTrue(testData.SequenceEqual(await f.ReadAsync()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteByte_Sync_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(new byte[8]);
                f.Write(0, testData[0]);
                f.Write(1, testData[1]);
                f.Write(2, testData[2]);
                f.Write(3, testData[3]);
                f.Write(4, testData[4]);
                f.Write(5, testData[5]);
                f.Write(6, testData[6]);
                f.Write(7, testData[7]);
                Assert.AreEqual(testData[0], f.Read(0));
                Assert.AreEqual(testData[1], f.Read(1));
                Assert.AreEqual(testData[2], f.Read(2));
                Assert.AreEqual(testData[3], f.Read(3));
                Assert.AreEqual(testData[4], f.Read(4));
                Assert.AreEqual(testData[5], f.Read(5));
                Assert.AreEqual(testData[6], f.Read(6));
                Assert.AreEqual(testData[7], f.Read(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteByte_Sync_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(new byte[8]);
                f.Write(0, testData[0]);
                f.Write(1, testData[1]);
                f.Write(2, testData[2]);
                f.Write(3, testData[3]);
                f.Write(4, testData[4]);
                f.Write(5, testData[5]);
                f.Write(6, testData[6]);
                f.Write(7, testData[7]);
                Assert.AreEqual(testData[0], f.Read(0));
                Assert.AreEqual(testData[1], f.Read(1));
                Assert.AreEqual(testData[2], f.Read(2));
                Assert.AreEqual(testData[3], f.Read(3));
                Assert.AreEqual(testData[4], f.Read(4));
                Assert.AreEqual(testData[5], f.Read(5));
                Assert.AreEqual(testData[6], f.Read(6));
                Assert.AreEqual(testData[7], f.Read(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteByte_Async_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(new byte[8]);
                await f.WriteAsync(0, testData[0]);
                await f.WriteAsync(1, testData[1]);
                await f.WriteAsync(2, testData[2]);
                await f.WriteAsync(3, testData[3]);
                await f.WriteAsync(4, testData[4]);
                await f.WriteAsync(5, testData[5]);
                await f.WriteAsync(6, testData[6]);
                await f.WriteAsync(7, testData[7]);
                Assert.AreEqual(testData[0], await f.ReadAsync(0));
                Assert.AreEqual(testData[1], await f.ReadAsync(1));
                Assert.AreEqual(testData[2], await f.ReadAsync(2));
                Assert.AreEqual(testData[3], await f.ReadAsync(3));
                Assert.AreEqual(testData[4], await f.ReadAsync(4));
                Assert.AreEqual(testData[5], await f.ReadAsync(5));
                Assert.AreEqual(testData[6], await f.ReadAsync(6));
                Assert.AreEqual(testData[7], await f.ReadAsync(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteByte_Async_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(new byte[8]);
                await f.WriteAsync(0, testData[0]);
                await f.WriteAsync(1, testData[1]);
                await f.WriteAsync(2, testData[2]);
                await f.WriteAsync(3, testData[3]);
                await f.WriteAsync(4, testData[4]);
                await f.WriteAsync(5, testData[5]);
                await f.WriteAsync(6, testData[6]);
                await f.WriteAsync(7, testData[7]);
                Assert.AreEqual(testData[0], await f.ReadAsync(0));
                Assert.AreEqual(testData[1], await f.ReadAsync(1));
                Assert.AreEqual(testData[2], await f.ReadAsync(2));
                Assert.AreEqual(testData[3], await f.ReadAsync(3));
                Assert.AreEqual(testData[4], await f.ReadAsync(4));
                Assert.AreEqual(testData[5], await f.ReadAsync(5));
                Assert.AreEqual(testData[6], await f.ReadAsync(6));
                Assert.AreEqual(testData[7], await f.ReadAsync(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteByte_Sequence_Sync_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(new byte[8]);
                f.WriteByte(testData[0]);
                f.WriteByte(testData[1]);
                f.WriteByte(testData[2]);
                f.WriteByte(testData[3]);
                f.WriteByte(testData[4]);
                f.WriteByte(testData[5]);
                f.WriteByte(testData[6]);
                f.WriteByte(testData[7]);
                Assert.AreEqual(testData[0], f.Read(0));
                Assert.AreEqual(testData[1], f.Read(1));
                Assert.AreEqual(testData[2], f.Read(2));
                Assert.AreEqual(testData[3], f.Read(3));
                Assert.AreEqual(testData[4], f.Read(4));
                Assert.AreEqual(testData[5], f.Read(5));
                Assert.AreEqual(testData[6], f.Read(6));
                Assert.AreEqual(testData[7], f.Read(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteByte_Sequence_Sync_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(new byte[8]);
                f.WriteByte(testData[0]);
                f.WriteByte(testData[1]);
                f.WriteByte(testData[2]);
                f.WriteByte(testData[3]);
                f.WriteByte(testData[4]);
                f.WriteByte(testData[5]);
                f.WriteByte(testData[6]);
                f.WriteByte(testData[7]);
                Assert.AreEqual(testData[0], f.Read(0));
                Assert.AreEqual(testData[1], f.Read(1));
                Assert.AreEqual(testData[2], f.Read(2));
                Assert.AreEqual(testData[3], f.Read(3));
                Assert.AreEqual(testData[4], f.Read(4));
                Assert.AreEqual(testData[5], f.Read(5));
                Assert.AreEqual(testData[6], f.Read(6));
                Assert.AreEqual(testData[7], f.Read(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteByte_Sequence_Async_Memory()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = true;
                f.CreateFile(new byte[8]);
                await f.WriteByteAsync(testData[0]);
                await f.WriteByteAsync(testData[1]);
                await f.WriteByteAsync(testData[2]);
                await f.WriteByteAsync(testData[3]);
                await f.WriteByteAsync(testData[4]);
                await f.WriteByteAsync(testData[5]);
                await f.WriteByteAsync(testData[6]);
                await f.WriteByteAsync(testData[7]);
                Assert.AreEqual(testData[0], await f.ReadAsync(0));
                Assert.AreEqual(testData[1], await f.ReadAsync(1));
                Assert.AreEqual(testData[2], await f.ReadAsync(2));
                Assert.AreEqual(testData[3], await f.ReadAsync(3));
                Assert.AreEqual(testData[4], await f.ReadAsync(4));
                Assert.AreEqual(testData[5], await f.ReadAsync(5));
                Assert.AreEqual(testData[6], await f.ReadAsync(6));
                Assert.AreEqual(testData[7], await f.ReadAsync(7));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteByte_Sequence_Async_Stream()
        {
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (var f = new GenericFile())
            {
                f.EnableInMemoryLoad = false;
                f.CreateFile(new byte[8]);
                await f.WriteByteAsync(testData[0]);
                await f.WriteByteAsync(testData[1]);
                await f.WriteByteAsync(testData[2]);
                await f.WriteByteAsync(testData[3]);
                await f.WriteByteAsync(testData[4]);
                await f.WriteByteAsync(testData[5]);
                await f.WriteByteAsync(testData[6]);
                await f.WriteByteAsync(testData[7]);
                Assert.AreEqual(testData[0], await f.ReadAsync(0));
                Assert.AreEqual(testData[1], await f.ReadAsync(1));
                Assert.AreEqual(testData[2], await f.ReadAsync(2));
                Assert.AreEqual(testData[3], await f.ReadAsync(3));
                Assert.AreEqual(testData[4], await f.ReadAsync(4));
                Assert.AreEqual(testData[5], await f.ReadAsync(5));
                Assert.AreEqual(testData[6], await f.ReadAsync(6));
                Assert.AreEqual(testData[7], await f.ReadAsync(7));
            }
        }
        #endregion

        #region Integer Access
        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadInt16_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 255, 255 });
                Assert.AreEqual(42, f.ReadInt16(0));
                Assert.AreEqual(-1, f.ReadInt16(2));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadInt16_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 255, 255 });
                Assert.AreEqual(42, await f.ReadInt16Async(0));
                Assert.AreEqual(-1, await f.ReadInt16Async(2));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadInt16_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 255, 255 });
                Assert.AreEqual(42, f.ReadInt16());
                Assert.AreEqual(-1, f.ReadInt16());
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadUInt16_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 255, 255 });
                Assert.AreEqual(42, f.ReadUInt16(0));
                Assert.AreEqual(UInt16.MaxValue, f.ReadUInt16(2));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadUInt16_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 255, 255 });
                Assert.AreEqual(42, await f.ReadUInt16Async(0));
                Assert.AreEqual(UInt16.MaxValue, await f.ReadUInt16Async(2));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadUInt16_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 255, 255 });
                Assert.AreEqual(42, f.ReadUInt16());
                Assert.AreEqual(UInt16.MaxValue, f.ReadUInt16());
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadInt32_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 });
                Assert.AreEqual(42, f.ReadInt32(0));
                Assert.AreEqual(-1, f.ReadInt32(4));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadInt32_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 });
                Assert.AreEqual(42, await f.ReadInt32Async(0));
                Assert.AreEqual(-1, await f.ReadInt32Async(4));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadInt32_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 });
                Assert.AreEqual(42, f.ReadInt32());
                Assert.AreEqual(-1, f.ReadInt32());
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadUInt32_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 });
                Assert.AreEqual((UInt32)42, f.ReadUInt32(0));
                Assert.AreEqual(UInt32.MaxValue, f.ReadUInt32(4));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadUInt32_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 });
                Assert.AreEqual((UInt32)42, await f.ReadUInt32Async(0));
                Assert.AreEqual(UInt32.MaxValue, await f.ReadUInt32Async(4));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadUInt32_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 });
                Assert.AreEqual((UInt32)42, f.ReadUInt32());
                Assert.AreEqual(UInt32.MaxValue, f.ReadUInt32());
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadInt64_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 });
                Assert.AreEqual(42, f.ReadInt64(0));
                Assert.AreEqual(-1, f.ReadInt64(8));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadInt64_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 });
                Assert.AreEqual(42, await f.ReadInt64Async(0));
                Assert.AreEqual(-1, await f.ReadInt64Async(8));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadInt64_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 });
                Assert.AreEqual(42, f.ReadInt64());
                Assert.AreEqual(-1, f.ReadInt64());
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadUInt64_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 });
                Assert.AreEqual((UInt64)42, f.ReadUInt64(0));
                Assert.AreEqual(UInt64.MaxValue, f.ReadUInt64(8));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task ReadUInt64_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 });
                Assert.AreEqual((UInt64)42, await f.ReadUInt64Async(0));
                Assert.AreEqual(UInt64.MaxValue, await f.ReadUInt64Async(8));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void ReadUInt64_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 });
                Assert.AreEqual((UInt64)42, f.ReadUInt64());
                Assert.AreEqual(UInt64.MaxValue, f.ReadUInt64());
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteInt16_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[4]);
                f.WriteInt16(0, 42);
                f.WriteInt16(2, -1);
                Assert.IsTrue((new byte[] { 42, 00, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteInt16_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[4]);
                await f.WriteInt16Async(0, 42);
                await f.WriteInt16Async(2, -1);
                Assert.IsTrue((new byte[] { 42, 00, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteInt16_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[4]);
                f.WriteInt16(42);
                f.WriteInt16(-1);
                Assert.IsTrue((new byte[] { 42, 00, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteUInt16_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[4]);
                f.WriteUInt16(0, 42);
                f.WriteUInt16(2, UInt16.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteUInt16_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[4]);
                await f.WriteUInt16Async(0, 42);
                await f.WriteUInt16Async(2, UInt16.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteUInt16_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[4]);
                f.WriteUInt16(42);
                f.WriteUInt16(UInt16.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteInt32_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[8]);
                f.WriteInt32(0, 42);
                f.WriteInt32(4, -1);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteInt32_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[8]);
                await f.WriteInt32Async(0, 42);
                await f.WriteInt32Async(4, -1);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteInt32_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[8]);
                f.WriteInt32(42);
                f.WriteInt32(-1);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteUInt32_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[8]);
                f.WriteUInt32(0, 42);
                f.WriteUInt32(4, UInt32.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteUInt32_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[8]);
                await f.WriteUInt32Async(0, 42);
                await f.WriteUInt32Async(4, UInt32.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteUInt32_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[8]);
                f.WriteUInt32(42);
                f.WriteUInt32(UInt32.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteInt64_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[16]);
                f.WriteInt64(0, 42);
                f.WriteInt64(8, -1);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteInt64_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[16]);
                await f.WriteInt64Async(0, 42);
                await f.WriteInt64Async(8, -1);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteInt64_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[16]);
                f.WriteInt64(42);
                f.WriteInt64(-1);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteUInt64_Offset()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[16]);
                f.WriteUInt64(0, 42);
                f.WriteUInt64(8, UInt64.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task WriteUInt64_Async()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[16]);
                await f.WriteUInt64Async(0, 42);
                await f.WriteUInt64Async(8, UInt64.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WriteUInt64_Sequence()
        {
            using (var f = new GenericFile())
            {
                f.CreateFile(new byte[16]);
                f.WriteUInt64(42);
                f.WriteUInt64(UInt64.MaxValue);
                Assert.IsTrue((new byte[] { 42, 00, 00, 00, 00, 00, 00, 00, 255, 255, 255, 255, 255, 255, 255, 255 }).SequenceEqual(f.Read()));
            }
        }
        #endregion
    }
}