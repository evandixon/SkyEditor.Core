Imports System.IO
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Windows.Providers

Namespace Utilties
    <TestClass>
    Public Class FileSystemTests
        Public Const TestCategory = "File System Integration Tests"

        Public Property CurrentIOProvider As IOProvider

        <TestInitialize>
        Public Sub Init()
            CurrentIOProvider = New WindowsIOProvider
        End Sub

        <TestMethod> <TestCategory(TestCategory)>
        Public Sub DeleteDirectory()
            Try
                Directory.CreateDirectory("TestDirectory")
                File.WriteAllText(Path.Combine("TestDirectory", "test.txt"), "Test file")
                Directory.CreateDirectory(Path.Combine("TestDirectory", "Test2"))
            Catch ex As Exception
                Assert.Inconclusive("Error creating test directory or file.  Exception message: " & ex.ToString)
            End Try

            SkyEditor.Core.Utilities.FileSystem.DeleteDirectory("TestDirectory", CurrentIOProvider).Wait()

            Assert.IsFalse(Directory.Exists("TestDirectory"), "Failed to delete TestDirectory")
        End Sub
    End Class
End Namespace
