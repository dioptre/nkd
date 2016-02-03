using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.ComponentModel;

namespace NKD.Helpers
{
    public static class ObjectHelper
    {
        public static string CleanTokenForSQL(this string o)
        {
            if (o == null)
                return null;
            o = o.Trim();
            return o.Split(new string[] { " ", "\t", "\r", "\n", "'" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();            
        }

        public static byte[] ComputeHashBytes(this object o)
        {        
            SHA1 sha = new SHA1CryptoServiceProvider();
            var serializer = new DataContractJsonSerializer(o.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, o);
                return sha.ComputeHash(ms.ToArray());
            }
        }

        public static string ComputeHash(this object o)
        {
            return Encoding.UTF8.GetString(ComputeHashBytes(o));
        }

        public static string ToJson(this object o)
        {
            var serializer = new DataContractJsonSerializer(o.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, o);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static string ToJsonWithType(this object o)
        {
            JavaScriptSerializer js = new JavaScriptSerializer(new SimpleTypeResolver());
            return js.Serialize(o);
        }

        public static T Deserialize<T>(this string json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)(serializer.ReadObject(ms));
            }
        }

        public static object ConvertType(string stringValue, Type type)
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(type);
            return converter.IsValid(stringValue) ? converter.ConvertFrom(stringValue) : null;
        }

        public static object ConvertType(string stringValue, string type)
        {
            return ConvertType(stringValue, Type.GetType(type, true, true));
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            byte[] buffer = new byte[16384];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        public static string ToHexString(this byte[] bytes)
        {
            string sbinary = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                sbinary += bytes[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }

        public static T DeepCopy<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static T Mirror<T>(this T original, List<string> propertyExcludeList=null)
        {

            T destination = Activator.CreateInstance<T>();
            propertyExcludeList = propertyExcludeList ?? new List<string>();
            PropertyInfo[] piList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (PropertyInfo pi in piList)
            {
                if (!propertyExcludeList.Contains(pi.Name))
                {
                    if (pi.GetValue(destination, null) != pi.GetValue(original, null))
                    {
                        pi.SetValue(destination, pi.GetValue(original, null), null);
                    }
                }
            }
            return destination;

        }

        public static void Mirror<T>(this T original, ref T destination, List<string> propertyExcludeList=null, bool builtInOnly=true, bool publicOnly=true, bool excludeVersioning=true)
        {
            propertyExcludeList = propertyExcludeList ?? new List<string>();
            BindingFlags bf = BindingFlags.Public | BindingFlags.Instance;
            if (!publicOnly)
                bf = bf | BindingFlags.NonPublic;
            PropertyInfo[] piList = typeof(T).GetProperties(bf);
            foreach (PropertyInfo pi in piList)
            {
                if (builtInOnly && pi.PropertyType.Module.ScopeName != "CommonLanguageRuntimeLibrary")
                    continue;
                if (!(excludeVersioning && pi.Name.StartsWith("Version")) && !propertyExcludeList.Contains(pi.Name) && pi.CanWrite)
                {
                    if (pi.GetValue(destination, null) != pi.GetValue(original, null))
                    {
                        pi.SetValue(destination, pi.GetValue(original, null), null);
                    }
                }
            }
        }

        public static T Materialize<T>(this Dictionary<string,string> fields, List<string> propertyExcludeList = null, bool builtInOnly = true, bool publicOnly = true, bool excludeVersioning = true)
        {
            T destination = (T)Activator.CreateInstance(typeof(T));
            propertyExcludeList = propertyExcludeList ?? new List<string>();
            BindingFlags bf = BindingFlags.Public | BindingFlags.Instance;
            if (!publicOnly)
                bf = bf | BindingFlags.NonPublic;
            PropertyInfo[] piList = typeof(T).GetProperties(bf);
            foreach (PropertyInfo pi in piList)
            {
                if (builtInOnly && pi.PropertyType.Module.ScopeName != "CommonLanguageRuntimeLibrary")
                    continue;
                if (!(excludeVersioning && pi.Name.StartsWith("Version")) && !propertyExcludeList.Contains(pi.Name) && pi.CanWrite)
                {
                    string foundValue;
                    if (fields.TryGetValue(pi.Name, out foundValue))
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(pi.PropertyType);
                        pi.SetValue(destination, typeConverter.ConvertFromString(foundValue), null);
                    }
                }
            }
            return destination;
        }

        public static object GetDefaultValue(this Type t)
        {
            if (!t.IsValueType || Nullable.GetUnderlyingType(t) != null)
                return null;
            return Activator.CreateInstance(t);
        }

        public static T GetDefaultValue<T>()
        {
            return default(T);
        }
        
    }
}