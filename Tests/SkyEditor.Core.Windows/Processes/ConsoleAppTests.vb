Imports Microsoft.QualityTools.Testing.Fakes
Imports SkyEditor.Core.Windows.Processes

Namespace Processes
    <TestClass>
    Public Class ConsoleAppTests
        <TestMethod> Public Sub RunProgramNoOutputTest()
            Using ShimsContext.Create
                '' Todo: figure out how to make fakes work
                'Dim p1 = New System.Diagnostics.Fakes.ShimProcess
                'p1.StartInfoGet = Function()
                '                      Return New ProcessStartInfo
                '                  End Function

                'p1.Start = Function()
                '               Return True
                '           End Function

                'p1.WaitForExit = Sub()
                '                 End Sub

                'p1.ProcessNameGet = Function() "blarg"

                'System.Diagnostics.Fakes.ShimProcess.Constructor = Function()
                '                                                       Return p1
                '                                                   End Function

                ConsoleApp.RunProgramNoOutput("ping.exe", "").Wait()
            End Using
        End Sub

        <TestMethod> Public Sub RunProgramTest()
            Using ShimsContext.Create
                '' Todo: figure out how to make fakes work
                ConsoleApp.RunProgram("ping.exe", "").Wait()
            End Using
        End Sub
    End Class
End Namespace
