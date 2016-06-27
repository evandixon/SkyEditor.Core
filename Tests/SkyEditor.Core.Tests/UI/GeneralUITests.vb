Imports SkyEditor.Core.Tests.TestComponents

Namespace UI
    <TestClass> Public Class GeneralUITests
        Public Property CurrentIOUIManager As IOUIManager
        <TestInitialize> Public Sub Initialize()
            Dim manager As New PluginManager
            manager.LoadCore(New TestCoreMod).Wait()
            CurrentIOUIManager = manager.CurrentIOUIManager
        End Sub

        <TestMethod> Public Sub TestMenuItemVisibility()
            'Set up so that active model is a new text file

        End Sub
    End Class
End Namespace

