Imports System.Threading.Tasks

Namespace ConsoleCommands
    ''' <summary>
    ''' Represents a console command subroutine.
    ''' </summary>
    Public MustInherit Class ConsoleCommand
        Inherits ConsoleCommandAsync

        Public Overrides Function MainAsync(arguments() As String) As Task
            Main(arguments)
            Return Task.FromResult(0)
        End Function

        Public MustOverride Sub Main(arguments As String())
    End Class
End Namespace