using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T>
        where T : class
    {
        protected readonly IDatabaseContext DatabaseContext;
        private readonly IMapper _mapper;

        protected RepositoryBase(IDatabaseContext databaseContext, IMapper mapper)
        {
            DatabaseContext = databaseContext;
            _mapper = mapper;
        }

        protected abstract IDbSet<T> DbSet { get; }
        protected abstract Expression<Func<T, bool>> GetByIdExpression(T obj);

        protected virtual T GetById(T obj)
        {
            return DbSet.Single(GetByIdExpression(obj));
        }

        public IQueryable<T> Collection { get { return DbSet; } }

        public T Create(T objectToCreate)
        {
            DbSet.Add(objectToCreate);
            try
            {
                DatabaseContext.SaveChanges();
            }
            catch
            {
                DbSet.Remove(objectToCreate);
                throw;
            }
            return objectToCreate;
        }

        public virtual T Update(T objectToUpdate)
        {
            T entityToUpdate = GetById(objectToUpdate);
            _mapper.Map(objectToUpdate, entityToUpdate);
            DatabaseContext.SaveChanges();
            return entityToUpdate;
        }

        public void Update(IEnumerable<T> objectsToUpdate)
        {
            foreach (var objectToUpdate in objectsToUpdate)
            {
                T entityToUpdate = GetById(objectToUpdate);
                _mapper.Map(objectToUpdate, entityToUpdate);
            }
            DatabaseContext.SaveChanges();
        }

        public void DeleteSingle(Func<T, bool> deletionPredicate)
        {
            var objectsFromDeletionPredicate = DbSet.Where(deletionPredicate);
            if (objectsFromDeletionPredicate.Count() == 1)
            {
                T objectToDelete = objectsFromDeletionPredicate.Single();
                DbSet.Remove(objectToDelete);
                DatabaseContext.SaveChanges();
            }
        }

        public void DeleteMany(Func<T, bool> deletionPredicate)
        {
            IEnumerable<T> objectsToDelete = DbSet.Where(deletionPredicate);
            foreach (var objectToDelete in objectsToDelete)
            {
                DbSet.Remove(objectToDelete);
            }
            DatabaseContext.SaveChanges();
        }
    }
}