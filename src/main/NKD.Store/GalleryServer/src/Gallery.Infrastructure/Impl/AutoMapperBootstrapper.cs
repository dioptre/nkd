using System;
using AutoMapper;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Core.Extensions;
using System.Linq;

namespace Gallery.Infrastructure.Impl
{
    public class AutoMapperBootstrapper : IMapperBootstrapper
    {
        private readonly IDependencyStringFactory _dependencyStringFactory;
        private readonly IRepository<PackageDataAggregate> _packageDataAggregateRepository;

        public AutoMapperBootstrapper(IDependencyStringFactory dependencyStringFactory, IRepository<PackageDataAggregate> packageDataAggregateRepository)
        {
            _dependencyStringFactory = dependencyStringFactory;
            _packageDataAggregateRepository = packageDataAggregateRepository;
        }

        public void RegisterMappings()
        {
            Mapper.CreateMap<Package, Package>()
                .ForMember(p => p.DownloadCount, opt => opt.Ignore())
                .ForMember(p => p.RatingsCount, opt => opt.Ignore())
                .ForMember(p => p.RatingAverage, opt => opt.Ignore())
                .ForMember(p => p.Screenshots, opt => opt.Ignore());
            Mapper.CreateMap<PublishedPackage, PublishedPackage>();
            CreateMapForPackageToPublishedPackage();
            Mapper.CreateMap<Screenshot, PublishedScreenshot>()
                .ForMember(ps => ps.Id, opt => opt.Ignore())
                .ForMember(ps => ps.PublishedPackageId, opt => opt.MapFrom(s => s.PackageId))
                .ForMember(ps => ps.PublishedPackageVersion, opt => opt.MapFrom(s => s.PackageVersion));
            AssertConfigurationIsValid();
        }

        private void CreateMapForPackageToPublishedPackage()
        {
            Mapper.CreateMap<Package, PublishedPackage>()
                .ForMember(pp => pp.Title, opt => opt.MapFrom(p => p.Title.NullifyEmpty()))
                .ForMember(pp => pp.Authors, opt => opt.MapFrom(p => p.Authors.NullifyEmpty()))
                .ForMember(pp => pp.PackageType, opt => opt.MapFrom(p => p.PackageType.NullifyEmpty()))
                .ForMember(pp => pp.Summary, opt => opt.MapFrom(p => p.Summary.NullifyEmpty()))
                .ForMember(pp => pp.Description, opt => opt.MapFrom(p => p.Description.NullifyEmpty()))
                .ForMember(pp => pp.Copyright, opt => opt.MapFrom(p => p.Copyright.NullifyEmpty()))
                .ForMember(pp => pp.PackageHashAlgorithm, opt => opt.MapFrom(p => p.PackageHashAlgorithm.NullifyEmpty()))
                .ForMember(pp => pp.PackageHash, opt => opt.MapFrom(p => p.PackageHash.NullifyEmpty()))
                .ForMember(pp => pp.ExternalPackageUrl, opt => opt.MapFrom(p => p.ExternalPackageUrl.NullifyEmpty()))
                .ForMember(pp => pp.ProjectUrl, opt => opt.MapFrom(p => p.ProjectUrl.NullifyEmpty()))
                .ForMember(pp => pp.LicenseUrl, opt => opt.MapFrom(p => p.LicenseUrl.NullifyEmpty()))
                .ForMember(pp => pp.IconUrl, opt => opt.MapFrom(p => p.IconUrl.NullifyEmpty()))
                .ForMember(pp => pp.Categories, opt => opt.MapFrom(p => p.Categories.NullifyEmpty()))
                .ForMember(pp => pp.Tags, opt => opt.MapFrom(PadTags))
                .ForMember(pp => pp.Dependencies, opt => opt.MapFrom(p => _dependencyStringFactory.CreateDependencyListAsString(p.Dependencies)))
                .ForMember(pp => pp.VersionRating, opt => opt.MapFrom(p => p.RatingAverage))
                .ForMember(pp => pp.VersionDownloadCount, opt => opt.MapFrom(p => p.DownloadCount))
                .ForMember(pp => pp.VersionRatingsCount, opt => opt.MapFrom(p => p.RatingsCount))
                .ForMember(pp => pp.ReportAbuseUrl, opt => opt.Ignore())
                .ForMember(pp => pp.Rating, opt => opt.MapFrom(p => GetAggregate(p.Id, pda => pda.Rating)))
                .ForMember(pp => pp.RatingsCount, opt => opt.MapFrom(p => GetAggregate(p.Id, pda => pda.RatingsCount)))
                .ForMember(pp => pp.DownloadCount, opt => opt.MapFrom(p => GetAggregate(p.Id, pda => pda.DownloadCount)));
        }

        private T GetAggregate<T>(string packageId, Func<PackageDataAggregate, T> aggregateToGet)
        {
            PackageDataAggregate packageDataAggregate = _packageDataAggregateRepository.Collection.SingleOrDefault(pda => pda.PackageId == packageId);
            return packageDataAggregate != null ? aggregateToGet(packageDataAggregate) : default(T);
        }

        private static string PadTags(Package package)
        {
            var packageTags = package.Tags.NullifyEmpty();
            if (packageTags != null)
            {
                packageTags = String.Format(" {0} ", packageTags.Trim());
            }
            return packageTags;
        }

        public void AssertConfigurationIsValid()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}