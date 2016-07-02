Imports System.Reflection
Imports SkyEditor.Core.UI
Imports SkyEditor.Core.Utilities

Namespace UI
    Public Class UIHelper

        'Prevent creating instances of this static class
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Gets the currently registered MenuActions in heiarchy form.
        ''' </summary>
        ''' <param name="pluginManager">Instance of the current plugin manager.</param>
        ''' <param name="isDevMode">Whether or not to get the dev-only menu items.</param>
        ''' <returns></returns>
        Private Shared Function GetMenuItemInfo(isContextBased As Boolean, target As Object, pluginManager As PluginManager, isDevMode As Boolean) As List(Of MenuItemInfo)
            If pluginManager Is Nothing Then
                Throw New ArgumentNullException(NameOf(pluginManager))
            End If

            Dim menuItems As New List(Of MenuItemInfo)
            For Each ActionInstance In pluginManager.GetRegisteredObjects(Of MenuAction)
                ActionInstance.CurrentPluginManager = pluginManager
                '1: If this is a context menu, only get actions that support the target and are context based
                '2: Ensure menu actions are only visible based on their environment: non-context in regular menu, context in context menu
                '3: DevOnly menu actions are only supported if we're in dev mode.
                If (Not isContextBased OrElse (ActionInstance.SupportsObject(target) AndAlso ActionInstance.IsContextBased)) AndAlso
                    (isContextBased = ActionInstance.IsContextBased) AndAlso
                    (isDevMode OrElse Not ActionInstance.DevOnly) Then

                    'Generate the MenuItem
                    If ActionInstance.ActionPath.Count >= 1 Then
                        'Create parent menu items
                        Dim parent = From m In menuItems Where m.Header = ActionInstance.ActionPath(0)

                        Dim current As MenuItemInfo
                        If parent.Any Then
                            current = parent.First
                            If current.ActionTypes.Count = 0 Then
                                current.SortOrder = Math.Min(current.SortOrder, ActionInstance.SortOrder)
                            End If
                        Else
                            Dim m As New MenuItemInfo
                            m.Header = ActionInstance.ActionPath(0)
                            m.Children = New List(Of MenuItemInfo)
                            m.ActionTypes = New List(Of TypeInfo)
                            m.SortOrder = ActionInstance.SortOrder
                            If ActionInstance.ActionPath.Count = 1 Then
                                m.ActionTypes.Add(ActionInstance.GetType.GetTypeInfo)
                            End If
                            menuItems.Add(m)
                            current = m
                        End If


                        For count = 1 To ActionInstance.ActionPath.Count - 2
                            Dim index = count 'To avoid potential issues with using the below linq expression.  Might not be needed, but it's probably best to avoid potential issues.
                            parent = From m As MenuItemInfo In current.Children Where m.Header = ActionInstance.ActionPath(index)
                            If parent.Any Then
                                current = parent.First
                                If current.ActionTypes.Count = 0 Then
                                    current.SortOrder = Math.Min(current.SortOrder, ActionInstance.SortOrder)
                                End If
                            Else
                                Dim m As New MenuItemInfo
                                m.Header = ActionInstance.ActionPath(count)
                                m.Children = New List(Of MenuItemInfo)
                                m.SortOrder = ActionInstance.SortOrder
                                If count = 0 Then
                                    menuItems.Add(m)
                                Else
                                    current.Children.Add(m)
                                End If
                                current = m
                            End If
                        Next


                        If ActionInstance.ActionPath.Count > 1 Then
                            'Check to see if the menu item exists
                            parent = From m As MenuItemInfo In current.Children Where m.Header = ActionInstance.ActionPath.Last

                            If parent.Any Then
                                Dim m = DirectCast(parent.First, MenuItemInfo)
                                m.ActionTypes = New List(Of TypeInfo)
                                m.ActionTypes.Add(ActionInstance.GetType.GetTypeInfo)
                            Else
                                'Add the menu item, and give it a proper tag
                                Dim m As New MenuItemInfo
                                m.Children = New List(Of MenuItemInfo)
                                m.Header = ActionInstance.ActionPath.Last
                                m.SortOrder = ActionInstance.SortOrder
                                m.ActionTypes = New List(Of TypeInfo)
                                m.ActionTypes.Add(ActionInstance.GetType.GetTypeInfo)
                                current.Children.Add(m)
                            End If
                        End If

                    Else 'Count=0
                        Throw New ArgumentException(My.Resources.Language.ErrorMenuActionEmptyActionPath)
                    End If
                End If
            Next
            Return menuItems
        End Function

        Public Shared Function GetMenuItemInfo(pluginManager As PluginManager, isDevMode As Boolean) As List(Of MenuItemInfo)
            Return GetMenuItemInfo(False, Nothing, pluginManager, isDevMode)
        End Function

        Public Shared Function GetContextMenuItemInfo(target As Object, pluginManager As PluginManager, isDevMode As Boolean) As List(Of MenuItemInfo)
            Return GetMenuItemInfo(True, target, pluginManager, isDevMode)
        End Function

        ''' <summary>
        ''' Generates MenuItems from the given IEnumerable of MenuItemInfo.
        ''' </summary>
        ''' <param name="MenuItemInfo">IEnumerable of MenuItemInfo that will be used to create the MenuItems.</param>
        ''' <param name="targets">Direct targets of the action, if applicable.  If Nothing, the IOUIManager will control the targets</param>
        ''' <returns></returns>
        Public Shared Function GenerateLogicalMenuItems(MenuItemInfo As IEnumerable(Of MenuItemInfo), ioui As IOUIManager, targets As IEnumerable(Of Object)) As List(Of ActionMenuItem)
            If MenuItemInfo Is Nothing Then
                Throw New ArgumentNullException(NameOf(MenuItemInfo))
            End If

            Dim output As New List(Of ActionMenuItem)

            'Create the menu items
            For Each item In From m In MenuItemInfo Order By m.SortOrder, m.Header
                Dim m As New ActionMenuItem '= ReflectionHelpers.CreateInstance(RootMenuItemType.GetTypeInfo)
                m.Header = item.Header
                m.CurrentIOUIManager = ioui
                m.ContextTargets = targets
                For Each action In item.ActionTypes
                    Dim a As MenuAction = ReflectionHelpers.CreateInstance(action)
                    a.CurrentPluginManager = ioui.CurrentPluginManager
                    m.Actions.Add(a)
                Next
                For Each child In GenerateLogicalMenuItems(item.Children, ioui, targets)
                    m.Children.Add(child)
                Next
                output.Add(m)
            Next

            Return output
        End Function

        ''' <summary>
        ''' Returns a new instance of each registered ObjectControl.
        ''' </summary>
        ''' <returns></returns>
        Private Shared Function GetObjectControls(Manager As PluginManager) As IEnumerable(Of IObjectControl)
            Return Manager.GetRegisteredObjects(Of IObjectControl)()
        End Function

        ''' <summary>
        ''' Gets an object control that can edit the given object.
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetObjectControl(model As Object, RequestedTabTypes As IEnumerable(Of Type), Manager As PluginManager) As IObjectControl
            Return GetRefreshedTabs(model, RequestedTabTypes, Manager).FirstOrDefault
        End Function

        ''' <summary>
        ''' Returns a list of iObjectControl that edit the given ObjectToEdit.
        ''' </summary>
        ''' <param name="model">Object the iObjectControl should edit.</param>
        ''' <param name="RequestedTabTypes">Limits what types of iObjectControl should be returned.  If the iObjectControl is not of any type in this IEnumerable, it will not be used.  If empty or nothing, no constraints will be applied, which is not recommended because the iObjectControl could be made for a different environment (for example, a Windows Forms user control being used in a WPF environment).</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetRefreshedTabs(model As Object, RequestedTabTypes As IEnumerable(Of Type), Manager As PluginManager) As IEnumerable(Of IObjectControl)
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            Dim modelType = model.GetType.GetTypeInfo
            Dim allTabs As New List(Of IObjectControl)

            Dim objControls As List(Of IObjectControl) = GetObjectControls(Manager)

            For Each etab In (From e In objControls Where RequestedTabTypes.Any(Function(t As Type) As Boolean
                                                                                    Return ReflectionHelpers.IsOfType(e, t.GetTypeInfo, False)
                                                                                End Function)
                              Order By e.GetSortOrder(modelType.AsType, True) Ascending)

                etab.SetPluginManager(Manager)
                Dim isMatch As Boolean = False
                Dim viewModel As GenericViewModel = Nothing

                'Check to see if the tab support the type of the given object
                Dim supportedTypes = etab.GetSupportedTypes

                For Each t In supportedTypes

                    Dim info As TypeInfo = t.GetTypeInfo
                    If info.IsInterface Then
                        'The target is an interface.  Check to see if there's a view model that implements it.
                        'Otherwise, check the model

                        Manager.CurrentConsoleProvider.WriteLine($"{etab.GetType.AssemblyQualifiedName} supports an interface.")

                        Dim viewmodelsForModel = Manager.CurrentIOUIManager.GetViewModelsForModel(model)

                        Dim availableViewModels As IEnumerable(Of GenericViewModel) = Nothing

                        If viewmodelsForModel IsNot Nothing Then
                            Manager.CurrentConsoleProvider.WriteLine("All view models:")
                            For Each item In viewmodelsForModel
                                Manager.CurrentConsoleProvider.WriteLine(item.GetType.AssemblyQualifiedName)
                            Next

                            availableViewModels = From v In viewmodelsForModel
                                                  Where ReflectionHelpers.IsOfType(v, info, False) AndAlso v.SupportsObject(model)

                            Manager.CurrentConsoleProvider.WriteLine("Available view models:")
                            For Each item In availableViewModels
                                Manager.CurrentConsoleProvider.WriteLine(item.GetType.AssemblyQualifiedName)
                            Next
                        ElseIf TypeOf model Is FileViewModel Then
                            availableViewModels = DirectCast(model, FileViewModel).GetViewModels(Manager)
                        End If

                        If availableViewModels IsNot Nothing AndAlso availableViewModels.Any Then
                            Manager.CurrentConsoleProvider.WriteLine("Available view models exist")
                            'This view model fits the critera
                            Dim first = availableViewModels.First
                            isMatch = etab.SupportsObject(first)
                            viewModel = first
                            If isMatch Then Exit For
                        ElseIf ReflectionHelpers.IsOfType(model, info, True) Then
                            'The model implements this interface
                            isMatch = etab.SupportsObject(model)
                            If isMatch Then Exit For
                        Else
                            Manager.CurrentConsoleProvider.WriteLine("Available view models do not exist")
                        End If

                    ElseIf ReflectionHelpers.IsOfType(model, t.GetTypeInfo, True) Then
                        'The model is the same type as the target
                        isMatch = etab.SupportsObject(model)
                        If isMatch Then Exit For

                        'Check to see if the 
                    ElseIf ReflectionHelpers.IsOfType(t, GetType(GenericViewModel).GetTypeInfo, False) Then
                        'The object control is targeting a view model

                        'First, check to see if there's any view models for this model (i.e., is this an open file?)
                        Dim viewmodelsForModel = Manager.CurrentIOUIManager.GetViewModelsForModel(model)

                        If viewmodelsForModel Is Nothing AndAlso TypeOf model Is FileViewModel Then
                            viewmodelsForModel = DirectCast(model, FileViewModel).GetViewModels(Manager)
                        End If

                        'If there are, check to see if the target view model supports the model
                        If viewmodelsForModel IsNot Nothing Then
                            Dim potentialViewModel As GenericViewModel = (From v In Manager.CurrentIOUIManager.GetViewModelsForModel(model)
                                                                          Where ReflectionHelpers.IsOfType(v, info, False) AndAlso v.SupportsObject(model)).FirstOrDefault
                            If potentialViewModel IsNot Nothing Then
                                'This view model supports our model
                                isMatch = etab.SupportsObject(potentialViewModel)
                                viewModel = potentialViewModel
                                If isMatch Then Exit For
                            End If
                        End If

                    End If
                Next

                'This is a supported tab.  We're adding it!
                If isMatch Then
                    'Create another instance of etab, since etab is our cached, search-only instance.
                    Dim tab As IObjectControl = ReflectionHelpers.CreateNewInstance(etab)
                    tab.SetPluginManager(Manager)

                    'Set the appropriate object
                    If viewModel IsNot Nothing Then
                        'We have a view model that the view wants
                        tab.EditingObject = viewModel
                    ElseIf tab.SupportsObject(model) Then
                        'This model is what the view wants
                        tab.EditingObject = model
                    Else
                        'This model is a container of what the view wants
                        For Each type In tab.GetSupportedTypes
                            If ReflectionHelpers.IsIContainerOfType(model, type.GetTypeInfo) Then
                                tab.EditingObject = ReflectionHelpers.GetIContainerContents(model, type)
                                Exit For
                            End If
                        Next
                    End If

                    allTabs.Add(tab)
                End If
            Next

            Dim backupTabs As New List(Of IObjectControl)
            Dim notBackup As New List(Of IObjectControl)

            'Sort the backup vs non-backup tabs
            For Each item In allTabs
                If item.IsBackupControl(model) Then
                    backupTabs.Add(item)
                Else
                    notBackup.Add(item)
                End If
            Next

            'And use the non-backup ones if available
            If notBackup.Count > 0 Then
                Return notBackup
            Else
                Dim toUse = (From b In backupTabs Order By b.GetSortOrder(modelType.AsType, True)).FirstOrDefault
                If toUse Is Nothing Then
                    Return {}
                Else
                    Return {toUse}
                End If
            End If
        End Function
    End Class
End Namespace

