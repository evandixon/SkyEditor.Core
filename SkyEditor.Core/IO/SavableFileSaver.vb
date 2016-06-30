Namespace IO
    ''' <summary>
    ''' A class that can save models that implement ISavable or ISavableAs
    ''' </summary>
    Public Class SavableFileSaver
        Implements IFileSaver

        Public Function GetDefaultExtension(model As Object) As String Implements IFileSaver.GetDefaultExtension
            Return DirectCast(model, ISavableAs).GetDefaultExtension()
        End Function

        Public Function Save(model As Object, provider As IOProvider) As Task Implements IFileSaver.Save
            DirectCast(model, ISavable).Save(provider)
            Return Task.FromResult(0)
        End Function

        Public Function Save(model As Object, filename As String, provider As IOProvider) As Task Implements IFileSaver.Save
            DirectCast(model, ISavableAs).Save(filename, provider)
            Return Task.FromResult(0)
        End Function

        Public Function SupportsSave(model As Object) As Boolean Implements IFileSaver.SupportsSave
            Return TypeOf model Is ISavable
        End Function

        Public Function SupportsSaveAs(model As Object) As Boolean Implements IFileSaver.SupportsSaveAs
            Return TypeOf model Is ISavableAs
        End Function
    End Class
End Namespace

