using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ionic.Zip;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using NKD.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Validation;
using Orchard;
using System.Transactions;
using NKD.Helpers;
using System.Data;
using NKD.Module.BusinessObjects;


namespace NKD.Services
{
    [UsedImplicitly]
    public class PrivateDataService : IPrivateDataService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IUsersService _users;
        public Localizer _localizer { get; set; }
        public PrivateDataService(IOrchardServices orchardServices, IUsersService users)
        {
            _users = users;
            _orchardServices = orchardServices;
            _localizer = NullLocalizer.Instance;
        }

        //singleton
        public static PrivateDataService NullService = new PrivateDataService(null, null);

        public T GetObject<T>(string lookup)
        {
            var o = GetObject(lookup);
            if (o == null)
                return default(T);
            return (T)o; //leave like this, if bad type then cast will throw an error
        }

        public object GetObject(string lookup)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var o = d.PrivateDatas.Where(x => x.UniqueID == lookup).FirstOrDefault();
                if (o == null)
                    return null;
                return ObjectHelper.ConvertType(o.Value, o.SystemDataType);
            }
        }

        public Guid? GetKey(string lookup)
        {
            return GetObject<Guid?>(lookup);
        }

        public MetaData[] GetMetadata(Guid referenceID, Func<MetaData, bool> predicate = null)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var o = from r in d.MetaDataRelations where r.ReferenceID==referenceID join m in d.MetaDatas
                                                      on r.MetaDataID equals m.MetaDataID
                                                      select m;
                if (predicate != null)
                    return o.Where(predicate).ToArray();
                else return o.ToArray();
            }
        }

        public T GetFirstMetadata<T>(Guid referenceID, Func<MetaData, bool> predicate = null)
        {
            var m = GetMetadata(referenceID, predicate).OrderByDescending(f=>f.VersionUpdated).Where(f=>f.VersionDeletedBy == null).FirstOrDefault();
            if (m != null)
                return m.ContentToIndex.Deserialize<T>();
            else
                return default(T);
        }

        private const string guid_lookup_string = "NKD_GUID_LOG";
        private const string guid_log_string = "e8ac1711-df77-4738-82d3-63253177f0f8";
        private static Guid? guid_log = null;

        public void SetObject(string lookup, string value, string tabletype = null, string usertype = null, string systemdatatype = null )
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var o = d.PrivateDatas.Where(x => x.UniqueID == lookup).FirstOrDefault();
                if (o == null)
                {
                    o = new PrivateData { PrivateDataID= Guid.NewGuid(), UniqueID = lookup, Value = value, TableType = tabletype, UserDataType = usertype, SystemDataType = systemdatatype, VersionUpdated = DateTime.UtcNow };
                    d.PrivateDatas.AddObject(o);
                    d.SaveChanges();

                    //Assume this is always a string/guid TODO, may want to change later
                    string cmd = string.Format("insert into {0} ({1}) values('{2}')", 
                                                    tabletype.CleanTokenForSQL(),
                                                    GetPrimaryKeys(tabletype.CleanTokenForSQL())[0],
                                                    value.CleanTokenForSQL());
                    try
                    {
                        d.Connection.Open();
                        using (var command = d.Connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = cmd;
                            command.ExecuteNonQuery();
                        }
                    }
                    catch { } //We tried!
                    finally
                    {
                        d.Connection.Close();
                    }
                    
                    
                }
                
            }
        }

        public string[] GetPrimaryKeys(string tablename)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var cmd = d.Connection.CreateCommand();
                cmd.CommandText = "SELECT column_name FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1 AND table_name = '" + tablename.CleanTokenForSQL() + "'";
                var t = new DataTable("t");
                t.Columns.Add("column_name", typeof(string));
                try
                {
                    d.Connection.Open();
                    t.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges);
                    return (from o in t.Select() select o[0] as string).ToArray();
                }
                finally
                {
                    d.Connection.Close();
                }
            }
        }

        
        
    }
}