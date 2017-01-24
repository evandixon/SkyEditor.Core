Imports SkyEditor.Core.Utilities

Namespace ConsoleCommands.UI
    Public Class ViewFileIndex
        Inherits ConsoleCommand

        Public Overrides Sub Main(arguments() As String)
            Dim file = CurrentPluginManager.CurrentIOUIManager.OpenFiles(arguments(0))
            Console.WriteLine($"Title: {file.Title}")
            Console.WriteLine($"Filename: {file.Filename}")
            Console.WriteLine($"Modified: {file.IsFileModified}")
            Dim vm = file.GetViewModels(CurrentPluginManager)
            Console.WriteLine($"{vm.Count()} View Models:")
            For count = 0 To vm.Count - 1
                Console.WriteLine($"{count}: {ReflectionHelpers.GetTypeFriendlyName(vm(count).GetType)}")
            Next
        End Sub
    End Class
End Namespace