Imports SkyEditor.Core.IO

Namespace TestComponents
    Public Class TextFile
        Implements IOpenableFile

        Public Function OpenFile(Filename As String, Provider As IOProvider) As Task Implements IOpenableFile.OpenFile
            Return Task.FromResult(Provider.ReadAllText(Filename))
        End Function

        Public Property Text As String
    End Class
End Namespace
