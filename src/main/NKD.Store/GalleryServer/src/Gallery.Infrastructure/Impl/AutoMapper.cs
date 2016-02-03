using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class AutoMapper : IMapper
    {
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return Mapper.Map<TSource, TDestination>(source);
        }

        public void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            Mapper.Map(source, destination);
        }

        public IList<TDestination> MapMultiple<TSource, TDestination>(IEnumerable<TSource> sources)
        {
            return sources.Select(Map<TSource, TDestination>).ToList();
        }
    }
}