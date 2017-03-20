using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEditor.Core.Tests.Utilities
{
    [TestClass]
    public class SemanticVersionTests
    {
        public const string TestCategory = "Utilities - Semantic Version";

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestStandardParsing()
        {
            var version1 = SemanticVersion.Parse("1.10.567");
            var version234 = SemanticVersion.Parse("2.3.4.58");

            Assert.AreEqual(1, version1.Major);
            Assert.AreEqual(10, version1.Minor);
            Assert.AreEqual(567, version1.Patch);
            Assert.IsFalse(version1.Build.HasValue);

            Assert.AreEqual(2, version234.Major);
            Assert.AreEqual(3, version234.Minor);
            Assert.AreEqual(4, version234.Patch);
            Assert.AreEqual(58, version234.Build.Value);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestPreReleaseParsing()
        {
            var versionPre = SemanticVersion.Parse("1.0.0-pre1");
            var versionAlpha = SemanticVersion.Parse("1.0.0-alpha2");
            var versionBeta = SemanticVersion.Parse("1.0.0-beta3");
            var versionRC = SemanticVersion.Parse("1.0.0-rc4");
            var versionDev = SemanticVersion.Parse("1.0.0-dev5");
            var versionCI = SemanticVersion.Parse("1.0.0-CI6");

            Assert.AreEqual("pre1", versionPre.PreReleaseTag);
            Assert.AreEqual("alpha2", versionAlpha.PreReleaseTag);
            Assert.AreEqual("beta3", versionBeta.PreReleaseTag);
            Assert.AreEqual("rc4", versionRC.PreReleaseTag);
            Assert.AreEqual("dev5", versionDev.PreReleaseTag);
            Assert.AreEqual("CI6", versionCI.PreReleaseTag);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestMetadataParsing()
        {
            var versionPre = SemanticVersion.Parse("1.0.0+debug");
            var versionAlpha = SemanticVersion.Parse("1.0.0+fixed");

            Assert.AreEqual("debug", versionPre.Metadata);
            Assert.AreEqual("fixed", versionAlpha.Metadata);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestCombinedParsing()
        {
            var version1 = SemanticVersion.Parse("1.10.567-CI12345+debug");
            var version234 = SemanticVersion.Parse("2.3.4.58-Beta56+fixed");

            Assert.AreEqual(1, version1.Major);
            Assert.AreEqual(10, version1.Minor);
            Assert.AreEqual(567, version1.Patch);
            Assert.IsFalse(version1.Build.HasValue);
            Assert.AreEqual("CI12345", version1.PreReleaseTag);
            Assert.AreEqual("debug", version1.Metadata);

            Assert.AreEqual(2, version234.Major);
            Assert.AreEqual(3, version234.Minor);
            Assert.AreEqual(4, version234.Patch);
            Assert.AreEqual(58, version234.Build.Value);
            Assert.AreEqual("Beta56", version234.PreReleaseTag);
            Assert.AreEqual("fixed", version234.Metadata);
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestSorting()
        {
            var versions = new SemanticVersion[]
            {
                SemanticVersion.Parse("1.0.0"),
                SemanticVersion.Parse("1.0.0+debug"),
                SemanticVersion.Parse("4.3.5"),
                SemanticVersion.Parse("4.3.2-beta1+fix2"),
                SemanticVersion.Parse("3.0.0"),
                SemanticVersion.Parse("4.3.2-beta1+fix1"),
                SemanticVersion.Parse("4.3.0"),
                SemanticVersion.Parse("2.0.0"),
                SemanticVersion.Parse("4.3.1"),
                SemanticVersion.Parse("4.3.2-beta1"),
                SemanticVersion.Parse("4.1.0"),
                SemanticVersion.Parse("4.3.2-beta2"),
                SemanticVersion.Parse("5.0.0"),
            };

            var sorted = new SemanticVersion[] 
            {
                SemanticVersion.Parse("1.0.0"),
                SemanticVersion.Parse("1.0.0+debug"),
                SemanticVersion.Parse("2.0.0"),
                SemanticVersion.Parse("3.0.0"),
                SemanticVersion.Parse("4.1.0"),
                SemanticVersion.Parse("4.3.0"),
                SemanticVersion.Parse("4.3.1"),
                SemanticVersion.Parse("4.3.2-beta1"),
                SemanticVersion.Parse("4.3.2-beta1+fix1"),
                SemanticVersion.Parse("4.3.2-beta1+fix2"),
                SemanticVersion.Parse("4.3.2-beta2"),
                SemanticVersion.Parse("4.3.5"),
                SemanticVersion.Parse("5.0.0"),
            };

            Assert.IsTrue(versions.OrderBy(x => x).SequenceEqual(sorted));
        }
    }
}
