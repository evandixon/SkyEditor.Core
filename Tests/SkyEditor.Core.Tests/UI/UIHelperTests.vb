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
        End Sub

        <TestMethod> Public Sub GetRefreshedTabsTests()
            'Todo: test throwing exception when model is null
            'Todo: test RequestedTabTypes
            'Todo: test throwing exception when manager is null

            Dim tabs = UIHelper.GetRefreshedTabs(Model, {GetType(Object)}, CurrentPluginManager)
            Assert.AreEqual(2, tabs.Count())
        End Sub
    End Class
End Namespace
