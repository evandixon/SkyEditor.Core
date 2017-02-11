Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports SkyEditor
Imports SkyEditor.Core
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.Utilities
Imports SkyEditor.Core.Windows

Namespace Utilities

    'Legacy code to deal with the old manor of handling plugins, some of which will still be used for plugin development.
    Public Class RedistributionHelpers
        Public Shared Event ApplicationRestartRequested(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Runs the PrepareForDistribution method in all plugins, deleting any files that aren't distribution safe.
        ''' </summary>
        ''' <param name="Manager"></param>
        Public Shared Sub PrepareForDistribution(Manager As PluginManager)
            For Each item In Manager.GetPlugins
                item.PrepareForDistribution(Manager)
            Next
        End Sub

        ''' <summary>
        ''' Packs the given plugin into a zip file.
        ''' </summary>
        ''' <param name="Plugins">Definitions of the plugins to pack.</param>
        ''' <param name="DestinationFilename">File path of the zip to create.</param>
        ''' <returns></returns>
        Public Shared Async Function PackPlugins(Plugins As IEnumerable(Of SkyEditorPlugin), DestinationFilename As String, Info As ExtensionInfo, manager As PluginManager) As Task
            Dim tempDir = Path.Combine(Environment.CurrentDirectory, "PackageTemp" & Guid.NewGuid.ToString)
            Dim ToCopy As New List(Of String)

            Dim devDir As String
            'Todo: replace Path.Combine(manager.ExtensionDirectory, "Plugins", "Development") with something not hard coded
            If Directory.Exists(Path.Combine(manager.ExtensionDirectory, "Plugins", "Development")) Then
                devDir = Path.Combine(manager.ExtensionDirectory, "Plugins", "Development")
            Else
                devDir = Path.GetDirectoryName(GetType(RedistributionHelpers).Assembly.Location)
            End If

            For Each plugin In Plugins
                Dim plgAssembly = plugin.GetType.Assembly
                Dim filename = Path.GetFileNameWithoutExtension(plgAssembly.Location)

                'Prepare the plugin for distribution
                Dim plg = (From p In manager.GetPlugins Where p.GetType.Assembly.Location = plgAssembly.Location).FirstOrDefault
                If plg IsNot Nothing Then
                    plg.PrepareForDistribution(manager)
                Else
                    'Then the assembly isn't currently loaded.  In this case, we'll load it and tell it to prepare for distribution.
                    Using reflector As New AssemblyReflectionManager
                        reflector.LoadAssembly(plgAssembly.Location, "PackPlugin")
                        reflector.Reflect(plgAssembly.Location, Function(CurrentAssembly As Assembly, Args() As Object) As Object
                                                                    For Each result In From t In CurrentAssembly.GetTypes Where ReflectionHelpers.IsOfType(t, GetType(SkyEditorPlugin).GetTypeInfo) AndAlso t.GetConstructor({}) IsNot Nothing
                                                                        Dim def As SkyEditorPlugin = result.GetConstructor({}).Invoke({})
                                                                        def.PrepareForDistribution(manager)
                                                                    Next
                                                                    Return Nothing
                                                                End Function)
                    End Using
                End If

                'Find the files we should pack
                ToCopy.Add(plgAssembly.Location)

                'Try to detect dependencies.
                For Each item In WindowsReflectionHelpers.GetAssemblyDependencies(plgAssembly)
                    If Not ToCopy.Contains(item) Then
                        ToCopy.Add(item)
                    End If
                Next
            Next

            'Copy temporary files
            Await Core.Utilities.FileSystem.EnsureDirectoryEmpty(tempDir, manager.CurrentIOProvider).ConfigureAwait(False)
            For Each filePath In ToCopy
                If File.Exists(filePath) Then
                    Dim dest = filePath.Replace(devDir, tempDir)
                    If Not Directory.Exists(Path.GetDirectoryName(dest)) Then
                        Directory.CreateDirectory(Path.GetDirectoryName(dest))
                    End If
                    If filePath <> dest Then
                        File.Copy(filePath, dest, True)
                    End If
                Else
                    'It's probably a directory.
                    If Directory.Exists(filePath) Then
                        Await Core.Utilities.FileSystem.CopyDirectory(filePath, filePath.Replace(Path.GetDirectoryName(filePath), tempDir), manager.CurrentIOProvider).ConfigureAwait(False)
                        'Else
                        'Guess not.  Do nothing.
                    End If
                End If
            Next

            'Create the extension info file
            Info.ExtensionTypeName = GetType(PluginExtensionType).AssemblyQualifiedName
            Info.IsEnabled = True
            For Each item In Plugins
                Info.ExtensionFiles.Add(Path.GetFileName(item.GetType.Assembly.Location))
            Next
            Info.Save(Path.Combine(tempDir, "info.skyext"), manager.CurrentIOProvider)

            'Then zip it
            Await Zip.ZipDir(tempDir, DestinationFilename, manager.CurrentIOProvider)
            manager.CurrentIOProvider.DeleteDirectory(tempDir)
        End Function

        ''' <summary>
        ''' Restarts the application.
        ''' </summary>
        Public Shared Sub RequestRestartProgram()
            RaiseEvent ApplicationRestartRequested(Nothing, New EventArgs)
        End Sub

    End Class

End Namespace