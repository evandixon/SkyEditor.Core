Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Utilities

Namespace Utilities
    <TestClass()> Public Class ReflectionHelpersTests
        Public Class TestContainerClass(Of T)
            Implements IContainer(Of T)

            Public Property Item As T Implements IContainer(Of T).Item
        End Class
        <TestMethod> Public Sub IsOfTypeTests()
            'Standard equality
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(ReflectionHelpersTests), GetType(ReflectionHelpersTests), False), "Failed to see type equality (ReflectionHelpersTests is of type ReflectionHelpersTests)")
            'Interface checks
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(GenericFile), GetType(IOpenableFile), False), "Failed to see interface IOpenableFile on GenericFile")
            'Inheritance tests
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(CoreSkyEditorPlugin), GetType(SkyEditorPlugin), False), "Failed to see CoreSkyEditorPlugin inherits SkyEditorPlugin")
            'Make sure it returns false sometimes
            Assert.IsFalse(ReflectionHelpers.IsOfType(GetType(String), GetType(Integer), False), "Failed to see String is not of type Integer")

            'Repeat the above tests with the "Check Container" flag enabled
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(ReflectionHelpersTests), GetType(ReflectionHelpersTests), True), "checkContainer breaks standard equality (ReflectionHelpersTests is of type ReflectionHelpersTests)")
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(GenericFile), GetType(IOpenableFile), True), "checkContainer breaks interface check (interface IOpenableFile on GenericFile)")
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(CoreSkyEditorPlugin), GetType(SkyEditorPlugin), True), "checkContainer breaks inheritance check (CoreSkyEditorPlugin inherits SkyEditorPlugin)")
            Assert.IsFalse(ReflectionHelpers.IsOfType(GetType(String), GetType(Integer), True), "checkContainer breaks inequality check (String is not of type Integer)")

            'Actually test the "Check Container" flag
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(TestContainerClass(Of String)), GetType(String), True), "checkContainer failed to see TestContainerClass as a container of String")

            'Make sure it returns false when "Check Container" flag is false
            Assert.IsFalse(ReflectionHelpers.IsOfType(GetType(TestContainerClass(Of String)), GetType(String), False), "checkContainer saw TestContainerClass as a container of String, when told not to check container")
        End Sub

        <TestMethod> Public Sub IsIContainerOfTypeTests()
            'Check for True
            Assert.IsTrue(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of String), GetType(String)), "Failed to see container of String")
            Assert.IsTrue(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of Guid), GetType(Guid)), "Failed to see container of Guid")
            Assert.IsTrue(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of ReflectionHelpersTests), GetType(ReflectionHelpersTests)), "Failed to see container of ReflectionHelpersTests")
            Assert.IsTrue(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of Integer), GetType(Integer)), "Failed to see container of Integer.")

            'Check for False
            Assert.IsFalse(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of String), GetType(TestMethodAttribute)), "Incorrectly container of String")
            Assert.IsFalse(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of Guid), GetType(TestMethodAttribute)), "Incorrectly container of Guid")
            Assert.IsFalse(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of ReflectionHelpersTests), GetType(TestMethodAttribute)), "Incorrectly container of ReflectionHelpersTests")
            Assert.IsFalse(ReflectionHelpers.IsIContainerOfType(New TestContainerClass(Of Integer), GetType(TestMethodAttribute)), "Incorrectly container of Integer.")
        End Sub
    End Class
End Namespace

