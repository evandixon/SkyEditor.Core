Imports System.IO
Imports System.Reflection
Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.Utilities

Namespace ConsoleCommands
    Public Class GeneratePluginExtensions
        Inherits ConsoleCommand

        Public Overrides Async Function MainAsync(Arguments() As String) As Task
            Dim manager = CurrentApplicationViewModel.CurrentPluginManager
            With CurrentApplicationViewModel.CurrentPluginManager.CurrentIOProvider
                For Each item In manager.GetPlugins
                    Dim a = item.GetType.GetTypeInfo.Assembly
                    If (Not manager.IsAssemblyDependant(a)) OrElse Arguments.Contains("-dependant") Then
                        Dim info As New ExtensionInfo
                        info.Name = item.PluginName
                        info.Author = item.PluginAuthor
                        info.Version = a.GetName.Version.ToString
                        Dim workingPath = Path.Combine("Exported Plugins", a.GetName.Name & ".zip")
                        If Not .DirectoryExists(Path.GetDirectoryName(workingPath)) Then
                            .CreateDirectory(Path.GetDirectoryName(workingPath))
                        End If
                        Await RedistributionHelpers.PackPlugins({item}, workingPath, info, manager).ConfigureAwait(False)
                    End If
                Next
            End With
        End Function
    End Class

End Namespace