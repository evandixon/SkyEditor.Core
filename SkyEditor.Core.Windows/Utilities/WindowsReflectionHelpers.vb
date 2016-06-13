Imports System.IO
Imports System.Reflection
Imports SkyEditor.Core
Imports SkyEditor.Core.Utilities

Namespace Utilities
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
    End Class
End Namespace