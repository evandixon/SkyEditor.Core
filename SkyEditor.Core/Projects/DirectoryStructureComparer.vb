Namespace Projects
    Public Class DirectoryStructureComparer
        Implements IComparer(Of String)

        Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
            Dim xParts = x.Replace("\", "/").Split("/")
            Dim yParts = y.Replace("\", "/").Split("/")

            For count = 0 To Math.Min(xParts.Length - 1, yParts.Length - 1)
                If xParts(count).ToLowerInvariant <> yParts(count).ToLowerInvariant Then
                    Return xParts(count).CompareTo(yParts(count))
                End If
            Next

            'If we get here, then we have two directories like:
            'Test/Ing
            'Test/Ing/Dir

            'We want to make sure Test/Ing comes before Test/Ing/Dir
            Return xParts.Length.CompareTo(yParts.Length)
        End Function
    End Class
End Namespace
