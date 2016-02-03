using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace NKD.Helpers
{
    public static class LinqHelper
    {
        public static Func<A, R> Y<A, R>(Func<Func<A, R>, Func<A, R>> f)
        {
            return t => f(Y(f))(t);
        }

        public static IEnumerable<TSource> Recurse<TSource>(TSource start,
                                                  Func<TSource, TSource> step)
        {
            TSource current = start;
            while (true)
            {
                yield return current;
                current = step(current);
            }
        }
        
        public static DataReaderEnumerable AsEnumerable(this IDataReader source)
        {
            return new DataReaderEnumerable(source);
        }

        public static IEnumerable<Dictionary<string, object>> AsEnumerableDictionary(this System.Data.IDataReader source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            while (source.Read())
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int i = 0; i < source.FieldCount; i++)
                {
                    object value = source.GetValue(i);
                    // return an empty string for dbnull value of field type string
                    if (source.GetFieldType(i) == typeof(string) && source.IsDBNull(i))
                        value = string.Empty;
                    row.Add(source.GetName(i), value);
                }
                yield return row;
            }
        }

        public class FuncEqualityComparer<T> : IEqualityComparer<T>
        {
            readonly Func<T, T, bool> _comparer;
            readonly Func<T, int> _hash;

            public FuncEqualityComparer(Func<T, T, bool> comparer)
                : this(comparer, t => 0) // NB Cannot assume anything about how e.g., t.GetHashCode() interacts with the comparer's behavior
            {
            }

            public FuncEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
            {
                _comparer = comparer;
                _hash = hash;
            }

            public bool Equals(T x, T y)
            {
                return _comparer(x, y);
            }

            public int GetHashCode(T obj)
            {
                return _hash(obj);
            }
        }
    }
}