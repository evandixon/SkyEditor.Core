Imports System.Reflection
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Projects
Imports SkyEditor.Core.Tests.TestComponents
Imports SkyEditor.Core.Utilities

Namespace Utilities
    <TestClass()> Public Class ReflectionHelpersTests
        Public Class TestContainerClassMulti

            Public Property StringItem As String

            Public Property IntegerItem As Integer

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
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(ReflectionHelpersTests), GetType(ReflectionHelpersTests)), "Failed to see type equality (ReflectionHelpersTests is of type ReflectionHelpersTests)")
            'Interface checks
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(GenericFile), GetType(IOpenableFile)), "Failed to see interface IOpenableFile on GenericFile")
            'Inheritance tests
            Assert.IsTrue(ReflectionHelpers.IsOfType(GetType(CoreSkyEditorPlugin), GetType(SkyEditorPlugin)), "Failed to see CoreSkyEditorPlugin inherits SkyEditorPlugin")
            'Make sure it returns false sometimes
            Assert.IsFalse(ReflectionHelpers.IsOfType(GetType(String), GetType(Integer)), "Failed to see String is not of type Integer")
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

