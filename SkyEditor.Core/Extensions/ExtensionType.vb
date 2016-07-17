Imports System.IO
Imports System.Threading.Tasks
Imports SkyEditor
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.IO

Namespace Extensions
    Public MustInherit Class ExtensionType
        Implements IExtensionCollection

        ''' <summary>
        ''' The user-friendly name of the extension type.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function GetName() As Task(Of String) Implements IExtensionCollection.GetName

        ''' <summary>
        ''' Gets or sets the directory the ExtensionType stores extensions in.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RootExtensionDirectory As String
            Get
                Return CurrentPluginManager.ExtensionDirectory
            End Get
        End Property

        Public Property CurrentPluginManager As PluginManager

        ''' <summary>
        ''' The internal name of the extension type used in paths.
        ''' </summary>
        ''' <returns></returns>
        Protected Overridable ReadOnly Property InternalName As String
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Overridable Function GetExtensionDirectory(extensionID As Guid) As String
            Return Path.Combine(RootExtensionDirectory, InternalName, extensionID.ToString)
        End Function

        Public Overridable Function GetInstalledExtensions(manager As PluginManager) As IEnumerable(Of ExtensionInfo)
            Dim out As New List(Of ExtensionInfo)

            'Todo: cache this so paging works more efficiently
            If manager.CurrentIOProvider.DirectoryExists(Path.Combine(RootExtensionDirectory, InternalName)) Then
                For Each item In manager.CurrentIOProvider.GetDirectories(Path.Combine(RootExtensionDirectory, InternalName), True)
                    If manager.CurrentIOProvider.FileExists(Path.Combine(item, "info.skyext")) Then
                        Dim e = ExtensionInfo.OpenFromFile(Path.Combine(item, "info.skyext"), manager.CurrentIOProvider)
                        e.IsInstalled = True
                        out.Add(e)
                    End If
                Next
            End If

            Return out
        End Function

        ''' <summary>
        ''' Lists the extensions that are currently installed.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function GetInstalledExtensions(skip As Integer, take As Integer, manager As PluginManager) As Task(Of IEnumerable(Of ExtensionInfo)) Implements IExtensionCollection.GetExtensions
            Return Task.FromResult(GetInstalledExtensions(manager).Skip(skip).Take(take))
        End Function

        Public Function GetExtensionCount(manager As Core.PluginManager) As Task(Of Integer) Implements IExtensionCollection.GetExtensionCount
            Return Task.FromResult(GetInstalledExtensions(manager).Count())
        End Function

        Private Function InstallExtension(extensionID As Guid, manager As PluginManager) As Task(Of ExtensionInstallResult) Implements IExtensionCollection.InstallExtension
            Throw New NotSupportedException("This IExtensionCollection lists extensions that are currently installed, not ones that can be installed, so this cannnot install extensions.")
        End Function

        ''' <summary>
        ''' Installs the extension that's stored in the given directory.
        ''' </summary>
        ''' <param name="TempDir">Temporary directory that contains the extension's files.</param>
        Public Overridable Async Function InstallExtension(extensionID As Guid, TempDir As String) As Task(Of ExtensionInstallResult)
            Await Core.Utilities.FileSystem.CopyDirectory(TempDir, GetExtensionDirectory(extensionID), CurrentPluginManager.CurrentIOProvider)
            Return ExtensionInstallResult.Success
        End Function

        ''' <summary>
        ''' Uninstalls the given extension.
        ''' </summary>
        ''' <param name="extensionID">ID of the extension to uninstall</param>
        Public Overridable Function UninstallExtension(extensionID As Guid, manager As PluginManager) As Task(Of ExtensionUninstallResult) Implements IExtensionCollection.UninstallExtension
            CurrentPluginManager.CurrentIOProvider.DeleteDirectory(GetExtensionDirectory(extensionID))
            Return Task.FromResult(ExtensionUninstallResult.Success)
        End Function

        Public Function GetChildCollections(manager As PluginManager) As Task(Of IEnumerable(Of IExtensionCollection)) Implements IExtensionCollection.GetChildCollections
            Throw New NotSupportedException
        End Function
    End Class
End Namespace

