﻿Imports System.Reflection
Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.Windows.ConsoleCommands
Imports SkyEditor.Core.Windows.Providers
Imports SkyEditor.Core.Windows.Utilities

Namespace CoreMods
    Public MustInherit Class WindowsCoreSkyEditorPlugin
        Inherits CoreSkyEditorPlugin

        Public Overrides Function GetIOProvider() As SkyEditor.Core.IO.IOProvider
            Return New WindowsIOProvider
        End Function

        Public Overrides Function GetConsoleProvider() As IConsoleProvider
            Return New WindowsConsoleProvider
        End Function

        Public Overrides Sub Load(manager As PluginManager)
            MyBase.Load(manager)

            manager.RegisterTypeRegister(GetType(ConsoleCommandAsync))
            manager.RegisterType(GetType(ConsoleCommandAsync), GetType(DistPrep))
            manager.RegisterType(GetType(ConsoleCommandAsync), GetType(GeneratePluginExtensions))
            manager.CurrentIOUIManager.RegisterIOFilter("*.skysln", My.Resources.Language.SkyEditorSolution)
        End Sub

        Public Overrides Function IsPluginLoadingEnabled() As Boolean
            Return True
        End Function

        Public Overrides Function LoadAssembly(assemblyPath As String) As Assembly
            'First, check to see if we already loaded it
            Dim name As AssemblyName = AssemblyName.GetAssemblyName(assemblyPath)
            Dim q1 = From a In AppDomain.CurrentDomain.GetAssemblies Where a.FullName = name.FullName

            If q1.Any Then
                'If we did, then there's no point in loading it again.  In some cases, it could cause more problems
                Return q1.First
            Else
                'If we didn't, then load it
                If WindowsReflectionHelpers.IsSupportedPlugin(assemblyPath) Then
                    Return Assembly.LoadFrom(assemblyPath)
                Else
                    Return Nothing
                End If
            End If
        End Function

        Public Overrides Function GetSettingsProvider(manager As SkyEditor.Core.PluginManager) As ISettingsProvider
            Return SettingsProvider.Open(EnvironmentPaths.GetSettingsFilename, manager)
        End Function

        Public Overrides Function GetExtensionDirectory() As String
            Return EnvironmentPaths.GetExtensionDirectory
        End Function

    End Class

End Namespace
