using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.IO
{
    [TestClass]
    public class IOHelperTests
    {
        public const string TestCategory = "I/O - IOHelper";

        #region Null Parameter Tests
        private void TestStaticFunction(Type type, string functionName, string testParamName, Type[] types, params object[] args)
        {
            var method = type.GetTypeInfo().GetMethod(functionName, types);
            try
            {
                var result = method.Invoke(null, args);
                if (result is Task)
                {
                    (result as Task).Wait();
                }
            }
            catch (ArgumentNullException)
            {
                // Pass
                return;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count > 1)
                {
                    Assert.Fail($"Too many exceptions for parameter \"{testParamName}\".  Details: " + ex.ToString());
                }
                else if (ex.InnerExceptions.Count == 1)
                {
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(ArgumentNullException));
                    return;
                }
                else
                {
                    Assert.Fail("Got an AggregateException without any inner exceptions.");
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is ArgumentNullException)
                {
                    // Pass
                    return;
                }
                else
                {
                    Assert.Fail("Got an TargetInvocationException with incorrect exception type.  Details: " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Incorrect exception thrown for param \"{testParamName}\".  Should be ArgumentNullException.  Details: " + ex.ToString());
            }
            Assert.Fail($"No exception thrown for param \"{testParamName}\".");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetCreatableFileTypes_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetCreateableFileTypes), "manager", new Type[] { typeof(PluginManager) }, new object[] { null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetOpenableFileTypes_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetOpenableFileTypes), "manager", new Type[] { typeof(PluginManager) }, new object[] { null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void CreateNewFile_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.CreateNewFile), "fileType", new Type[] { typeof(string), typeof(TypeInfo) }, new object[] { null, null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void OpenFile_SpecificType_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "filename", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "fileType", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "test.txt", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "test.txt", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void OpenFile_AutoDetect_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "path", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "duplicateFileTypeSelector", new Type[] { typeof(string), typeof(TypeInfo), typeof(PluginManager) }, new object[] { "test.txt", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenFile), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "test.txt", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void OpenDirectory_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenDirectory), "path", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenDirectory), "duplicateFileTypeSelector", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.OpenDirectory), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetFileType_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetFileType), "path", new Type[] { typeof(GenericFile), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetFileType), "duplicateFileTypeSelector", new Type[] { typeof(GenericFile), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { new GenericFile(), null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetFileType), "manager", new Type[] { typeof(GenericFile), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { new GenericFile(), new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void GetDirectoryType_ExceptionOnNullParameter()
        {
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetDirectoryType), "path", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { null, new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetDirectoryType), "duplicateFileTypeSelector", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", null, new PluginManager() });
            TestStaticFunction(typeof(IOHelper), nameof(IOHelper.GetDirectoryType), "manager", new Type[] { typeof(string), typeof(IOHelper.DuplicateMatchSelector), typeof(PluginManager) }, new object[] { "/test", new IOHelper.DuplicateMatchSelector(IOHelper.PickFirstDuplicateMatchSelector), null });
        }

        #endregion



    }
}
