using System;
using Gallery.Core;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using Gallery.IntegrationTests.Helpers;
using NUnit.Framework;

namespace Gallery.IntegrationTests
{
    public class ConfigSettingsTester : IntegrationTesterBase<IConfigSettings>
    {
        private readonly IConfigSettings _instance;

        public ConfigSettingsTester()
        {
            IoC.Resolver.Register<IConfigSettings, ConfigSettings>();
            _instance = IoC.Resolver.Resolve<IConfigSettings>();
        }

        [Test]
        public void CanRetrieveRelativePackagesDirectory()
        {
            string packageDirectory = _instance.RelativePackageDirectory;

            Assert.IsNotNullOrEmpty(packageDirectory, "Retrieved RelativePackageDirectory should not be null or empty.");
        }

        [Test]
        public void CanRetrieveRelativeTemporaryDirectory()
        {
            string temporaryDirectory = _instance.RelativeTemporaryDirectory;

            Assert.IsNotNullOrEmpty(temporaryDirectory, "Retrieved RelativeTemporaryDirectory should not be null or empty.");
        }

        [Test]
        public void CanRetrieveRelativePluginAssemblyDirectory()
        {
            string relativePluginAssemblyDirectory = _instance.RelativePluginAssemblyDirectory;

            Assert.IsNotNullOrEmpty(relativePluginAssemblyDirectory, "Retrieved RelativePluginAssemblyDirectory should not be null or empty.");
        }

        [Test]
        public void CanRetrieveFrontEndWebSiteRoot()
        {
            string frontEndWebSiteRoot = _instance.FrontEndWebSiteRoot;

            Assert.IsNotNullOrEmpty(frontEndWebSiteRoot, "Retrieved FrontEndWebSiteRoot should not be null or empty.");
        }

        [Test]
        public void CanRetrieveValidatePackageKeyUri()
        {
            string validatePackageKeyUri = _instance.ValidatePackageKeyUri;

            Assert.IsNotNullOrEmpty(validatePackageKeyUri, "Retrieved ValidatePackageKeyUri should not be null or empty.");
        }

        [Test]
        public void CanRetrieveReportAbuseUriTemplate()
        {
            string reportAbuseUriTemplate = _instance.ReportAbuseUriTemplate;

            Assert.IsNotNullOrEmpty(reportAbuseUriTemplate, "Retrieved ReportAbuseUriTemplate should not be null or empty.");
        }

        [Test]
        public void ReportAbuseUriTemplateShouldBePrefixedWithFrontEndWebSiteRoot()
        {
            string frontEndWebSiteRoot = _instance.FrontEndWebSiteRoot;

            string reportAbuseUriTemplate = _instance.ReportAbuseUriTemplate;

            StringAssert.Contains(frontEndWebSiteRoot, reportAbuseUriTemplate);
        }

        [Test]
        public void ReportAbuseUriTemplateShouldBeValidUri()
        {
            string reportAbuseUriTemplate = _instance.ReportAbuseUriTemplate.RemoveTemplateTokens();

            Assert.IsTrue(Uri.IsWellFormedUriString(reportAbuseUriTemplate, UriKind.Absolute),
                "ReportAbuseUriTemplate '{0}' is not a valid URI.", reportAbuseUriTemplate);
        }

        [Test]
        public void CanRetrieveHashAlgorithm()
        {
            string hashAlgorithm = _instance.HashAlgorithm;

            Assert.IsNotNullOrEmpty(hashAlgorithm, "Retrieved HashAlgorithm should not be null or empty.");
        }

        [Test]
        public void CanRetrieveAuthenticatePackageRequests()
        {
#pragma warning disable 168
            bool authenticatePackageRequests = _instance.AuthenticatePackageRequests;
#pragma warning restore 168
        }

        [Test]
        public void CanRetrieveHashEncodingType()
        {
            string hashEncodingType = _instance.HashEncodingType;

            Assert.IsNotNullOrEmpty(hashEncodingType, "Retrieved HashEncodingType should not be null or empty.");
        }

        [Test]
        public void CanRetrieveAuthorizePackageIdsUri()
        {
            string authorizePackageIdsUri = _instance.AuthorizePackageIdsUri;

            Assert.IsNotNullOrEmpty(authorizePackageIdsUri, "Retrieved AuthorizePackageIdsUri should not be null or empty.");
        }

        [Test]
        public void CanRetrieveGalleryDetailsUriTemplate()
        {
            string galleryDetailsUriTemplate = _instance.GalleryDetailsUriTemplate;

            Assert.IsNotNullOrEmpty(galleryDetailsUriTemplate, "Retrieved GalleryDetailsUriTemplate should not be null or empty.");
        }

        [Test]
        public void GalleryDetailsUriTemplateShouldBePrefixedWithFrontEndWebSiteRoot()
        {
            string frontEndWebSiteRoot = _instance.FrontEndWebSiteRoot;

            string galleryDetailsUriTemplate = _instance.GalleryDetailsUriTemplate;

            StringAssert.Contains(frontEndWebSiteRoot, galleryDetailsUriTemplate);
        }

        [Test]
        public void GalleryDetailsUriTemplateShouldBeValidUri()
        {
            string galleryDetailsUriTemplate = _instance.GalleryDetailsUriTemplate.RemoveTemplateTokens();

            Assert.IsTrue(Uri.IsWellFormedUriString(galleryDetailsUriTemplate, UriKind.Absolute),
                "GalleryDetailsUriTemplate '{0}' is not a valid URI.", galleryDetailsUriTemplate);
        }

        [Test]
        public void CanRetrieveExternalPackageRequestTimeout()
        {
#pragma warning disable 168
            int externalPackageRequestTimeout = _instance.ExternalPackageRequestTimeout;
#pragma warning restore 168
        }

        [Test]
        public void CanRetrieveMaxPackageLogEntryRecordCount()
        {
#pragma warning disable 168
            int maxPackageLogEntryRecordCount = _instance.MaxPackageLogEntryRecordCount;
#pragma warning restore 168
        }
    }
}