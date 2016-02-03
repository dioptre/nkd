using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Impl;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.AutoMapper
{
    [TestFixture]
    public abstract class AutoMapperTester
    {
        protected readonly IMapper Mapper = new Infrastructure.Impl.AutoMapper();
        protected readonly Mock<IRepository<PackageDataAggregate>> MockedPackageDataAggregateRepository;

        protected AutoMapperTester()
        {
            MockedPackageDataAggregateRepository = new Mock<IRepository<PackageDataAggregate>>();
            AutoMapperBootstrapper autoMapperBootstrapper = new AutoMapperBootstrapper(new Mock<IDependencyStringFactory>().Object, MockedPackageDataAggregateRepository.Object);
            autoMapperBootstrapper.RegisterMappings();
        }

    }
}