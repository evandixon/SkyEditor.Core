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
        Dim items = CurrentIOUIManager.RootMenuItems
        'Test to see if there's 1
        Assert.AreEqual(1, items.Count, 0, "Incorrect number of root menu items.")
        'Test to see if CurrentPluginManager is not null
        For Each item In items
            For Each menuAction In item.Actions
                Assert.IsNotNull(menuAction.CurrentPluginManager)
            Next
        Next
    End Sub
End Class
