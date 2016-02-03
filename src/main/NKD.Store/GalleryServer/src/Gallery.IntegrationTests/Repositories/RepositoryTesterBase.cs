using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.IntegrationTests.Repositories
{
    public abstract class RepositoryTesterBase<T> : IntegrationTesterBase<IRepository<T>>
    {
        private readonly string _repositoryTypeName = typeof(T).Name;

        protected abstract T GetObjectToCreate();
        protected abstract Func<T, bool> GetDeletionPredicate();

        protected RepositoryTesterBase()
            : base(true)
        { }

        [TearDown]
        public void TearDown()
        {
            Instance.DeleteMany(t => true);
        }

        [Test]
        public void CollectionShouldReturnNonNullCollection()
        {
            IQueryable<T> queryable = Instance.Collection;

            Assert.IsNotNull(queryable, "Collection of {0} objects should not be null.", _repositoryTypeName);
        }

        [Test]
        public void CreateShouldInsertGivenObjectIntoPersistence()
        {
            T objectToCreate = GetObjectToCreate();

            Instance.Create(objectToCreate);

            List<T> collection = Instance.Collection.ToList();
            Assert.IsTrue(collection.Contains(objectToCreate), "{0} was not created or not returned from persistence.", _repositoryTypeName);

            Instance.DeleteSingle(GetDeletionPredicate());
        }

        [Test]
        public void DeleteSingleShouldWork()
        {
            Instance.Create(GetObjectToCreate());

            Instance.DeleteSingle(GetDeletionPredicate());
        }

        [Test]
        public void DeleteManyWithPredicateMatchingAllRecordsShouldRemoveAllRecords()
        {
            Instance.Create(GetObjectToCreate());

            Instance.DeleteMany(t => true);

            var numberOfRecords = Instance.Collection.Count();
            Assert.AreEqual(0, numberOfRecords, "No more records for '{0}' should exist after deleting all of them.", typeof(T).Name);
        }
    }
}