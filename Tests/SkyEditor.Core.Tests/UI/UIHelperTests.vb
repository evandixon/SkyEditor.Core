Imports SkyEditor.Core.Tests.TestComponents
Imports SkyEditor.Core.UI

Namespace UI
    <TestClass()> Public Class UIHelperTests
        Public Property CurrentPluginManager As PluginManager
        Public Property Model As TextFile
        <TestInitialize> Public Sub Initialize()
            CurrentPluginManager = New PluginManager()
            CurrentPluginManager.LoadCore(New TestCoreMod).Wait()

            Model = New TextFile
            Model.Text = "Test"

            CurrentPluginManager.CurrentIOUIManager.OpenFile(Model, False)
        End Sub

        <TestMethod> Public Sub GetRefreshedTabsTests()
            'Todo: test throwing exception when model is null
            'Todo: test RequestedTabTypes
            'Todo: test throwing exception when manager is null

            'Test expected number of views
            Dim tabs = UIHelper.GetRefreshedTabs(Model, {GetType(Object)}, CurrentPluginManager)
            Assert.AreEqual(3, tabs.Count(), 0, "Expected 3 views: directly binding to model, binding to view model binding to model, binding to interface implemented by view model binding to model")
        End Sub
    End Class
End Namespace
