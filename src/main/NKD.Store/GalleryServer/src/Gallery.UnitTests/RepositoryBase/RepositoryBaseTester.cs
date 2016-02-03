using System.Data.Entity;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.RepositoryBase
{
    [TestFixture]
    public abstract class RepositoryBaseTester
    {
        protected FakeStringRepository FakeStringRepository;

        protected Mock<IDatabaseContext> MockedDatabaseContext;
        protected Mock<IMapper> MockedMapper;

        [SetUp]
        public void SetUp()
        {
            MockedDatabaseContext = new Mock<IDatabaseContext>();
            MockedMapper = new Mock<IMapper>();

            FakeStringRepository = new FakeStringRepository(MockedDatabaseContext.Object, MockedMapper.Object);
        }
    }
}