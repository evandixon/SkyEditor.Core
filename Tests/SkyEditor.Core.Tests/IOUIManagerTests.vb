Imports SkyEditor.Core.Tests.TestComponents

<TestClass> Public Class IOUIManagerTests
    Public Property CurrentIOUIManager As IOUIManager
    <TestInitialize> Public Sub Initialize()
        Dim m As New PluginManager
        m.LoadCore(New TestCoreMod).Wait()
        CurrentIOUIManager = m.CurrentIOUIManager
        Assert.IsNotNull(CurrentIOUIManager)
    End Sub

    <TestMethod> Public Sub RootMenuItemsTests()
        'Test always visible
        Dim items = From m In CurrentIOUIManager.RootMenuItems Where m.IsVisible = True
        Assert.AreEqual(1, items.Count, 0, "Expected only 1 always visible menu item.")

        'Test targeted
        Dim file As New TextFile
        CurrentIOUIManager.OpenFile(file, False)
        CurrentIOUIManager.ActiveContent = file
        items = From m In CurrentIOUIManager.RootMenuItems Where m.IsVisible = True
        Assert.AreEqual(3, items.Count(), 0, "Expected 3 visible menu item: the one that's always visible, the one targeting the model, and the one targeting the view model.")

        'Test to see if CurrentPluginManager is not null
        For Each item In items
            For Each menuAction In item.Actions
                Assert.IsNotNull(menuAction.CurrentPluginManager)
            Next
        Next
    End Sub
End Class
