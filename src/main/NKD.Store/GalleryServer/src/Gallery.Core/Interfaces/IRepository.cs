using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallery.Core.Interfaces
{
    public interface IRepository<T>
    {
        IQueryable<T> Collection { get; }
        T Create(T objectToCreate);
        T Update(T objectToUpdate);
        void Update(IEnumerable<T> objectsToUpdate);
        void DeleteSingle(Func<T, bool> deletionPredicate);
        void DeleteMany(Func<T, bool> deletionPredicate);
    }
}