using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests
{
    public static class TestHelpers
    {
        /// <summary>
        /// Tests a static function to ensure a <see cref="ArgumentNullException"/> is thrown
        /// </summary>
        /// <param name="type">Type of the static class</param>
        /// <param name="functionName">Name of the function to test</param>
        /// <param name="testParamName">Name of the paramter to test (only used in Assert failure messages)</param>
        /// <param name="types">Types of the parameters of the function (to distinguish between overloads)</param>
        /// <param name="args">Arguments to the function</param>
        public static void TestStaticFunctionNullParameters(Type type, string functionName, string testParamName, Type[] types, params object[] args)
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
    }
}
