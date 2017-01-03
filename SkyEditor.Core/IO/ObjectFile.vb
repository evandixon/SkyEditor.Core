Imports System.Reflection
Imports System.Threading.Tasks
Imports SkyEditor.Core.Utilities

Namespace IO
    Friend Class JsonContainer(Of U)
        Public Property ContainedObject As U

        Public Property ContainedTypeName As String
    End Class

    <Obsolete> Public Class ObjectFile
        Implements INamed
        Implements IOpenableFile
        Implements ISavableAs
        Implements IOnDisk
        Implements ICreatableFile

        Public Sub New()
        End Sub

        Public Sub New(FileProvider As IOProvider)
            Me.CurrentIOProvider = FileProvider
        End Sub

        Public Sub New(FileProvider As IOProvider, Filename As String)
            Me.CurrentIOProvider = FileProvider
            Me.OpenFileInternal(Of Object)(Filename)
        End Sub

        Public Overridable Sub CreateFile(Name As String) Implements ICreatableFile.CreateFile
            ContainedObject = New Object
            Me.Name = Name
        End Sub

        Public Overridable Function OpenFile(Filename As String, Provider As IOProvider) As Task Implements IOpenableFile.OpenFile
            Me.CurrentIOProvider = Provider
            OpenFileInternal(Of Object)(Filename)
            Return Task.FromResult(0)
        End Function

        Protected Overridable Sub OpenFileInternal(Of T)(Filename As String)
            Me.Filename = Filename

            If CurrentIOProvider.FileExists(Filename) Then
                Dim c = Json.DeserializeFromFile(Of JsonContainer(Of T))(Filename, CurrentIOProvider)
                Me.ContainedObject = c.ContainedObject
                Me.ContainedTypeName = c.ContainedTypeName
            Else
                Me.ContainedObject = ReflectionHelpers.CreateInstance(GetType(T).GetTypeInfo)
                Me.ContainedTypeName = GetType(T).AssemblyQualifiedName
            End If
        End Sub

#Region "Properties"
        Public Property ContainedObject As Object

        Public Property ContainedTypeName As String

        Public Property Filename As String Implements IOnDisk.Filename

        Public Property CurrentIOProvider As IOProvider

        Public Property Name As String Implements INamed.Name
            Get
                If String.IsNullOrEmpty(Filename) Then
                    Return _name
                Else
                    Return Path.GetFileName(Filename)
                End If
            End Get
            Protected Set(value As String)
                _name = value
            End Set
        End Property
        Dim _name As String
#End Region

#Region "ISaveable support"

        Public Overridable Function Save(filename As String, provider As IOProvider) As Task Implements ISavableAs.Save
            Dim c As New JsonContainer(Of Object)
            c.ContainedObject = Me.ContainedObject
            c.ContainedTypeName = Me.GetType.AssemblyQualifiedName
            Json.SerializeToFile(filename, c, provider)
            RaiseFileSaved(Me, New EventArgs)
            Return Task.FromResult(0)
        End Function

        Public Async Function Save(provider As IOProvider) As Task Implements ISavable.Save
            Await Save(Me.Filename, provider)
        End Function

        Public Event FileSaved(sender As Object, e As EventArgs) Implements ISavable.FileSaved
        Protected Sub RaiseFileSaved(sender As Object, e As EventArgs)
            RaiseEvent FileSaved(sender, e)
        End Sub

#End Region

        Public Shared Function GetGenericTypeDefinition() As Type
            Return GetType(ObjectFile(Of Object)).GetGenericTypeDefinition
        End Function

        Public Shared Function IsObjectFile(TypeToCheck As TypeInfo) As Boolean
            Return (TypeToCheck.IsGenericType AndAlso TypeToCheck.GetGenericTypeDefinition.Equals(GetGenericTypeDefinition)) OrElse (Not TypeToCheck.BaseType.Equals(GetType(Object)) AndAlso IsObjectFile(TypeToCheck.BaseType.GetTypeInfo))
        End Function

        Public Overridable Function GetDefaultExtension() As String Implements ISavableAs.GetDefaultExtension
            Return Nothing
        End Function

        Public Function GetSupportedExtensions() As IEnumerable(Of String) Implements ISavableAs.GetSupportedExtensions
            Return Nothing
        End Function
    End Class

    <Obsolete> Public Class ObjectFile(Of T)
        Inherits ObjectFile

        Public Shadows Property ContainedObject As T
            Get
                Return MyBase.ContainedObject
            End Get
            Set(value As T)
                MyBase.ContainedObject = value
            End Set
        End Property

        Public Sub New()
        End Sub

        Public Sub New(FileProvider As IOProvider)
            Me.CurrentIOProvider = FileProvider
        End Sub

        Public Sub New(FileProvider As IOProvider, Filename As String)
            Me.CurrentIOProvider = FileProvider
            Me.OpenFileInternal(Of T)(Filename)
        End Sub

        Public Overrides Sub CreateFile(Name As String)
            If ReflectionHelpers.CanCreateInstance(GetType(T).GetTypeInfo) Then
                ContainedObject = ReflectionHelpers.CreateInstance(GetType(T).GetTypeInfo)
                Me.Name = Name
            End If
        End Sub

        Public Overrides Function OpenFile(Filename As String, Provider As IOProvider) As Task
            Me.CurrentIOProvider = Provider
            OpenFileInternal(Of T)(Filename)
            Return Task.FromResult(0)
        End Function

        Public Overrides Function Save(Filename As String, provider As IOProvider) As Task
            Dim c As New JsonContainer(Of T)
            c.ContainedObject = Me.ContainedObject
            c.ContainedTypeName = Me.GetType.AssemblyQualifiedName
            Json.SerializeToFile(Filename, c, provider)
            RaiseFileSaved(Me, New EventArgs)
            Return Task.FromResult(0)
        End Function

    End Class
End Namespace

