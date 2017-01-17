Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.TestComponents
Imports SkyEditor.Core.Tests.TestComponents

Namespace ConsoleCommands
    <TestClass> Public Class ConsoleCommandTestIntegration
        Private Const ConsoleTestsCategory As String = "Console Tests"

        Public Class TestConsoleCommand
            Inherits ConsoleCommand

            Public Overrides Sub Main(arguments() As String)
                Console.WriteLine(CurrentPluginManager IsNot Nothing)
                For Each item In arguments
                    Console.WriteLine(item)
                Next
                Dim stdInLine = Console.ReadLine
                While Not String.IsNullOrEmpty(stdInLine)
                    Console.WriteLine(stdInLine)
                    stdInLine = Console.ReadLine
                End While
            End Sub
        End Class

        <TestMethod> <TestCategory(ConsoleTestsCategory)> Public Sub ConsoleCommandTest()
            Dim manager As New PluginManager
            manager.LoadCore(New TestCoreMod)

            Dim command As New TestConsoleCommand

            Assert.AreEqual("True\ntest\narguments\nstandard\nin\n".Replace("\n", vbCrLf), TestHelpers.TestConsoleCommand(command, manager, {"test", "arguments"}, "standard" & vbCrLf & "in").Result)
        End Sub
    End Class
End Namespace
