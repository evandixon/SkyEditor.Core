Imports SkyEditor.Core.ConsoleCommands

Namespace TestComponents
    Public Class TestHelpers
        ''' <summary>
        ''' Runs a console command with custom input and returns the output.
        ''' </summary>
        ''' <param name="command">Command to run</param>
        ''' <param name="manager">Plugin manager to use</param>
        ''' <param name="arguments">Input arguments</param>
        ''' <param name="stdIn">New-line separated standard input</param>
        ''' <returns>New-line separated standard output</returns>
        Public Shared Async Function TestConsoleCommand(command As ConsoleCommandAsync, manager As PluginManager, arguments As String(), stdIn As String) As Task(Of String)
            Dim provider As New MemoryConsoleProvider
            provider.StdIn.Append(stdIn)
            command.CurrentPluginManager = manager
            command.Console = provider
            Await command.MainAsync(arguments)
            Return provider.GetStdOut
        End Function
    End Class
End Namespace