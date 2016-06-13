Imports SkyEditor.Core.Utilities

Namespace IO
    Public Interface ICreatableFile
        Inherits IOnDisk
        Inherits ISavable
        Inherits INamed
        Inherits IOpenableFile
        Sub CreateFile(Name As String)
    End Interface
End Namespace


