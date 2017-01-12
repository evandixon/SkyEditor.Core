Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace Projects
    Public Class TestCreatableFIle
        Implements ICreatableFile

        Public Property Filename As String Implements IOnDisk.Filename

        Public Property Name As String Implements INamed.Name

        Public Event FileSaved As ISavable.FileSavedEventHandler Implements ISavable.FileSaved

        Public Sub CreateFile(Name As String) Implements ICreatableFile.CreateFile
            Me.Name = Name
        End Sub

        Public Function Save(provider As IIOProvider) As Task Implements ISavable.Save
            Throw New NotImplementedException()
        End Function

        Public Function OpenFile(Filename As String, Provider As IIOProvider) As Task Implements IOpenableFile.OpenFile
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace

