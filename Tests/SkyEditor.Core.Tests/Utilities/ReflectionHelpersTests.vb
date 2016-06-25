Imports System.Reflection
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Tests.TestComponents
Imports SkyEditor.Core.Utilities

Namespace Utilities
    <TestClass()> Public Class ReflectionHelpersTests
        Public Class TestContainerClass(Of T)
            Implements IContainer(Of T)

            Public Property Item As T Implements IContainer(Of T).Item
        End Class
        Public Class TestContainerClassMulti
            Implements IContainer(Of String)
            Implements IContainer(Of Integer)

            Public Property StringItem As String Implements IContainer(Of String).Item

            Public Property IntegerItem As Integer Implements IContainer(Of Integer).Item

            Public Sub New()
                StringItem = "Test!!!"
                IntegerItem = 7
            End Sub
        End Class

        <TestMethod> Public Sub GetCachedInstanceTests()
            Dim type As TypeInfo = GetType(TestContainerClassMulti).GetTypeInfo
            Dim instance1 As TestContainerClassMulti = ReflectionHelpers.GetCachedInstance(type)
            Dim instance2 As TestContainerClassMulti = ReflectionHelpers.GetCachedInstance(type)
            Dim instance3 As New TestContainerClassMulti

            Assert.IsNotNull(instance1)
            Assert.IsNotNull(instance2)
            Assert.ReferenceEquals(instance1, instance2)
            Assert.IsTrue(instance1 IsNot instance3, "New instance should not have same reference of cached instance")
        End Sub

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

        <TestMethod> Public Sub GetIContainerContentsTests()
            'Test single container
            Dim container As New TestContainerClass(Of Guid)
            container.Item = Guid.NewGuid

            Assert.AreEqual(ReflectionHelpers.GetIContainerContents(container, GetType(Guid)), container.Item)

            'Test multi-container
            Dim container2 As New TestContainerClassMulti
            Assert.AreEqual(container2.StringItem, ReflectionHelpers.GetIContainerContents(container2, GetType(String)))
            Assert.AreEqual(container2.IntegerItem, ReflectionHelpers.GetIContainerContents(container2, GetType(Integer)))
        End Sub

        <TestMethod> Public Sub SetIContainerContentsTests()
            Dim container As New TestContainerClass(Of Guid)
            Dim guidTest = Guid.NewGuid
            ReflectionHelpers.SetIContainerContents(container, GetType(Guid), guidTest)
            Assert.AreEqual(guidTest, container.Item)

            Dim container2 As New TestContainerClassMulti

            ReflectionHelpers.SetIContainerContents(container2, GetType(String), guidTest.ToString)
            Assert.AreEqual(guidTest.ToString, container2.StringItem)

            Dim intTest = (New Random).Next(0, Integer.MaxValue)
            ReflectionHelpers.SetIContainerContents(container2, GetType(Integer), intTest)
            Assert.AreEqual(intTest, container2.IntegerItem)
        End Sub

        <TestMethod> Public Sub GetTypeByNameTests()
            'This one is a little hard to test due to the nature of reflection in a plugin-based environment
            'We will only test types that this function has no excuse to not be able to find

            Using manager As New PluginManager
                manager.LoadCore(New TestCoreMod)

                'Test PluginManager
                Dim managerType = ReflectionHelpers.GetTypeByName(GetType(PluginManager).AssemblyQualifiedName, manager)
                Assert.IsNotNull(managerType)
                Assert.AreEqual(GetType(PluginManager), managerType)

                'Test String
                Dim stringType = ReflectionHelpers.GetTypeByName(GetType(String).AssemblyQualifiedName, manager)
                Assert.IsNotNull(stringType)
                Assert.AreEqual(GetType(String), stringType)

                'Test something that's NOT an assembly qualified name, and SHOULD return null
                Dim bogusType = ReflectionHelpers.GetTypeByName(Guid.NewGuid.ToString, manager)
                Assert.IsNull(bogusType)
            End Using
        End Sub

        <TestMethod> Public Sub CanCreateInstanceTests()
            Assert.IsTrue(ReflectionHelpers.CanCreateInstance(GetType(ReflectionHelpersTests)), "Failed to indicate that ReflectionHelpersTests can have an instance created")
            Assert.IsFalse(ReflectionHelpers.CanCreateInstance(GetType(IContainer(Of Object))), "Incorrectly indicated an IContainer(Of Object) can have an instance created")
            Assert.IsFalse(ReflectionHelpers.CanCreateInstance(GetType(CoreSkyEditorPlugin)), "Incorrectly indicated an abstract class can have an instance created")
        End Sub

        <TestMethod> Public Sub CreateInstanceTests()
            Dim multi = ReflectionHelpers.CreateInstance(GetType(TestContainerClassMulti))
            Assert.IsNotNull(multi)
            Assert.IsInstanceOfType(multi, GetType(TestContainerClassMulti))
        End Sub

        <TestMethod> Public Sub CreateNewInstanceTests()
            Dim original As New TestContainerClassMulti
            original.IntegerItem = 94
            original.StringItem = "94"

            Dim newInst = ReflectionHelpers.CreateNewInstance(original)
            Assert.IsNotNull(newInst)
            Assert.IsInstanceOfType(newInst, GetType(TestContainerClassMulti))
            Assert.AreNotEqual(94, DirectCast(newInst, TestContainerClassMulti).IntegerItem, "CreateNewInstance either cloned the class, or returned the same instance")

            original.StringItem = "Altered"
            Assert.AreNotEqual(original.StringItem, DirectCast(newInst, TestContainerClassMulti), "CreateNewInstance did not create a new instance, instead returning the same instance")
        End Sub

        <TestMethod> Public Sub GetTypeFriendlyNameTests()
            'Due to the nature of this function (reflection to find an entry in a resource file), we cannot fully test it.
            'Therefore, the only requirement to pass the test is that we return a string that is not null or empty.
            'The rest must be tested manually

            Assert.IsFalse(String.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(GetType(ReflectionHelpersTests))))
            Assert.IsFalse(String.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(GetType(Integer))))
            Assert.IsFalse(String.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(GetType(GenericFile))))
            Assert.IsFalse(String.IsNullOrWhiteSpace(ReflectionHelpers.GetTypeFriendlyName(GetType(Solution))))
        End Sub
    End Class
End Namespace

