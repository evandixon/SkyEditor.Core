Imports System.Reflection
Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace CoreMods
    Public MustInherit Class WindowsCoreSkyEditorPlugin
        Inherits CoreSkyEditorPlugin

        Public Overrides Function GetIOProvider() As IIOProvider
            Return New PhysicalIOProvider
        End Function

        Public Overrides Function GetConsoleProvider() As IConsoleProvider
            Return New StandardConsoleProvider
        End Function

        Public Overrides Sub Load(manager As PluginManager)
            MyBase.Load(manager)

            manager.RegisterType(Of ConsoleCommand, DistPrep)()
            manager.RegisterType(Of ConsoleCommand, GeneratePluginExtensions)()
        End Sub

        Public Overrides Function IsPluginLoadingEnabled() As Boolean
            Return True
        End Function

        Public Overrides Function LoadAssembly(assemblyPath As String) As Assembly
            Return WindowsReflectionHelpers.LoadAssembly(assemblyPath)
        End Function

        Public Overrides Function GetSettingsProvider(manager As PluginManager) As ISettingsProvider
            Return SettingsProvider.Open(EnvironmentPaths.GetSettingsFilename, manager)
        End Function

        Public Overrides Function GetExtensionDirectory() As String
            Return EnvironmentPaths.GetExtensionDirectory
        End Function

    End Class

End Namespace