Namespace ConsoleCommands.UI
    Public Class ViewFiles
        Inherits ConsoleCommand

        Public Overrides Sub Main(arguments() As String)
            Dim files = CurrentPluginManager.CurrentIOUIManager.OpenFiles
            Console.WriteLine($"{files.Count} files:")
            For count = 0 To files.Count - 1
                Console.WriteLine($"{count}: {files(count).Title}")
            Next
        End Sub
    End Class
End Namespace