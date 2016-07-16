Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SkyEditor.Core.Windows.ConsoleCommands

Namespace Utilities
    <TestClass()> Public Class RedistributionHelpers

        Public Property CurrentPluginManager As PluginManager

        <TestInitialize>
        Public Sub InitTest()
            CurrentPluginManager = New PluginManager
            CurrentPluginManager.LoadCore(New PluginDefinition)
        End Sub

        <TestMethod()>
        <TestCategory(PluginDefinition.IntegrationTestCategory)>
        Public Sub GeneratePluginExtensions()
            Dim consoleCommand As New GeneratePluginExtensions
            consoleCommand.CurrentPluginManager = CurrentPluginManager
            consoleCommand.MainAsync({}).Wait()

            'Todo: assert
        End Sub

    End Class
End Namespace
