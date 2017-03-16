Imports System.Reflection
Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace CoreMods
    Public MustInherit Class WindowsCoreSkyEditorPlugin
        Inherits CoreSkyEditorPlugin

        Public Overrides Sub Load(manager As PluginManager)
            MyBase.Load(manager)

            manager.RegisterType(Of ConsoleCommand, DistPrep)()
            manager.RegisterType(Of ConsoleCommand, GeneratePluginExtensions)()
        End Sub

        Public Overrides Function LoadAssembly(assemblyPath As String) As Assembly
            Return WindowsReflectionHelpers.LoadAssembly(assemblyPath)
        End Function

    End Class

End Namespace