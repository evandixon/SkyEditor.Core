Imports SkyEditor.Core.Projects

Namespace Projects
    <TestClass> Public Class ProjectTests

#Region "File System"
        Public Const ProjectFileSystem = "Project Base - Logical File System"

        Private Property DirectoryTestProject As TestProject

        Private Sub InitProjectDirectory()
            DirectoryTestProject = New TestProject
            If DirectoryTestProject.GetDirectories("", True).Count > 0 Then
                Assert.Inconclusive("Project already has directories")
            End If
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub CreateDirectory()
            InitProjectDirectory()

            DirectoryTestProject.CreateDirectory("/Test/Ing")

            Dim directories = DirectoryTestProject.GetDirectories("", True)
            Assert.AreEqual(2, directories.Count, "Incorrect number of directories")
            Assert.IsTrue(directories.Contains("/Test/Ing"), "Directory ""/Test/Ing"" not created.")
            Assert.IsTrue(directories.Contains("/Test"), "Parent directory ""/Test"" not automatically created.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DirectoryExists()
            InitProjectDirectory()

            DirectoryTestProject.CreateDirectory("/Test/Ing")

            Assert.IsTrue(DirectoryTestProject.DirectoryExists("/Test/Ing"))
            Assert.IsFalse(DirectoryTestProject.DirectoryExists("/Blarg"))
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DirectoryExistsNotFile()
            InitProjectDirectory()

            DirectoryTestProject.CreateFile("", "file.txt", GetType(TestCreatableFIle))

            Assert.IsFalse(DirectoryTestProject.DirectoryExists("/file.txt"), "/file.txt is not a directory.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DirectoryRoot()
            InitProjectDirectory()

            Assert.IsTrue(DirectoryTestProject.DirectoryExists(""), "Root directory """" should exist.")
            Assert.IsTrue(DirectoryTestProject.DirectoryExists("/"), "Root directory ""/"" should exist; slash should not matter.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DeleteDirectory()
            InitProjectDirectory()

            DirectoryTestProject.CreateDirectory("/Test/Ing")
            DirectoryTestProject.CreateDirectory("/Bla/rg")

            If DirectoryTestProject.GetDirectories("", True).Count <> 4 Then
                Assert.Inconclusive("Directories not created properly.")
            End If

            'Delete single directory
            DirectoryTestProject.DeleteDirectory("/Test/Ing")

            Dim directories = DirectoryTestProject.GetDirectories("", True)
            Assert.IsFalse(directories.Contains("/Test/Ing"), "Target directory not deleted.")
            Assert.IsTrue(directories.Contains("/Test"), "Parent directory ""/Test"" incorrectly deleted.")
            Assert.IsTrue(directories.Contains("/Bla/rg"), "Unrelated directory ""/Test"" incorrectly deleted.")
            Assert.IsTrue(directories.Contains("/Bla"), "Unrelated directory ""/Test"" incorrectly deleted.")
            Assert.AreEqual(3, directories.Count, "Incorrect number of directories")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DeleteDirectoryRecursiveDirectoryDelete()
            InitProjectDirectory()

            DirectoryTestProject.CreateDirectory("/Test/Ing")
            DirectoryTestProject.CreateDirectory("/Bla/rg")

            If DirectoryTestProject.GetDirectories("", True).Count <> 4 Then
                Assert.Inconclusive("Directories not created properly.")
            End If

            'Delete single directory
            DirectoryTestProject.DeleteDirectory("/Test")

            Dim directories = DirectoryTestProject.GetDirectories("", True)
            Assert.IsFalse(directories.Contains("/Test/Ing"), "Child directory ""/Test/Ing"" not deleted.")
            Assert.IsFalse(directories.Contains("/Test"), "Directory ""/Test"" not deleted.")
            Assert.IsTrue(directories.Contains("/Bla/rg"), "Unrelated directory ""/Test"" incorrectly deleted.")
            Assert.IsTrue(directories.Contains("/Bla"), "Unrelated directory ""/Test"" incorrectly deleted.")
            Assert.AreEqual(2, directories.Count, "Incorrect number of directories")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DeleteDirectoryRecursiveItemDelete()
            InitProjectDirectory()

            DirectoryTestProject.CreateDirectory("/Test/Ing")
            DirectoryTestProject.CreateDirectory("/Bla/rg")
            DirectoryTestProject.CreateFile("/Test/Ing", "file.txt", GetType(TestCreatableFIle))

            If Not DirectoryTestProject.FileExists("/Test/Ing/file.txt") Then
                Assert.Inconclusive("File not properly created.")
            End If

            If DirectoryTestProject.GetDirectories("", True).Count <> 4 Then
                Assert.Inconclusive("Directories not created properly.")
            End If

            'Delete single directory
            DirectoryTestProject.DeleteDirectory("/Test")

            Dim directories = DirectoryTestProject.GetDirectories("", True)
            Assert.IsFalse(directories.Contains("/Test/Ing"), "Child directory ""/Test/Ing"" not deleted.")
            Assert.IsFalse(directories.Contains("/Test"), "Directory ""/Test"" not deleted.")
            Assert.IsTrue(directories.Contains("/Bla/rg"), "Unrelated directory ""/Test"" incorrectly deleted.")
            Assert.IsTrue(directories.Contains("/Bla"), "Unrelated directory ""/Test"" incorrectly deleted.")
            Assert.AreEqual(2, directories.Count, "Incorrect number of directories")

            Assert.IsFalse(DirectoryTestProject.FileExists("/Test/Ing/file.txt"), "File not recursively deleted.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub CreateFile()
            InitProjectDirectory()
            DirectoryTestProject.CreateFile("/Test/Ing", "file.txt", GetType(TestCreatableFIle))
            Assert.IsTrue(DirectoryTestProject.FileExists("/Test/Ing/file.txt"), "File not created.")

            Dim directories = DirectoryTestProject.GetDirectories("", True)
            Assert.IsTrue(directories.Contains("/Test/Ing"), "Child directory ""/Test/Ing"" not deleted.")
            Assert.IsTrue(directories.Contains("/Test"), "Directory ""/Test"" not deleted.")
            Assert.AreEqual(2, directories.Count, "Incorrect number of directories")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub DeleteFile()
            InitProjectDirectory()
            DirectoryTestProject.CreateFile("/Test/Ing", "file.txt", GetType(TestCreatableFIle))
            If Not DirectoryTestProject.FileExists("/Test/Ing/file.txt") Then
                Assert.Inconclusive("File not properly created.")
            End If

            DirectoryTestProject.DeleteFile("/Test/Ing/file.txt")

            Assert.IsFalse(DirectoryTestProject.FileExists("/Test/Ing/file.txt"), "File not deleted.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub GetDirectories()
            InitProjectDirectory()

            DirectoryTestProject.CreateDirectory("/Test/Ing")

            Dim directories = DirectoryTestProject.GetDirectories("/Test", True)
            Assert.AreEqual(1, directories.Count, "Incorrect number of directories")
            Assert.IsTrue(directories.Contains("/Test/Ing"), "Directory ""/Test/Ing"" not created.")
            Assert.IsFalse(directories.Contains("/Test"), "Parent directory ""/Test"" not automatically created.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub GetItems()
            InitProjectDirectory()
            DirectoryTestProject.CreateFile("/Test/Ing", "file.txt", GetType(TestCreatableFIle))
            If Not DirectoryTestProject.FileExists("/Test/Ing/file.txt") Then
                Assert.Inconclusive("File not properly created.")
            End If

            Dim items = DirectoryTestProject.GetItems("/Test/Ing", True)
            Assert.IsTrue(items.ContainsKey("/Test/Ing/file.txt"), "Child directory ""/Test/Ing"" not returned.")
            Assert.AreEqual(1, items.Count, "Incorrect number of files")

            Assert.IsTrue(items.Keys.ToArray.SequenceEqual(DirectoryTestProject.GetItems("", True).Keys.ToArray), "Failed to get all items when using the root.")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub GetItemsNonRecursive()
            InitProjectDirectory()
            DirectoryTestProject.CreateFile("/Test/Ing", "file.txt", GetType(TestCreatableFIle))
            DirectoryTestProject.CreateFile("/Test", "file.txt", GetType(TestCreatableFIle))
            If Not DirectoryTestProject.FileExists("/Test/file.txt") OrElse Not DirectoryTestProject.FileExists("/Test/Ing/file.txt") Then
                Assert.Inconclusive("Test data not properly created.")
            End If

            Dim items = DirectoryTestProject.GetItems("/Test", False)
            Assert.IsTrue(items.ContainsKey("/Test/file.txt"), "Child directory ""/Test/Ing"" not returned.")
            Assert.AreEqual(1, items.Count, "Incorrect number of files")
        End Sub

        <TestMethod> <TestCategory(ProjectFileSystem)> Public Sub GetItemsNonRecursiveFromRoot()
            InitProjectDirectory()
            DirectoryTestProject.CreateFile("/Test/Ing", "file.txt", GetType(TestCreatableFIle))
            DirectoryTestProject.CreateFile("", "file.txt", GetType(TestCreatableFIle))
            If Not DirectoryTestProject.FileExists("/file.txt") OrElse Not DirectoryTestProject.FileExists("/Test/Ing/file.txt") Then
                Assert.Inconclusive("Test data not properly created.")
            End If

            Dim items = DirectoryTestProject.GetItems("", False)
            Assert.IsTrue(items.ContainsKey("/file.txt"), "Child directory ""/Test/Ing"" not returned.")
            Assert.AreEqual(1, items.Count, "Incorrect number of files")
        End Sub

#End Region

#Region "Building"
        Public Const ProjectBuild = "Project Base - Building"

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
#End Region

    End Class
End Namespace