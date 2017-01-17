Namespace IO
    Public Interface IOpenableFile
        Function OpenFile(Filename As String, Provider As IIOProvider) As Task
    End Interface

End Namespace