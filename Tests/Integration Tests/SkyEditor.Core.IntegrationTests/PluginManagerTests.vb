Imports System.IO
Imports SkyEditor.Core.Settings

<TestClass>
Public Class PluginManagerTests
    Public Const TestCategory = "Plugin Manager Integration Tests"

    <TestMethod> <TestCategory(TestCategory)>
    Public Sub TestScheduledDirectoryDeletion()
        'Set up test
        Dim manager1 As PluginManager = Nothing
        Try
            manager1 = New PluginManager
            manager1.LoadCore(New NoPluginsPluginDefinition).Wait()

            Directory.CreateDirectory("TestDirectory")
            File.WriteAllText(Path.Combine("TestDirectory", "test.txt"), "Test file")

            If Not Directory.Exists("TestDirectory") Then
                Assert.Inconclusive("Failed to create test directory.")
            End If

            If Not File.Exists(Path.Combine("TestDirectory", "test.txt")) Then
                Assert.Inconclusive("Failed to create test file.")
            End If
        Catch ex As Exception
            If manager1 IsNot Nothing Then
                manager1.Dispose()
                manager1 = Nothing
            End If
            Assert.Inconclusive("Error setting up test.  Exception message: " & ex.ToString)
        End Try
        If manager1 Is Nothing Then
            Exit Sub
        End If

        'Act
        manager1.CurrentSettingsProvider.ScheduleDirectoryForDeletion("TestDirectory", manager1.CurrentIOProvider)
        manager1.Dispose()

        Dim manager2 As New PluginManager
        manager2.LoadCore(New NoPluginsPluginDefinition).Wait() 'Should delete directory here

        Assert.IsFalse(Directory.Exists("TestDirectory"), "Failed to delete TestDirectory")
    End Sub
End Class
