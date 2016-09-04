Imports SkyEditor.Core.TestComponents

Namespace TestComponentTests
    <TestClass>
    Public Class MemoryIOProviderTests
        Public Const MemoryIOProviderCategory = "Memory IOProvider Tests"

        Public Property Provider As MemoryIOProvider

        <TestInitialize>
        Public Sub Init()
            Provider = New MemoryIOProvider()
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub FileExistsNegativeTest()
            Assert.IsFalse(Provider.FileExists(""), "No files should exist.")
            Assert.IsFalse(Provider.FileExists("/temp/0"), "No files should exist.")
            Assert.IsFalse(Provider.FileExists("/directory"), "No files should exist.")
            Assert.IsFalse(Provider.FileExists("/"), "No files should exist.")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub DirectoryExistsNegativeTest()
            Assert.IsFalse(Provider.DirectoryExists(""), "No directories should exist.")
            Assert.IsFalse(Provider.DirectoryExists("/temp/0"), "No directories should exist.")
            Assert.IsFalse(Provider.DirectoryExists("/directory"), "No directories should exist.")
            Assert.IsFalse(Provider.DirectoryExists("/"), "No directories should exist.")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub CreateDirectory()
            Provider.CreateDirectory("/directory")
            Assert.IsTrue(Provider.DirectoryExists("/directory"), "Directory ""/directory"" not created")

            Provider.CreateDirectory("/directory/subDirectory")
            Assert.IsTrue(Provider.DirectoryExists("/directory/subDirectory"), "Directory ""/directory/subDirectory"" not created")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub CreateDirectoryRecursive()
            Provider.CreateDirectory("/root/directory")
            If Not Provider.DirectoryExists("/root/directory") Then
                Assert.Inconclusive("Directory /root/directory not created.")
            End If
            Assert.IsTrue(Provider.DirectoryExists("/root"), "Directory ""/root"" not created when ""/root/directory"" was created.")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub ByteReadWrite()
            Dim testSequence As Byte() = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
            Provider.WriteAllBytes("/testFile.bin", testSequence)

            Dim read = Provider.ReadAllBytes("/testFile.bin")
            Assert.AreEqual(testSequence, read)
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub TextReadWrite()
            Dim testSequence As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZqbcdefghijklmnopqrstuvwxyz0123456789àèéêç"
            Provider.WriteAllText("/testFile.bin", testSequence)

            Dim read = Provider.ReadAllText("/testFile.bin")
            Assert.AreEqual(testSequence, read)
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub FileLength()
            Dim testSequence As Byte() = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
            Provider.WriteAllBytes("/testFile.bin", testSequence)

            Assert.AreEqual(CType(testSequence.Length, Long), Provider.GetFileLength("/testFile.bin"))
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub DeleteDirectory()
            Provider.CreateDirectory("/directory/subDirectory")
            Provider.CreateDirectory("/test/directory")
            If Not Provider.DirectoryExists("/directory") OrElse Not Provider.DirectoryExists("/test") Then
                Assert.Inconclusive("Couldn't create test directory")
            End If

            Provider.DeleteDirectory("/test/directory")
            Assert.IsFalse(Provider.DirectoryExists("/test/directory"), "Directory ""/test/directory"" not deleted.")
            Assert.IsTrue(Provider.DirectoryExists("/test"), "Incorrect directory deleted: ""/test""")
            Assert.IsTrue(Provider.DirectoryExists("/directory/subDirectory"), "Incorrect directory deleted: ""/directory/subDirectory""")
            Assert.IsTrue(Provider.DirectoryExists("/directory"), "Incorrect directory deleted: ""/directory""")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub DeleteDirectoryRecursive()
            Provider.CreateDirectory("/directory/subDirectory")
            Provider.CreateDirectory("/test/directory")
            If Not Provider.DirectoryExists("/directory") OrElse Not Provider.DirectoryExists("/test") Then
                Assert.Inconclusive("Couldn't create test directory")
            End If

            Provider.DeleteDirectory("/test")
            Assert.IsFalse(Provider.DirectoryExists("/test/directory"), "Directory ""/test/directory"" not deleted recursively.")
            Assert.IsFalse(Provider.DirectoryExists("/test"), "Directory ""/test"" not deleted.")
            Assert.IsTrue(Provider.DirectoryExists("/directory/subDirectory"), "Incorrect directory deleted: ""/directory/subDirectory""")
            Assert.IsTrue(Provider.DirectoryExists("/directory"), "Incorrect directory deleted: ""/directory""")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub DeleteFile()
            Dim testSequence As Byte() = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
            Provider.WriteAllBytes("/testFile.bin", testSequence)

            If Not Provider.FileExists("/testFile.bin") Then
                Assert.Inconclusive("Unable to create test file.")
            End If

            Provider.DeleteFile("/testFile.bin")

            Assert.IsFalse(Provider.FileExists("/testFile.bin"), "File not deleted.")
        End Sub

        <TestMethod> <TestCategory(MemoryIOProviderCategory)>
        Public Sub CopyFile()
            Dim testSequence As Byte() = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
            Provider.WriteAllBytes("/testFile.bin", testSequence)

            If Not Provider.FileExists("/testFile.bin") Then
                Assert.Inconclusive("Unable to create test file.")
            End If

            Provider.CopyFile("/testFile.bin", "/testFile2.bin")

            Assert.IsTrue(Provider.FileExists("/testFile2.bin"), "File not copied.")
            Assert.AreEqual(testSequence, Provider.ReadAllBytes("/testFile2.bin"), "Copied file has incorrect contents.")
        End Sub
    End Class
End Namespace
