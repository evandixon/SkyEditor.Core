Imports System.Threading.Tasks
Imports SkyEditor
Imports SkyEditor.Core.Extensions

Namespace Extensions
    Public Class InstalledExtensionCollection
        Implements IExtensionCollection

        Public Function GetName() As Task(Of String) Implements IExtensionCollection.GetName
            Return Task.FromResult("Installed Extensions")
        End Function

        Public Function GetChildCollections(manager As PluginManager) As Task(Of IEnumerable(Of IExtensionCollection)) Implements IExtensionCollection.GetChildCollections
            Dim out As IEnumerable(Of IExtensionCollection) = manager.GetRegisteredObjects(Of ExtensionType)
            For Each item As ExtensionType In out
                item.CurrentPluginManager = manager
            Next
            Return Task.FromResult(out)
        End Function

        Public Function GetExtensionCount(manager As PluginManager) As Task(Of Integer) Implements IExtensionCollection.GetExtensionCount
            Return Task.FromResult(0)
        End Function

        Public Function GetExtensions(skip As Integer, take As Integer, manager As PluginManager) As Task(Of IEnumerable(Of ExtensionInfo)) Implements IExtensionCollection.GetExtensions
            Dim info As ExtensionInfo() = {}
            Return Task.FromResult(info.AsEnumerable)
        End Function

        Public Function InstallExtension(extensionID As String, version As String, manager As PluginManager) As Task(Of ExtensionInstallResult) Implements IExtensionCollection.InstallExtension
            Throw New NotSupportedException
        End Function

        Public Function UninstallExtension(extensionID As String, manager As PluginManager) As Task(Of ExtensionUninstallResult) Implements IExtensionCollection.UninstallExtension
            Throw New NotSupportedException
        End Function
    End Class

End Namespace
