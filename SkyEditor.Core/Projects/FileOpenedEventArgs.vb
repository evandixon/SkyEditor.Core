Imports SkyEditor.Core.UI

Namespace Projects
    Public Class FileOpenedEventArguments
        Inherits EventArgs

        Public Sub New()
            DisposeOnExit = False
        End Sub

        ''' <summary>
        ''' The model representing the file that was added.
        ''' </summary>
        Public Property File As Object

        ''' <summary>
        ''' The <see cref="UI.FileViewModel"/> wrapping <see cref="File"/>.
        ''' </summary>
        Public Property FileViewModel As FileViewModel

        ''' <summary>
        ''' Whether or not the file will be disposed when closed.
        ''' </summary>
        Public Property DisposeOnExit As Boolean

        ''' <summary>
        ''' The project the file was opened from.
        ''' Null if the file is not in a project.
        ''' </summary>
        Public Property ParentProject As Project

    End Class

End Namespace