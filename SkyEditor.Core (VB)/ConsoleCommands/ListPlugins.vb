Imports System.Reflection
Namespace ConsoleCommands
    Public Class ListPlugins
        Inherits ConsoleCommand

        Public Overrides Sub Main(arguments() As String)
            Console.WriteLine("Plugins:")
            For Each item In CurrentPluginManager.Plugins
                Console.WriteLine($"{item.PluginName} ({item.GetType.GetTypeInfo.Assembly.FullName})")
            Next
        End Sub
    End Class
End Namespace