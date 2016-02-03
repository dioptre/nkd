using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NKD.Helpers
{
    public static class JsonHelper
    {

        public static dynamic ToDynamic(this HttpRequestBase request)
        {
            try
            {

                if (request.HttpMethod.ToLowerInvariant() == "get")
                {
                    return ((dynamic)JObject.Parse(JsonConvert.SerializeObject(request.Params)));

                }
                else
                {
                    request.InputStream.Seek(0, 0);
                    using (StreamReader sr = new StreamReader(request.InputStream))
                        return ((dynamic)JObject.Parse(sr.ReadToEnd()));
                }
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, object> ToDictionary(this NameValueCollection nvc, bool handleMultipleValuesPerKey = true)
        {
            var result = new Dictionary<string, object>();
            foreach (string key in nvc.Keys)
            {
                if (handleMultipleValuesPerKey)
                {
                    string[] values = nvc.GetValues(key);
                    if (values.Length == 1)
                    {
                        result.Add(key, values[0]);
                    }
                    else
                    {
                        result.Add(key, values);
                    }
                }
                else
                {
                    result.Add(key, nvc[key]);
                }
            }

            return result;
        }

        public static string ToJsonObject(this Dictionary<string, object> dict, bool handleMultipleValuesPerKey = true)
        {
            return JsonConvert.SerializeObject(dict, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });
        }
        public class JsonNetResult : JsonResult
        {
            public JsonNetResult(object data, JsonRequestBehavior jsonRequestBehavior)
                : base()
            {
                this.Data = data;
                this.JsonRequestBehavior = jsonRequestBehavior;
            }
            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                var response = context.HttpContext.Response;

                response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

                if (ContentEncoding != null)
                    response.ContentEncoding = ContentEncoding;

                if (Data == null)
                    return;

                // If you need special handling, you can call another form of SerializeObject below
                var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);
                response.Write(serializedObject);
            }
        }
    }
}