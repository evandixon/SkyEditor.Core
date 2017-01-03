﻿Namespace IO
    ''' <summary>
    ''' Marks a class that supports saving.
    ''' </summary>
    Public Interface ISavable
        ''' <summary>
        ''' Saves the class to the last filename.
        ''' </summary>
        Function Save(provider As IOProvider) As Task
        ''' <summary>
        ''' Raised when the file is saved.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Event FileSaved(sender As Object, e As EventArgs)
    End Interface

End Namespace
