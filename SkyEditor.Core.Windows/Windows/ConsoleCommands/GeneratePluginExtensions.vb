Imports System.IO
Imports System.Reflection
Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.Windows.Utilities

Namespace Windows.ConsoleCommands
    Public Class GeneratePluginExtensions
        Inherits ConsoleCommandAsync

        Public Overrides Async Function MainAsync(Arguments() As String) As Task
            With CurrentPluginManager.CurrentIOProvider
                For Each item In CurrentPluginManager.Plugins
                    Dim a = item.GetType.GetTypeInfo.Assembly
                    If (Not CurrentPluginManager.IsAssemblyDependant(a)) OrElse Arguments.Contains("-dependant") Then
                        Dim info As New ExtensionInfo
                        info.Name = item.PluginName
                        info.Author = item.PluginAuthor
                        info.Version = a.GetName.Version.ToString
                        Dim workingPath = Path.Combine("Exported Plugins", a.GetName.Name & ".zip")
                        If Not .DirectoryExists(Path.GetDirectoryName(workingPath)) Then
                            .CreateDirectory(Path.GetDirectoryName(workingPath))
                        End If
                        Await RedistributionHelpers.PackPlugins({item}, workingPath, info, CurrentPluginManager).ConfigureAwait(False)
                    End If
                Next
            End With
        End Function
    End Class

End Namespace
