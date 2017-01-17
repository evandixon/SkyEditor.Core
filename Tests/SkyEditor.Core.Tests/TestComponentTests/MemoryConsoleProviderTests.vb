Imports SkyEditor.Core.TestComponents

Namespace TestComponentTests
    <TestClass> Public Class MemoryConsoleProviderTests
        Private Const ConsoleTestsCategory As String = "Console Tests"

        <TestMethod> <TestCategory(ConsoleTestsCategory)> Public Sub TestStdIn()
            Dim provider As New MemoryConsoleProvider

            provider.StdIn.Append("!Line ")
            provider.StdIn.AppendLine("1")
            provider.StdIn.AppendLine("Line 2")

            Assert.AreEqual(Convert.ToInt32("!"c), provider.Read)
            Assert.AreEqual("Line 1", provider.ReadLine)
            Assert.AreEqual("Line 2", provider.ReadLine)
        End Sub

        <TestMethod> <TestCategory(ConsoleTestsCategory)> Public Sub TestStdOut()
            Dim provider As New MemoryConsoleProvider

            provider.Write("Test".ToCharArray)
            provider.Write("ing ")
            provider.Write(Me)
            provider.Write(" ")
            provider.Write("asserts {0} {1} ", "All", "Tests")
            provider.Write(True)
            provider.Write("This is .  Garbage string".ToCharArray, 8, 2)
            provider.WriteLine()
            provider.WriteLine("Test line".ToCharArray)
            provider.WriteLine("Test line 2")
            provider.WriteLine("Test {0}", "formatted line")
            provider.WriteLine(Me)
            provider.WriteLine("Testing a specific substring.".ToCharArray, 8, 20)

            Dim meString = Me.ToString
            Assert.AreEqual(String.Format("Testing {0} asserts All Tests True. " & vbCrLf &
                            "Test line" & vbCrLf &
                            "Test line 2" & vbCrLf &
                            "Test formatted line" & vbCrLf &
                            "{0}" & vbCrLf &
                            "a specific substring", meString) & vbCrLf, provider.StdOut.ToString)
        End Sub
    End Class
End Namespace