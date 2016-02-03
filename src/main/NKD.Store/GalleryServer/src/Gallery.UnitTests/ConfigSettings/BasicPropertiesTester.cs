using System;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.ConfigSettings
{
    public class BasicPropertiesTester : ConfigSettingsTester
    {
        private void AssertPropertyMatchesConfigurationManager(Func<IConfigSettings, string> propertyToTest, string key)
        {
            string expectedResult = "expected result " + key;
            MockedConfigurationManager.Setup(cm => cm.GetString(key)).Returns(expectedResult);

            string result = propertyToTest(ConfigSettings);

            Assert.AreEqual(expectedResult, result, "Property did not match what ConfigurationManager returned.");
        }

        private void AssertPropertyMatchesConfigurationManager(Func<IConfigSettings, int> propertyToTest, string key)
        {
            int expectedResult = new Random().Next();
            MockedConfigurationManager.Setup(cm => cm.GetInt(key)).Returns(expectedResult);

            int result = propertyToTest(ConfigSettings);

            Assert.AreEqual(expectedResult, result, "Property did not match what ConfigurationManager returned.");
        }

        private void AssertPropertyMatchesConfigurationManager(Func<IConfigSettings, bool> propertyToTest, string key)
        {
            const bool expectedResult = true;
            MockedConfigurationManager.Setup(cm => cm.GetBool(key)).Returns(expectedResult);

            bool result = propertyToTest(ConfigSettings);

            Assert.AreEqual(expectedResult, result, "Property did not match what ConfigurationManager returned.");
        }

        [Test]
        public void RelativePackageDirectorySettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.RelativePackageDirectory, "RelativePackageDirectory");
        }

        [Test]
        public void RelativeTemporaryDirectorySettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.RelativeTemporaryDirectory, "RelativeTemporaryDirectory");
        }

        [Test]
        public void RelativePluginAssemblyDirectorySettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.RelativePluginAssemblyDirectory, "RelativePluginAssemblyDirectory");
        }

        [Test]
        public void FrontEndWebSiteRootSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.FrontEndWebSiteRoot, "FrontEndWebSiteRoot");
        }

        [Test]
        public void ValidatePackageKeyUriSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.ValidatePackageKeyUri, "ValidatePackageKeyUri");
        }

        [Test]
        public void HashAlgorithmSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.HashAlgorithm, "HashAlgorithm");
        }

        [Test]
        public void AuthenticatePackageRequestsSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.AuthenticatePackageRequests, "AuthenticatePackageRequests");
        }

        [Test]
        public void HashEncodingTypeSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.HashEncodingType, "HashEncodingType");
        }

        [Test]
        public void MigratorProviderSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.MigratorProvider, "MigratorProvider");
        }

        [Test]
        public void RelativeAssemblyDirectorySettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.RelativeAssemblyDirectory, "RelativeAssemblyDirectory");
        }

        [Test]
        public void AuthorizePackageIdsUriSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.AuthorizePackageIdsUri, "AuthorizePackageIdsUri");
        }

        [Test]
        public void ExternalPackageRequestTimeoutSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.ExternalPackageRequestTimeout, "ExternalPackageRequestTimeout");
        }

        [Test]
        public void MaxPackageLogEntryRecordCountSettingShouldMatchWhatConfigurationManagerReturns()
        {
            AssertPropertyMatchesConfigurationManager(cs => cs.MaxPackageLogEntryRecordCount, "MaxPackageLogEntryRecordCount");
        }

        [Test]
        public void PhysicalSitePathShouldReturnAppDomainAppPath()
        {
            const string expectedValue = "expected";
            MockedHttpRuntime.SetupGet(hr => hr.AppDomainAppPath).Returns(expectedValue);

            string value = ConfigSettings.PhysicalSitePath;

            Assert.AreEqual(expectedValue, value, "PhysicalSitePath should have returned the AppDomainAppPath.");
        }

        [Test]
        public void GalleryDetailsUriTemplateShouldReturnFrontEndRootCombinedWithTemplateSeparatedBySlash()
        {
            const string frontEndWebSiteRoot = "frontEnd";
            const string galleryDetailsUriTemplate = "template";
            const string expectedResult = frontEndWebSiteRoot + "/" + galleryDetailsUriTemplate;
            MockedConfigurationManager.Setup(cm => cm.GetString("FrontEndWebSiteRoot")).Returns(frontEndWebSiteRoot);
            MockedConfigurationManager.Setup(cm => cm.GetString("GalleryDetailsUriTemplate")).Returns(galleryDetailsUriTemplate);

            string result = ConfigSettings.GalleryDetailsUriTemplate;

            Assert.AreEqual(expectedResult, result, "Incorrect template returned.");
        }

        [Test]
        public void ReportAbuseUriTemplateShouldReturnFrontEndRootCombinedWithTemplateSeparatedBySlash()
        {
            const string frontEndWebSiteRoot = "frontEnd";
            const string reportAbuseUriTemplate = "template";
            const string expectedResult = frontEndWebSiteRoot + "/" + reportAbuseUriTemplate;
            MockedConfigurationManager.Setup(cm => cm.GetString("FrontEndWebSiteRoot")).Returns(frontEndWebSiteRoot);
            MockedConfigurationManager.Setup(cm => cm.GetString("ReportAbuseUriTemplate")).Returns(reportAbuseUriTemplate);

            string result = ConfigSettings.ReportAbuseUriTemplate;

            Assert.AreEqual(expectedResult, result, "Incorrect template returned.");
        }
    }
}