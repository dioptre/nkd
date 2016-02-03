using System.Collections.Generic;

namespace Gallery.Core.Interfaces
{
    public interface IMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);
        void Map<TSource, TDestination>(TSource source, TDestination destination);
        IList<TDestination> MapMultiple<TSource, TDestination>(IEnumerable<TSource> sources);
    }
}