Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.TestComponents
Imports SkyEditor.Core.Windows.Providers

Namespace IO
    <TestClass()> Public Class GenericFileTests

        Private Const GenericFileCategory As String = "Generic File Tests"
        Private Const RealFileSystem As String = "Real File System Access"

        <TestMethod()> <TestCategory(GenericFileCategory)> Public Sub RawData_InMemory_1Byte_ReadTest()
            'Arrange
            Using file As New GenericFile()
                Dim data As Byte() = {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
                file.CreateFile(data)

                'Act
                Dim index0Test As Byte = file.RawData(0)
                Dim index1Test As Byte = file.RawData(1)
                Dim index2Test As Byte = file.RawData(2)
                Dim index3Test As Byte = file.RawData(3)
                Dim index4Test As Byte = file.RawData(4)
                Dim index5Test As Byte = file.RawData(5)
                Dim index6Test As Byte = file.RawData(6)
                Dim index7Test As Byte = file.RawData(7)
                Dim index8Test As Byte = file.RawData(8)
                Dim index9Test As Byte = file.RawData(9)

                Dim index0Actual As Byte = 9
                Dim index1Actual As Byte = 8
                Dim index2Actual As Byte = 7
                Dim index3Actual As Byte = 6
                Dim index4Actual As Byte = 5
                Dim index5Actual As Byte = 4
                Dim index6Actual As Byte = 3
                Dim index7Actual As Byte = 2
                Dim index8Actual As Byte = 1
                Dim index9Actual As Byte = 0

                'Assert
                Assert.AreEqual(index0Actual, index0Test, 0, $"Misread index 0: Expected {index0Actual}, got {index0Test}")
                Assert.AreEqual(index1Actual, index1Test, 0, $"Misread index 0: Expected {index1Actual}, got {index1Test}")
                Assert.AreEqual(index2Actual, index2Test, 0, $"Misread index 0: Expected {index2Actual}, got {index2Test}")
                Assert.AreEqual(index3Actual, index3Test, 0, $"Misread index 0: Expected {index3Actual}, got {index3Test}")
                Assert.AreEqual(index4Actual, index4Test, 0, $"Misread index 0: Expected {index4Actual}, got {index4Test}")
                Assert.AreEqual(index5Actual, index5Test, 0, $"Misread index 0: Expected {index5Actual}, got {index5Test}")
                Assert.AreEqual(index6Actual, index6Test, 0, $"Misread index 0: Expected {index6Actual}, got {index6Test}")
                Assert.AreEqual(index7Actual, index7Test, 0, $"Misread index 0: Expected {index7Actual}, got {index7Test}")
                Assert.AreEqual(index8Actual, index8Test, 0, $"Misread index 0: Expected {index8Actual}, got {index8Test}")
                Assert.AreEqual(index9Actual, index9Test, 0, $"Misread index 0: Expected {index9Actual}, got {index9Test}")
            End Using
        End Sub

        <TestMethod()> <TestCategory(GenericFileCategory)> Public Sub OpenFileTest()

            'Set up
            Dim data As Byte() = {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim provider As New MemoryIOProvider
            provider.WriteAllBytes("/test.bin", data)

            'Test
            Using file As New GenericFile
                file.OpenFile("/test.bin", provider).Wait()

                'Check
                Assert.IsTrue(data.SequenceEqual(file.RawData), "File data is incorrect.")
            End Using
        End Sub

        <TestMethod()> <TestCategory(GenericFileCategory)> Public Sub SaveFileTest()

            'Set up
            Dim data As Byte() = {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim finalData As Byte() = {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim provider As New MemoryIOProvider
            provider.WriteAllBytes("/test.bin", data)

            'Test
            Using file As New GenericFile
                file.OpenFile("/test.bin", provider).Wait()
                file.RawData(0) = 0
                file.Save(provider).Wait()

                'Check
                Assert.IsTrue(data.SequenceEqual(provider.ReadAllBytes("/test.bin")), "File data is incorrect.")
            End Using
        End Sub

        <TestMethod()> <TestCategory(GenericFileCategory)> Public Sub CreateFileCloneTest_InMemory()

            'Set up
            Dim dataOriginal As Byte() = {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim dataAltered As Byte() = {7, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim provider As New MemoryIOProvider
            provider.WriteAllBytes("/test.bin", dataOriginal)

            'Test
            Using file1 As New GenericFile
                file1.EnableInMemoryLoad = True
                file1.OpenFile("/test.bin", provider).Wait()
                file1.RawData(0) = 7

                Using file2 As New GenericFile
                    file2.CreateFile(file1, provider)

                    file1.RawData(0) = 9

                    'Check
                    Assert.IsTrue(dataOriginal.SequenceEqual(file1.RawData), "File 1 data is incorrect.")
                    Assert.IsTrue(dataAltered.SequenceEqual(file2.RawData), "File 2 data is incorrect.")

                End Using
            End Using
        End Sub

        <TestMethod()> <TestCategory(GenericFileCategory)> <TestCategory(RealFileSystem)> Public Sub CreateFileCloneTest_Stream()

            'Set up
            Dim dataOriginal As Byte() = {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim dataAltered As Byte() = {7, 8, 7, 6, 5, 4, 3, 2, 1, 0}
            Dim provider As New WindowsIOProvider
            provider.WriteAllBytes("test.bin", dataOriginal)

            'Test
            Using file1 As New GenericFile
                file1.EnableInMemoryLoad = False
                file1.OpenFile("test.bin", provider).Wait()
                file1.RawData(0) = 7

                Using file2 As New GenericFile
                    file2.CreateFile(file1, provider)

                    file1.RawData(0) = 9

                    'Check
                    Assert.IsTrue(dataOriginal.SequenceEqual(file1.RawData), "File 1 data is incorrect.")
                    Assert.IsTrue(dataAltered.SequenceEqual(file2.RawData), "File 2 data is incorrect.")
                End Using
            End Using
            provider.DeleteFile("test.bin")

        End Sub

    End Class
End Namespace
