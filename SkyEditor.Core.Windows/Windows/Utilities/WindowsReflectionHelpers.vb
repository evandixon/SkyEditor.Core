Imports System.IO
Imports System.Reflection
Imports SkyEditor.Core
Imports SkyEditor.Core.Extensions
Imports SkyEditor.Core.Utilities

Namespace Windows.Utilities
    Public Class WindowsReflectionHelpers

        Public Shared Function IsSupportedPlugin(filename As String) As Boolean
            Dim isSupported As Boolean
            'We're going to load these assemblies into another appdomain, so we don't accidentally create duplicates, and so we don't keep any unneeded assemblies loaded for the life of the application.
            Using reflectionManager As New AssemblyReflectionManager

                reflectionManager.LoadAssembly(filename, "PluginManagerAnalysis")

                Dim pluginInfoNames As New List(Of String)

                Try
                    pluginInfoNames =
                        reflectionManager.Reflect(filename,
                                                  Function(a As Assembly, Args() As Object) As List(Of String)
                                                      Dim out As New List(Of String)

                                                      If a IsNot Nothing AndAlso
                                                        Not (a.FullName = Assembly.GetCallingAssembly.FullName OrElse
                                                                (Assembly.GetEntryAssembly IsNot Nothing AndAlso a.FullName = Assembly.GetEntryAssembly.FullName) OrElse
                                                                a.FullName = Assembly.GetExecutingAssembly.FullName) Then
                                                          For Each t As Type In a.GetTypes
                                                              Dim isPlg As Boolean = ReflectionHelpers.IsOfType(t, GetType(SkyEditorPlugin).GetTypeInfo) AndAlso ReflectionHelpers.CanCreateInstance(t)
                                                              If isPlg Then
                                                                  out.Add(t.FullName)
                                                              End If
                                                          Next
                                                      End If

                                                      Return out
                                                  End Function)
                Catch ex As Reflection.ReflectionTypeLoadException
                    'If we fail here, then the assembly is NOT a valid plugin, so we won't load it.
                    Console.WriteLine(ex.ToString)
                Catch ex As FileNotFoundException
                    'If we fail here, then the assembly is missing some of its references, meaning it's not a valid plugin.
                    Console.WriteLine(ex.ToString)
                End Try

                If pluginInfoNames.Count > 0 Then
                    'Then we want to keep this assembly
                    isSupported = True
                End If
            End Using 'The reflection appdomain will be unloaded on dispose
            Return isSupported
        End Function

        ''' <summary>
        ''' Gets the full paths of all of the referenced assemblies of the <paramref name="sourceAssembly"/>.
        ''' </summary>
        ''' <param name="sourceAssembly">Assembly of which to get the dependencies.</param>
        ''' <returns>A list containing all of the file paths of all dependency assemblies, both direct and indirect.</returns>
        ''' <remarks>Only returns assemblies in the same directory as the <paramref name="sourceAssembly"/>.
        ''' 
        ''' A side effect that may cause issues is that any referenced assembly that is not in the current app domain will be loaded.</remarks>
        Public Shared Function GetAssemblyDependencies(sourceAssembly As Assembly) As List(Of String)
            Dim out As New List(Of String)
            Dim devAssemblyPaths As New List(Of String)
            Dim workingDirectory = Path.GetDirectoryName(sourceAssembly.Location)
            devAssemblyPaths.AddRange(Directory.GetFiles(workingDirectory, "*.dll"))
            devAssemblyPaths.AddRange(Directory.GetFiles(workingDirectory, "*.exe"))

            'Get the Sky Editor Plugin's resource directory
            Dim resourceDirectory = Path.Combine(Path.GetDirectoryName(sourceAssembly.Location), Path.GetFileNameWithoutExtension(sourceAssembly.Location))
            If Directory.Exists(resourceDirectory) Then
                out.Add(resourceDirectory)
            End If

            'Get regional resources
            Dim resourcesName = Path.GetFileNameWithoutExtension(sourceAssembly.Location) & ".resources.dll"
            For Each item In Directory.GetDirectories(Path.GetDirectoryName(sourceAssembly.Location))
                If File.Exists(Path.Combine(item, resourcesName)) Then
                    out.Add(Path.Combine(item, resourcesName))
                End If
            Next

            'Look at the dependencies
            For Each reference In sourceAssembly.GetReferencedAssemblies
                Dim isLocal As Boolean = False
                'Try to find the filename of this reference
                For Each source In devAssemblyPaths
                    Dim name = AssemblyName.GetAssemblyName(source)
                    If reference.Name = name.Name Then
                        If Not out.Contains(source) Then
                            out.Add(source)
                            isLocal = True
                            Exit For
                        End If
                    End If
                Next

                If isLocal Then
                    'Try to find the references of this reference
                    Dim q = (From a In AppDomain.CurrentDomain.GetAssemblies Where a.FullName = reference.FullName).FirstOrDefault

                    If q IsNot Nothing Then
                        out.AddRange(GetAssemblyDependencies(q))
                    Else
                        'Then this reference isn't in the app domain.
                        'Let's try to find the assembly.
                        'Todo: it would be optimal to do this in another Appdomain, but since this assembly would be loaded if needed, there's no real harm
                        For Each source In devAssemblyPaths
                            Dim name = AssemblyName.GetAssemblyName(source)
                            If reference.FullName = name.FullName Then
                                Dim a = LoadAssembly(source)
                                If a IsNot Nothing Then
                                    out.AddRange(GetAssemblyDependencies(a))
                                End If
                            End If
                        Next
                    End If
                End If
            Next

            Return out
        End Function

        ''' <summary>
        ''' Loads the assembly at the given path, if there's not another version in the AppDomain
        ''' </summary>
        ''' <param name="assemblyPath">Path of the assembly to load</param>
        ''' <returns>The assembly at the given path, or another assembly with the same full name.</returns>
        ''' <remarks>If the target assembly is a different version of an already loaded assembly, one that's already loaded will be returned instead.</remarks>
        Public Shared Function LoadAssembly(assemblyPath As String) As Assembly
            'First, check to see if we already loaded it
            Dim name As AssemblyName = AssemblyName.GetAssemblyName(assemblyPath)
            Dim q1 = From a In AppDomain.CurrentDomain.GetAssemblies Where a.GetName.Name = name.Name

            If q1.Any Then
                'If we did, then there's no point in loading it again.  In some cases, it could cause more problems
                Return q1.First
            Else
                'If we didn't, then load it
                If WindowsReflectionHelpers.IsSupportedPlugin(assemblyPath) Then
                    Dim loadedAssembly = Assembly.LoadFrom(assemblyPath)

                    'Relying on the side effect of this function to load all dependant assemblies into the current app domain
                    WindowsReflectionHelpers.GetAssemblyDependencies(loadedAssembly)

                    Return loadedAssembly
                Else
                    Return Nothing
                End If
            End If
        End Function
    End Class
End Namespace