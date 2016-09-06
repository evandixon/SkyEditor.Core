Imports SkyEditor.Core.Projects

Namespace Projects
    <TestClass> Public Class DirectoryStructureComparerTests
        Public Const DirectoryStructerComparerCategory = "Directory Structure Comparer"

        <TestMethod> <TestCategory(DirectoryStructerComparerCategory)> Public Sub FirstLevelCharacterTest()
            Dim c As New DirectoryStructureComparer

            Dim alphabet = "abcdefghijklmnopqrstuvwxyz1234567890"
            For Each c1 In alphabet
                For Each c2 In alphabet
                    Assert.AreEqual(String.Compare(c1, c2), c.Compare(c1, c2), $"Characters ""{c1}"" and ""{c2}"" not properly compared.")
                Next
            Next
        End Sub

        <TestMethod> <TestCategory(DirectoryStructerComparerCategory)> Public Sub CaseInsensitivityTest()
            Dim c As New DirectoryStructureComparer

            Dim upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            Dim lower = "abcdefghijklmnopqrstuvwxyz"
            For count = 0 To upper.Length - 1
                Dim c1 = upper(count)
                Dim c2 = lower(count)
                Assert.AreEqual(0, c.Compare(c1, c2), $"Characters ""{c1}"" and ""{c2}"" should be treated the same.")
            Next
        End Sub

        <TestMethod> <TestCategory(DirectoryStructerComparerCategory)> Public Sub SortTest()
            Dim testList As New List(Of String)
            testList.Add("/a/a/b")
            testList.Add("/a/b/y")
            testList.Add("/a/b/r")
            testList.Add("/a/a/w")
            testList.Add("/a/a/c")
            testList.Add("/a/a/x")
            testList.Add("/a/a/q")
            testList.Add("/a/b/z")
            testList.Add("/b/2")
            testList.Add("/b")
            testList.Add("/a/a/b")
            testList.Add("/a/a/y")
            testList.Add("/a/a/r")
            testList.Add("/a/b/w")
            testList.Add("/a/a/c")
            testList.Add("/a/a/x")
            testList.Add("/a/b/q")
            testList.Add("/a/a/z")
            testList.Sort(New DirectoryStructureComparer)

            Assert.AreEqual("/a/a/b", testList(0))
            Assert.AreEqual("/a/a/b", testList(1))
            Assert.AreEqual("/a/a/c", testList(2))
            Assert.AreEqual("/a/a/c", testList(3))
            Assert.AreEqual("/a/a/q", testList(4))
            Assert.AreEqual("/a/a/r", testList(5))
            Assert.AreEqual("/a/a/w", testList(6))
            Assert.AreEqual("/a/a/x", testList(7))
            Assert.AreEqual("/a/a/x", testList(8))
            Assert.AreEqual("/a/a/y", testList(9))
            Assert.AreEqual("/a/a/z", testList(10))
            Assert.AreEqual("/a/b/q", testList(11))
            Assert.AreEqual("/a/b/r", testList(12))
            Assert.AreEqual("/a/b/w", testList(13))
            Assert.AreEqual("/a/b/y", testList(14))
            Assert.AreEqual("/a/b/z", testList(15))
            Assert.AreEqual("/b", testList(16))
            Assert.AreEqual("/b/2", testList(17))
        End Sub
    End Class
End Namespace

