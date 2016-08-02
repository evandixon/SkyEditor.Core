Imports SkyEditor.Core.Projects

Namespace Projects
    Public Class TestProject
        Inherits Project

        ''' <summary>
        ''' Starts a build that stays in the building status until <see cref="CompleteBuild()"/> is called.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Async Function Build() As Task
            BuildStatus = BuildStatus.Building
            IsBuildProgressIndeterminate = True
            BuildStatusMessage = "Waiting for completion..."
            Await Task.Run(Sub()
                               While True
                                   If Me.IsCancelRequested Then
                                       BuildStatus = BuildStatus.Canceled
                                       Exit While
                                   End If

                                   Select Case BuildStatus
                                       Case BuildStatus.Done
                                           Exit While
                                       Case BuildStatus.Building
                                           'Block
                                       Case BuildStatus.Failed
                                           Exit While
                                       Case Else
                                           Assert.Fail("Invalid build status: " & BuildStatus.ToString)
                                   End Select
                               End While
                           End Sub)
        End Function

        Public Overridable Sub CompleteBuild()
            BuildStatus = BuildStatus.Done
        End Sub

        Public Overridable Sub FailBuild()
            BuildStatus = BuildStatus.Failed
        End Sub
    End Class

End Namespace
