using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Caching;
using Orchard.FileSystems.Media;
namespace NKD.Helpers
{
    public static class FileHelper
    {
        public static string GetAbsolutePath(this IStorageProvider sp, string relativePath)
        {
            return System.Web.HttpContext.Current.Server.MapPath(sp.GetPublicUrl(relativePath));
        }
    }

}
