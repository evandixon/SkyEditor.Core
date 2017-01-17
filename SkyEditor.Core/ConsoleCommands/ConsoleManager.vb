Imports System.Text.RegularExpressions

Namespace ConsoleCommands

    ''' <summary>
    ''' Provides the core logic for the Sky Editor console.
    ''' </summary>
    Public Class ConsoleManager
        Public Sub New(manager As PluginManager)
            Console = manager.CurrentConsoleProvider
            For Each item In manager.GetRegisteredObjects(Of ConsoleCommandAsync)
                item.CurrentPluginManager = manager
                item.Console = Console
                AllCommands.Add(item.CommandName, item)
            Next
        End Sub

        Protected Property AllCommands As Dictionary(Of String, ConsoleCommandAsync)

        Protected Property Console As IConsoleProvider

        Protected ReadOnly Property ParameterRegex As New Regex("(\"".*?\"")|(\S+)", RegexOptions.None)

        ''' <summary>
        ''' Listens for user input from the console provider provided via the plugin manager from <see cref="ConsoleManager.New(PluginManager)"/> and handles commands accordingly.
        ''' </summary>
        Public Async Function RunConsole() As Task
            While True
                Dim cmdParts = Console.ReadLine().Split(" ".ToCharArray, 2)

                Dim commandString = cmdParts(0).ToLower
                Dim argumentString = If(cmdParts.Length > 1, cmdParts(1), Nothing)

                If commandString = "exit" Then
                    'Stop listening
                    Exit While
                ElseIf commandString = "help" Then
                    'List available commands
                    Console.WriteLine(My.Resources.Language.ConsoleAvailableCommands)
                    Dim commands As New List(Of String)(AllCommands.Keys)
                    commands.Sort()
                    For Each item In commands
                        Console.WriteLine(item)
                    Next
                ElseIf AllCommands.Keys.Contains(commandString, StringComparer.CurrentCultureIgnoreCase) Then
                    Await RunCommand(commandString, argumentString, True).ConfigureAwait(False)
                Else
                    'Command not found
                    Console.WriteLine(String.Format(My.Resources.Language.ConsoleUnknownCommand, commandString))
                End If
            End While
        End Function

        ''' <summary>
        ''' Runs the command with the given name using the given arguments.
        ''' </summary>
        ''' <param name="commandName">Name of the command.</param>
        ''' <param name="argumentString">String containing the arguments of the command, separated by spaces.  Use quotation marks to include spaces in a parameter.</param>
        ''' <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        Public Async Function RunCommand(commandName As String, argumentString As String, Optional reportErrorsToConsole As Boolean = False) As Task
            'Split arg on spaces, while respecting quotation marks
            Dim args As New List(Of String)
            If argumentString IsNot Nothing Then
                For Each item As Match In ParameterRegex.Matches(argumentString)
                    args.Add(item.Value.Trim(""""))
                Next
            End If

            'Run the command
            Await RunCommand(commandName, args).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' Runs the command with the given name using the given arguments.
        ''' </summary>
        ''' <param name="commandName">Name of the command.</param>
        ''' <param name="arguments">Arguments of the command.</param>
        ''' <param name="reportErrorsToConsole">True to print exceptions in the console.  False to throw the exception.</param>
        Public Async Function RunCommand(commandName As String, arguments As IEnumerable(Of String), Optional reportErrorsToConsole As Boolean = False) As Task
            Try
                Dim command = (From c In AllCommands Where String.Compare(c.Key, commandName, StringComparison.CurrentCultureIgnoreCase) = 0 Select c.Value).SingleOrDefault
                Await command.MainAsync(arguments.ToArray).ConfigureAwait(False)
            Catch ex As Exception
                If reportErrorsToConsole Then
                    Console.WriteLine(ex.ToString)
                Else
                    Throw
                End If
            End Try
        End Function

    End Class
End Namespace

