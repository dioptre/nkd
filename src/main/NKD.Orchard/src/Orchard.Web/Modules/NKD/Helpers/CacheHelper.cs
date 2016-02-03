using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Caching;
using NKD.Module.BusinessObjects;

namespace NKD.Helpers
{
    public static class CacheHelper
    {
        public enum CacheType {
            Original,
            Preview
        }

        private static HttpRuntime _httpRuntime;

        public static Cache Cache
        {
            get
            {
                EnsureHttpRuntime();
                return HttpRuntime.Cache;
            }
        }

        public static FileData AddFileDataToCache(FileData file, string cacheKey = null, CacheType? ct = null, TimeSpan? expiry = null)
        {
            if (expiry == null)
                expiry = CacheHelper.DefaultTimeout;
            if (string.IsNullOrWhiteSpace(cacheKey))
                cacheKey = string.Format("{0}-{1}", file.FileDataID, ct);
            return CacheHelper.AddToCache<FileData>(() => { return file; }, cacheKey, expiry);
        }

        public static T AddToCache<T>(this Func<object> toRun, string cacheKey, TimeSpan? expires = null)
        {
            object c = CacheHelper.Cache[cacheKey];
            if (c == null)
            {
                c = toRun.Invoke();
                if (!expires.HasValue)
                    CacheHelper.Cache.Insert(cacheKey, c, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                else
                    CacheHelper.Cache.Insert(cacheKey, c, null, System.Web.Caching.Cache.NoAbsoluteExpiration, expires.Value, System.Web.Caching.CacheItemPriority.Normal, null);
            }
            return (T)c;
        }


        private static void EnsureHttpRuntime()
        {
            if (null == _httpRuntime)
            {
                try
                {
                    Monitor.Enter(typeof(CacheHelper));
                    if (null == _httpRuntime)
                    {
                        // Create an Http Content to give us access to the cache.
                        _httpRuntime = new HttpRuntime();
                    }
                }
                finally
                {
                    Monitor.Exit(typeof(CacheHelper));
                }
            }
        }

        private static TimeSpan? _defaultTimeout = null;
        public static TimeSpan DefaultTimeout
        {
            get
            {
                if (!_defaultTimeout.HasValue)
                {
                    try
                    {
                        Monitor.Enter(typeof(CacheHelper));
                        int timeout;
                        if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["CacheTimeOut"], out timeout))
                            timeout = 0;   //No Cache 
                        _defaultTimeout = new TimeSpan(0,0,timeout);
                    }
                    catch
                    {
                        _defaultTimeout = new TimeSpan(0,0,0);
                    }
                    finally
                    {
                        Monitor.Exit(typeof(CacheHelper));
                    }

                }
                return _defaultTimeout.Value;
            }
        }
    }
}
