Imports System.IO
Imports System.Threading.Tasks
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.settings

Namespace Extensions
    Public Class PluginExtensionType
        Inherits ExtensionType

        Public Overrides Function GetName() As Task(Of String)
            Return Task.FromResult("Plugins")
        End Function

        Protected Overrides ReadOnly Property InternalName As String
            Get
                Return "Plugins"
            End Get
        End Property

        Public Overrides Async Function InstallExtension(extensionID As Guid, TempDir As String) As Task(Of ExtensionInstallResult)
            Await MyBase.InstallExtension(extensionID, TempDir)
            Return ExtensionInstallResult.RestartRequired
        End Function

        ''' <summary>
        ''' Gets the directory that stores the files for the given extension ID.
        ''' </summary>
        ''' <param name="extensionID">ID of the extension for which to get the directory.</param>
        ''' <returns></returns>
        ''' <remarks>If the extensionID is an empty Guid, returns the plugin development directory.</remarks>
        Public Overrides Function GetExtensionDirectory(extensionID As Guid) As String
            If extensionID = Guid.Empty Then
                Return GetDevDirectory()
            Else
                Return MyBase.GetExtensionDirectory(extensionID)
            End If
        End Function

        ''' <summary>
        ''' Gets the plugin development directory.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function GetDevDirectory() As String
            Return Path.Combine(RootExtensionDirectory, InternalName, "Development")
        End Function

        Public Overrides Function GetInstalledExtensions(manager As PluginManager) As IEnumerable(Of ExtensionInfo)
            Dim extensions As New List(Of ExtensionInfo)
            extensions.AddRange(MyBase.GetInstalledExtensions(manager))
            If manager.CurrentSettingsProvider.GetIsDevMode Then
                'Load the development plugins
                Dim devDir = Path.Combine(RootExtensionDirectory, InternalName, "Development")
                Dim info As New ExtensionInfo
                info.ID = Guid.Empty
                info.Name = My.Resources.Language.PluginDevExtName
                info.Description = My.Resources.Language.PluginDevExtDescription
                info.Author = My.Resources.Language.PluginDevExtAuthor
                info.IsInstalled = True
                info.IsEnabled = True
                info.Version = My.Resources.Language.PluginDevExtVersion
                If manager.CurrentIOProvider.DirectoryExists(devDir) Then
                    For Each item In manager.CurrentIOProvider.GetFiles(devDir, "*.dll", True)
                        info.ExtensionFiles.Add(Path.GetFileName(item))
                    Next
                    For Each item In manager.CurrentIOProvider.GetFiles(devDir, "*.exe", True)
                        info.ExtensionFiles.Add(Path.GetFileName(item))
                    Next
                End If
                extensions.Add(info)
            End If
            Return extensions
        End Function

        ''' <summary>
        ''' Uninstalls the given extension.
        ''' </summary>
        ''' <param name="extensionID">ID of the extension to uninstall</param>
        Public Overrides Function UninstallExtension(extensionID As Guid) As Task(Of ExtensionUninstallResult)
            CurrentPluginManager.CurrentSettingsProvider.ScheduleDirectoryForDeletion(GetExtensionDirectory(extensionID), CurrentPluginManager.CurrentIOProvider)
            Return Task.FromResult(ExtensionUninstallResult.RestartRequired)
        End Function

    End Class

End Namespace
