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

            'Ensure it works when the model isn't an open file
            Dim model2 As New TextFile
            tabs = UIHelper.GetRefreshedTabs(model2, {GetType(Object)}, CurrentPluginManager)
            Assert.AreEqual(1, tabs.Count(), 0, "Incorrect number of tabs for model that's not in IOUIManager.OpenFiles.  Expected 1 view: directly binding to model")

            'Ensure it works on a FileViewModel that is not an open file
            Dim fvm As New FileViewModel(New TextFile)
            tabs = UIHelper.GetRefreshedTabs(fvm, {GetType(Object)}, CurrentPluginManager)
            Assert.AreEqual(2, tabs.Count(), 0, "Incorrect number of tabs for FileViewModel that's not in IOUIManager.OpenFiles.  Expected 2 views: directly binding to view model, binding to interface implemented by view model")
        End Sub
    End Class
End Namespace