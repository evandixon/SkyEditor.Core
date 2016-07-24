Imports System.Reflection
Imports SkyEditor.Core.UI

Namespace IO
    ''' <summary>
    ''' Models a node in the solution's logical heiarchy.
    ''' </summary>
    Public Class SolutionNode
        Inherits ProjectNodeBase
        Implements INotifyPropertyChanged

        Public Sub New(parentSolution As Solution, parentNode As SolutionNode)
            MyBase.New(parentSolution, parentNode)
            Children = New ObservableCollection(Of ProjectNodeBase)
        End Sub

        Public Shadows Property Item As Project
            Get
                Return MyBase.Item
            End Get
            Set(value As Project)
                MyBase.Item = value
            End Set
        End Property

        Public Overrides Function CreateChildDirectory(directoryName As String) As ProjectNodeBase
            If CanCreateChildDirectory() Then
                Dim node As New SolutionNode(ParentProject, Me)
                node.Name = Name
                Children.Add(node)
                Return node
            Else
                Return Nothing
            End If
        End Function

        Public Function CanCreateChildProject() As Boolean
            Return Me.IsDirectory AndAlso TypeOf ParentProject Is Solution AndAlso DirectCast(ParentProject, Solution).CanCreateProject(GetCurrentPath)
        End Function

        Public Function CanDeleteCurrentNode() As Boolean
            If Me.IsDirectory Then
                Return ParentProject.CanDeleteDirectory(GetCurrentPath)
            ElseIf TypeOf ParentProject Is Solution Then
                Return DirectCast(ParentProject, Solution).CanDeleteProject(GetCurrentPath)
            Else
                Return False
            End If
        End Function

        Public Async Function CreateChildProject(name As String, type As Type, manager As PluginManager) As Task
            If CanCreateChildProject() AndAlso TypeOf ParentProject Is Solution Then
                Await DirectCast(ParentProject, Solution).CreateProject(GetCurrentPath, name, type, manager)
            End If
        End Function

        Public Sub DeleteCurrentNode()
            If CanDeleteCurrentNode() AndAlso ParentNode IsNot Nothing Then
                ParentNode.Children.Remove(Me)
            End If
        End Sub

    End Class
End Namespace