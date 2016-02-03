using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using System.Collections;

namespace NKD.Helpers
{
    public static class DynamicHelper
    {

        public static dynamic Mirror<T>(T original)
        {
            
            var duplicate = new ExpandoObject();
            var dict = duplicate as IDictionary<string, Object>;
            PropertyInfo[] piList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in piList)
            {
                try
                {
                    dict.Add(p.Name, p.GetValue(original, null));
                }
                catch
                {                    
                }
            }
            return duplicate;
        }
    }

    public class NullableExpandoObject : DynamicObject
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!values.TryGetValue(binder.Name, out result))
                result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            values[binder.Name] = value;
            return true;
        }
    }
}
