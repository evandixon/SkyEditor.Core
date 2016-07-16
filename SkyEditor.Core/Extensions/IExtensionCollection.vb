Imports System.Threading.Tasks
Imports SkyEditor.Core.IO

Namespace Extensions
    Public Interface IExtensionCollection
        ReadOnly Property Name As String
        Function GetChildCollections(manager As PluginManager) As Task(Of IEnumerable(Of IExtensionCollection))
        Function GetExtensions(skip As Integer, take As Integer, manager As PluginManager) As Task(Of IEnumerable(Of ExtensionInfo))
        Function GetExtensionCount(manager As PluginManager) As Task(Of Integer)
        Function InstallExtension(extensionID As Guid) As Task(Of ExtensionInstallResult)
        Function UninstallExtension(extensionID As Guid) As Task(Of ExtensionUninstallResult)
    End Interface
End Namespace

