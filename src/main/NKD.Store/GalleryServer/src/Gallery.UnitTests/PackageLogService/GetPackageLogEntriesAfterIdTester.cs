using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageLogService
{
    [TestFixture]
    public class GetPackageLogEntriesAfterIdTester
    {
        private Server.PackageLogService _packageLogService;
        private Mock<IRepository<PackageLogEntry>> _mockedPackageLogRepository;
        private Mock<IWebFaultExceptionCreator> _mockedWebFaultExceptionFactory;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageLogRepository = new Mock<IRepository<PackageLogEntry>>();
            _mockedWebFaultExceptionFactory = new Mock<IWebFaultExceptionCreator>();
            var mockedConfigSettings = new Mock<IConfigSettings>();
            _packageLogService = new Server.PackageLogService(_mockedPackageLogRepository.Object, _mockedWebFaultExceptionFactory.Object,
                mockedConfigSettings.Object, new Mock<ILogger>().Object);

            mockedConfigSettings.SetupGet(cs => cs.MaxPackageLogEntryRecordCount).Returns(999999);
            _mockedWebFaultExceptionFactory.Setup(wfef => wfef.CreateWebFaultException(It.IsAny<Exception>(), It.IsAny<string>()))
                .Returns(new WebFaultException<string>(null, HttpStatusCode.Accepted));
        }

        [Test]
        public void ShouldReturnPackageLogEntriesAfterTheGivenId()
        {
            const string lastLogId = "3";

            List<PackageLogEntry> logEntries = Enumerable.Range(1, 5)
                .Select(id => new PackageLogEntry { Id = id }).ToList();
            List<PackageLogEntry> expectedNewLogEntries = logEntries.Where(le => le.Id > int.Parse(lastLogId)).ToList();
            _mockedPackageLogRepository.SetupGet(dpr => dpr.Collection).Returns(logEntries.AsQueryable());

            List<PackageLogEntry> packageLogEntriesAfterId = _packageLogService.GetNewPackageLogEntries(lastLogId);

            CollectionAssert.AreEqual(expectedNewLogEntries, packageLogEntriesAfterId, "The wrong Package Log Entries were returned.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("abc")]
        public void ShouldThrowWhenGivenLogIdStringCannotBeParsedAsInt(string logId)
        {
            TestDelegate methodThatShouldThrow = () => _packageLogService.GetNewPackageLogEntries(logId);

            Assert.Throws<WebFaultException<string>>(methodThatShouldThrow, "Exception should have been thrown.");
        }
    }
}