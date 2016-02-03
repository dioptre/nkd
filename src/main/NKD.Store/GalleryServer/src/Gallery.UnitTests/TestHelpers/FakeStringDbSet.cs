using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gallery.UnitTests.TestHelpers
{
    public class FakeStringDbSet : IDbSet<string>
    {
        public IList<string> Collection { get; set; }
        public bool RemoveWasCalled;

        public FakeStringDbSet()
        {
            Collection = new List<string>();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get { throw new NotImplementedException(); } }
        public Type ElementType { get { throw new NotImplementedException(); } }
        public IQueryProvider Provider { get { throw new NotImplementedException(); } }
        public string Find(params object[] keyValues) { throw new NotImplementedException(); }
        public string Add(string entity)
        {
            Collection.Add(entity);
            return entity;
        }
        public string Remove(string entity)
        {
            RemoveWasCalled = true;
            Collection.Remove(entity);
            return entity;
        }
        public string Attach(string entity) { throw new NotImplementedException(); }
        public string Create() { throw new NotImplementedException(); }

        TDerivedEntity IDbSet<string>.Create<TDerivedEntity>() { throw new NotImplementedException(); }

        public ObservableCollection<string> Local { get { throw new NotImplementedException(); } }
    }
}