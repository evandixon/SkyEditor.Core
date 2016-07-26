Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports SkyEditor
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.Utilities

Namespace Extensions
    Public Class ExtensionHelper
        Implements iNamed

        Public ReadOnly Property Name As String Implements iNamed.Name
            Get
                Return "Extensions"
            End Get
        End Property

        Private Shared Property ExtensionBanks As Dictionary(Of String, ExtensionType)

        ''' <summary>
        ''' Gets the <see cref="ExtensionType"/> with the given type name.
        ''' </summary>
        ''' <param name="extensionTypeName">Name of the type of the extension.</param>
        ''' <param name="manager">Instance of the current plugin manager.</param>
        ''' <returns>An instance of <see cref="ExtensionType"/> corresponsing to <paramref name="extensionTypeName"/>, or null if it cannot be found.</returns>
        Public Shared Function GetExtensionBank(extensionTypeName As String, manager As PluginManager) As ExtensionType
            If Not ExtensionBanks.ContainsKey(extensionTypeName) Then
                Dim extensionType = ReflectionHelpers.GetTypeByName(extensionTypeName, manager)
                If extensionType IsNot Nothing Then
                    Dim bank As ExtensionType = ReflectionHelpers.CreateInstance(extensionType)
                    bank.CurrentPluginManager = manager
                    ExtensionBanks.Add(extensionTypeName, bank)
                Else
                    Return Nothing
                End If
            End If
            Return ExtensionBanks(extensionTypeName)
        End Function


        Public Shared Function IsExtensionInstalled(info As ExtensionInfo, manager As PluginManager) As Boolean
            Dim extensionType = ReflectionHelpers.GetTypeByName(info.ExtensionTypeName, manager)
            If extensionType Is Nothing Then
                Return False
            Else
                Dim bank As ExtensionType = GetExtensionBank(info.ExtensionTypeName, manager)
                Return bank.GetInstalledExtensions(manager).Where(Function(x) x.ID = info.ID).Any()
            End If
        End Function

        Public Shared Async Function InstallExtensionZip(extensionZipPath As String, manager As PluginManager) As Task(Of ExtensionInstallResult)
            Dim provider = manager.CurrentIOProvider
            Dim result As ExtensionInstallResult

            'Get the temporary directory
            Dim tempDir = provider.GetTempDirectory

            'Ensure it contains no files
            Await Core.Utilities.FileSystem.ReCreateDirectory(tempDir, provider).ConfigureAwait(False)

            'Extract the given zip file to it
            Core.Utilities.Zip.Unzip(extensionZipPath, tempDir)

            'Open the info file
            Dim infoFilename As String = Path.Combine(tempDir, "info.skyext")
            If provider.FileExists(infoFilename) Then
                'Open the file itself
                Dim info = ExtensionInfo.OpenFromFile(infoFilename, provider)
                'Get the type
                Dim extType = GetExtensionBank(info.ExtensionTypeName, manager)
                'Determine if the type is supported
                If extType Is Nothing Then
                    result = ExtensionInstallResult.UnsupportedFormat
                Else
                    result = Await extType.InstallExtension(info.ID, tempDir).ConfigureAwait(False)
                End If
            Else
                result = ExtensionInstallResult.InvalidFormat
            End If

            'Cleanup
            Await Core.Utilities.FileSystem.DeleteDirectory(tempDir, provider).ConfigureAwait(False)

            Return result
        End Function

        Public Shared Async Function UninstallExtension(extensionTypeName As String, extensionID As Guid, manager As PluginManager) As Task(Of ExtensionUninstallResult)
            Dim bank As ExtensionType = GetExtensionBank(extensionTypeName, manager)
            Return Await bank.UninstallExtension(extensionID, manager).ConfigureAwait(False)
        End Function

        Public Shared Function GetExtensions(extensionTypeName As String, skip As Integer, take As Integer, manager As PluginManager) As IEnumerable(Of ExtensionInfo)
            Dim bank As ExtensionType = GetExtensionBank(extensionTypeName, manager)
            Return bank.GetInstalledExtensions(skip, take, manager)
        End Function
    End Class
End Namespace