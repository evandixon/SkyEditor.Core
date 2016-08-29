Imports SkyEditor.Core.IO
Imports SkyEditor.Core.UI

Namespace UI
    <TestClass>
    Public Class GenericViewModelTests
        Public Const TestCategory = "Generic View Model Tests"
        Public Property TestViewModel As GenericViewModel

#Region "Sibling View Models"
        Public Const SiblingViewModelTests = "Sibling View Model Tests"

#Region "Nested Types"
        Private Class StringViewModel1
            Inherits GenericViewModel(Of String)
        End Class

        Private Class StringViewModel2
            Inherits GenericViewModel(Of String)
        End Class

        Private Class SiblingViewModelCoreMod
            Inherits CoreSkyEditorPlugin

            Public Overrides ReadOnly Property Credits As String
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Overrides ReadOnly Property PluginAuthor As String
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Overrides ReadOnly Property PluginName As String
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Overrides Function GetExtensionDirectory() As String
                Throw New NotImplementedException()
            End Function

            Public Overrides Function GetIOProvider() As IOProvider
                Throw New NotImplementedException()
            End Function

            Public Overrides Function GetSettingsProvider(manager As PluginManager) As ISettingsProvider
                Throw New NotImplementedException()
            End Function

            Public Overrides Function IsPluginLoadingEnabled() As Boolean
                Return False
            End Function

            Public Overrides Sub Load(manager As PluginManager)
                MyBase.Load(manager)
                manager.RegisterType(Of GenericViewModel, StringViewModel1)()
                manager.RegisterType(Of GenericViewModel, StringViewModel2)()
            End Sub
        End Class
#End Region

        Private Sub InitSiblingVMTest()
            Dim testModel = "Test model"

            Dim m As New PluginManager
            m.LoadCore(New TestComponents.TestCoreMod).Wait()
            m.CurrentIOUIManager.OpenFile(testModel, False)
            TestViewModel = m.CurrentIOUIManager.GetViewModelsForModel(testModel).FirstOrDefault

            Assert.IsNotNull(TestViewModel, "Failed to set up test.")
        End Sub

        <TestMethod> <TestCategory(SiblingViewModelTests)>
        Public Sub HasSiblingViewModelFindsCorrectViewModels()
            InitSiblingVMTest()

            Assert.IsTrue(TestViewModel.HasSiblingViewModel(Of StringViewModel1), "Failed to find sibling view model of type StringViewModel1")
            Assert.IsTrue(TestViewModel.HasSiblingViewModel(Of StringViewModel2), "Failed to find sibling view model of type StringViewModel2")
        End Sub

        <TestMethod> <TestCategory(SiblingViewModelTests)>
        Public Sub HasSiblingViewModelReturnsFalseWithInvalidType()
            InitSiblingVMTest()

            Assert.IsFalse(TestViewModel.HasSiblingViewModel(Of TestComponents.TestViewModel), "Incorrectly identified TestViewModel as a sibling view model")
        End Sub

        <TestMethod> <TestCategory(SiblingViewModelTests)>
        Public Sub GetSiblingViewModelFindsCorrectViewModels()
            InitSiblingVMTest()

            Dim vm1 = TestViewModel.GetSiblingViewModel(Of StringViewModel1)
            Assert.IsNotNull(vm1)
            Assert.IsInstanceOfType(vm1, GetType(StringViewModel1))

            Dim vm2 = TestViewModel.GetSiblingViewModel(Of StringViewModel2)
            Assert.IsNotNull(vm2)
            Assert.IsInstanceOfType(vm2, GetType(StringViewModel2))
        End Sub
#End Region

    End Class
End Namespace