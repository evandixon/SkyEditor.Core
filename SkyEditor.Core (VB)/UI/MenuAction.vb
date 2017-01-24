Imports System.Reflection
Imports System.Threading.Tasks
Imports SkyEditor.Core.Utilities

Namespace UI
    Public MustInherit Class MenuAction

        Public Sub New(path As IEnumerable(Of String))
            AlwaysVisible = False
            ActionPath = New List(Of String)
            ActionPath.AddRange(path)
            DevOnly = False
            SortOrder = Integer.MaxValue
        End Sub

        Public Event CurrentPluginManagerChanged(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Whether or not the menu item appears in context menus.
        ''' </summary>
        ''' <returns></returns>
        Public Property IsContextBased As Boolean

        ''' <summary>
        ''' Names representing the action's location in a heiarchy of menu items.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ActionPath As List(Of String)

        ''' <summary>
        ''' The current instance of the plugin manager
        ''' </summary>
        ''' <returns></returns>
        Public Property CurrentPluginManager As PluginManager
            Get
                Return _currentPluginManager
            End Get
            Set
                If Value IsNot _currentPluginManager Then
                    _currentPluginManager = Value
                    RaiseEvent CurrentPluginManagerChanged(Me, New EventArgs)
                End If
            End Set
        End Property
        Dim _currentPluginManager As PluginManager

        '''' <summary>
        '''' True to target all open files and the current project.
        '''' False to target only the selected file and the current project.
        '''' </summary>
        '''' <returns></returns>
        'Public Property TargetAll As Boolean

        ''' <summary>
        ''' True to be visible regardless of current targets.
        ''' False to be dependant on MenuAction.SupportsObjects.
        ''' </summary>
        ''' <returns></returns>
        Public Property AlwaysVisible As Boolean
            Get
                Return _alwaysVisible
            End Get
            Protected Set
                _alwaysVisible = Value
            End Set
        End Property
        Dim _alwaysVisible As Boolean

        ''' <summary>
        ''' True to be visible only when Development Mode is enabled and normal visibility conditions are satisfied.
        ''' False to be visibile as normal.
        ''' </summary>
        ''' <returns></returns>
        Public Property DevOnly As Boolean
            Get
                Return _devOnly
            End Get
            Protected Set
                _devOnly = Value
            End Set
        End Property
        Dim _devOnly As Boolean

        ''' <summary>
        ''' Order in which menu items are sorted
        ''' </summary>
        ''' <returns></returns>
        Public Property SortOrder As Decimal
            Get
                Return _sortOrder
            End Get
            Protected Set
                _sortOrder = Value
            End Set
        End Property
        Dim _sortOrder As Decimal

        ''' <summary>
        ''' IEnumerable of types the action can be performed with.
        ''' If empty, can be performed on any type.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function SupportedTypes() As IEnumerable(Of TypeInfo)
            Return {}
        End Function

        ''' <summary>
        ''' Determines whether or not the given object is supported.
        ''' </summary>
        ''' <param name="obj">Object to determine if it is supported.</param>
        ''' <returns>A boolean indicating whether or not <paramref name="obj"/> is supported.</returns>
        Public Overridable Function SupportsObject(obj As Object) As Task(Of Boolean)
            If obj Is Nothing Then
                Return Task.FromResult(AlwaysVisible)
            Else
                Dim q = From t In SupportedTypes() Where ReflectionHelpers.IsOfType(obj.GetType, t)

                Return Task.FromResult(q.Any)
            End If
        End Function

        ''' <summary>
        ''' Determines whether or not the combination of given objects is supported.
        ''' </summary>
        ''' <param name="objects"><see cref="IEnumerable(Of Object)"/> to determine if they are supported.</param>
        ''' <returns>A boolean indicating whether or not the given combination of objects is supported.</returns>
        Public Overridable Async Function SupportsObjects(objects As IEnumerable(Of Object)) As Task(Of Boolean)
            For Each item In objects
                If Await SupportsObject(item) Then
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' Executes the logical function of the current <see cref="MenuAction"/>.
        ''' </summary>
        ''' <param name="targets">Targets of the <see cref="MenuAction"/>.</param>
        Public MustOverride Sub DoAction(targets As IEnumerable(Of Object))

    End Class

End Namespace