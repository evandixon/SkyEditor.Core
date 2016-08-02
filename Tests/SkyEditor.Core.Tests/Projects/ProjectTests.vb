Imports SkyEditor.Core.Projects

Namespace Projects
    <TestClass> Public Class ProjectTests
        Public Const ProjectBuild = "Project Building"

        <TestMethod> <TestCategory(ProjectBuild)> Public Sub TestBuildSuccess()
            Dim p As New TestProject

            Assert.AreEqual(BuildStatus.None, p.BuildStatus, "The build status should be None when not building.")

            Dim buildTask = p.Build

            Assert.AreEqual(BuildStatus.Building, p.BuildStatus, "The build status should be Building when building.")
            Assert.IsFalse(buildTask.IsCompleted, "The build should still be running.")

            p.CompleteBuild()
            buildTask.Wait()

            Assert.AreEqual(BuildStatus.Done, p.BuildStatus, "A successful build should have the Done status.")
            Assert.IsTrue(buildTask.IsCompleted, "The build should no longer be running.")
        End Sub

        <TestMethod> <TestCategory(ProjectBuild)> Public Sub TestBuildCancel()
            Dim p As New TestProject
            Dim buildTask = p.Build

            p.CancelBuild()
            buildTask.Wait()

            Assert.AreEqual(BuildStatus.Canceled, p.BuildStatus, "A canceled build should have the Canceled status.")
            Assert.IsTrue(buildTask.IsCompleted, "The build should no longer be running.")
        End Sub

        <TestMethod> <TestCategory(ProjectBuild)> Public Sub TestBuildFail()
            Dim p As New TestProject
            Dim buildTask = p.Build

            p.FailBuild()
            buildTask.Wait()

            Assert.AreEqual(BuildStatus.Failed, p.BuildStatus, "A failed build should have the Failed status.")
            Assert.IsTrue(buildTask.IsCompleted, "The build should no longer be running.")
        End Sub
    End Class
End Namespace

