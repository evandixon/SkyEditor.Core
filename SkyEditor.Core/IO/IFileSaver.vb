Namespace IO

    ''' <summary>
    ''' Saves a model to a file
    ''' </summary>
    Public Interface IFileSaver

        ''' <summary>
        ''' Determines whether or not the current IFileSaver supports saving the given model without a filename.
        ''' </summary>
        ''' <param name="model">Model to save.</param>
        ''' <returns>A boolean indicating whether or nto the current IFileSaver can save the given model without a filename.</returns>
        Function SupportsSave(model As Object) As Boolean

        ''' <summary>
        ''' Determines whether or not the current IFileSaver supports saving the given model with a filename.
        ''' </summary>
        ''' <param name="model">Model to save.</param>
        ''' <returns>A boolean indicating whether or nto the current IFileSaver can save the given model with a filename.</returns>
        Function SupportsSaveAs(model As Object) As Boolean

        ''' <summary>
        ''' Saves the model to disk.
        ''' </summary>
        ''' <param name="model">Model to save.</param>
        ''' <param name="provider">Instance of the current IOProvider</param>
        Function Save(model As Object, provider As IOProvider) As Task

        ''' <summary>
        ''' Saves the model to a file at the given path.
        ''' </summary>
        ''' <param name="model">Model to save.</param>
        ''' <param name="filename">Full path of the file to which the model will be saved.</param>
        ''' <param name="provider">Instance of the current IOProvider</param>
        Function Save(model As Object, filename As String, provider As IOProvider) As Task

        ''' <summary>
        ''' Gets the default extension for the given model when using Save As.
        ''' Should only be called if the <see cref="SupportsSaveAs(Object)"/> returns true.
        ''' </summary>
        ''' <param name="model">Model of which to determine the default extension.</param>
        ''' <returns>A string representing the default extension.</returns>
        Function GetDefaultExtension(model As Object) As String

        ''' <summary>
        ''' Gets the supported extensions for the given model when using Save As.
        ''' </summary>
        ''' <param name="model">Model of which to determine the supported extensions.</param>
        ''' <returns>An IEnumerable that contains every extension that can be used to save this file.</returns>
        Function GetSupportedExtensions(model As Object) As IEnumerable(Of String)
    End Interface

End Namespace
