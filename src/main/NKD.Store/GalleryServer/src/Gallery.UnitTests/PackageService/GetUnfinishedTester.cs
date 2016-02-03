using System;
using System.Collections.Generic;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageService {
    public class GetUnfinishedTester : PackageServiceTester {
        [Test]
        public void ShouldThrowWebFaultExceptionWhenGetUnfinishedPackagesThrows()
        {
            MockedUnfinishedPackageGetter.Setup(ufg => ufg.GetUnfinishedPackages(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new Exception());

            TestDelegate methodThatShouldThrow = () => PackageService.GetUnfinished(Guid.NewGuid().ToString(), new[] { "Foo", "Bar" });

            ExceptionAssert.ThrowsWebFaultException<string>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldLogErrorWhenAuthenticatorThrows()
        {
            MockedUnfinishedPackageGetter.Setup(ufg => ufg.GetUnfinishedPackages(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new Exception());

            try
            {
                PackageService.GetUnfinished(Guid.NewGuid().ToString(), new[] { "Foo", "Bar" });
            }
            catch (WebFaultException<string>)
            { }

            MockedLogger.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once(),
                "An error should have been logged.");
        }

        [Test]
        public void ShouldCallUnfinishedPackageGetter()
        {
            string userKey = Guid.NewGuid().ToString();
            var packageIds = new[] {"Foo", "Parse"};

            PackageService.GetUnfinished(userKey, packageIds);

            MockedUnfinishedPackageGetter.Verify(upg => upg.GetUnfinishedPackages(userKey, packageIds), Times.Once());
        }

        [Test]
        public void ShouldReturnWhatUnfinishedPackageGetterReturns()
        {
            var expectedPackages = new[] { new Package(), new Package() };
            MockedUnfinishedPackageGetter.Setup(ufg => ufg.GetUnfinishedPackages(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(expectedPackages);

            IEnumerable<Package> packages = PackageService.GetUnfinished(Guid.NewGuid().ToString(), new[] { "Foo", "Parse" });

            CollectionAssert.AreEqual(packages, expectedPackages, "Incorrect collection of Packages returned.");
        }
    }
}