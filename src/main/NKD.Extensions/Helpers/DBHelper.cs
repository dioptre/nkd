using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Threading;
using System.Data;
using System.Data.Sql;
using System.Data.EntityClient;
using System.Data.Common;
using System.Data.SqlClient;

namespace NKD.Helpers
{
    public static class DBHelper
    {
        public static PropertyInfo GetPrimaryKey<T>()
        {
            PropertyInfo[] infos = typeof(T).GetProperties();
            foreach (PropertyInfo info in infos)
            {
                var column = info.GetCustomAttributes(false)
                 .Where(x => x.GetType() == typeof(ColumnAttribute))
                 .FirstOrDefault(x =>
                  ((ColumnAttribute)x).IsPrimaryKey &&
                  ((ColumnAttribute)x).DbType.Contains("NOT NULL"));
                if (column != null)
                {
                    return info;
                }
            }
            return null;
        }

        private static int? _defaultTimeout = null;
        public static int DefaultTimeout
        {
            get
            {
                if (_defaultTimeout == null)
                {
                    try
                    {
                        Monitor.Enter(typeof(DBHelper));
                        int timeout;
                        if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["SqlCommandTimeOut"], out timeout))
                            timeout = 600;   //10 Minutes 
                        _defaultTimeout = timeout;
                    }
                    catch
                    {
                        _defaultTimeout = 600;
                    }
                    finally
                    {
                        Monitor.Exit(typeof(DBHelper));
                    }
                   
                }
                return _defaultTimeout.Value;
            }
        }

        public static string GetEnityConnectionString(string providerConnectionString, string entityType, string assembly, string nameSpace)
        {
            if (string.IsNullOrWhiteSpace(assembly))
                assembly = "*";
            if (string.IsNullOrWhiteSpace(nameSpace))
                nameSpace = "Models";
            System.Data.SqlClient.SqlConnectionStringBuilder scsb = new System.Data.SqlClient.SqlConnectionStringBuilder(providerConnectionString);
            EntityConnectionStringBuilder ecb = new EntityConnectionStringBuilder();
            ecb.Metadata = string.Format(@"res://{0}/{1}.{2}.csdl|res://{0}/{1}.{2}.ssdl|res://{0}/{1}.{2}.msl", assembly, nameSpace, entityType);
            ecb.Provider = "System.Data.SqlClient";
            ecb.ProviderConnectionString = scsb.ConnectionString;
            return ecb.ConnectionString;
        }

        public static string GetEnityConnectionString(string providerConnectionString, string entityType, string directory)
        {
            System.Data.SqlClient.SqlConnectionStringBuilder scsb = new System.Data.SqlClient.SqlConnectionStringBuilder(providerConnectionString);
            EntityConnectionStringBuilder ecb = new EntityConnectionStringBuilder();
            ecb.Metadata = string.Format(@"metadata={0}\{1}.csdl|{0}\{1}.ssdl|{0}\{1}.msl", directory, entityType);
            ecb.Provider = "System.Data.SqlClient";
            ecb.ProviderConnectionString = scsb.ConnectionString;
            return ecb.ConnectionString;
        }

        public static string GetEnityConnectionString(string providerConnectionString, string metadata)
        {
            System.Data.SqlClient.SqlConnectionStringBuilder scsb = new System.Data.SqlClient.SqlConnectionStringBuilder(providerConnectionString);
            EntityConnectionStringBuilder ecb = new EntityConnectionStringBuilder();
            ecb.Metadata = metadata;
            ecb.Provider = "System.Data.SqlClient";
            ecb.ProviderConnectionString = scsb.ConnectionString;
            return ecb.ConnectionString;
        }


        //TODO: Fix hack
        //http://stackoverflow.com/questions/995478/sql-server-full-text-search-escape-characters
        public static string FixSearchTerm(this string term)
        {
            term = string.Format("{0}", term);
            var terms = term.Split(new string[] { "\"", "'", "\t", "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);
            return string.Format("\"{0}\"", string.Join("\" AND \"", terms));
        }
    }
}