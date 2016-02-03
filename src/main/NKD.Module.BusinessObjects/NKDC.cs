using System;
using System.ComponentModel;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.Configuration;
using System.Transactions;

namespace NKD.Module.BusinessObjects
{
    public partial class NKDC : ObjectContext
    {
      
        public const string EF_METADATA = "res://NKD.Module.BusinessObjects/NKD.csdl|res://NKD.Module.BusinessObjects/NKD.ssdl|res://NKD.Module.BusinessObjects/NKD.msl";        
        private static string xStringDefault = null; //Never use this
        public static string XStringDefault
        {
            get
            {
                if (xStringDefault == null)
                {
                    var xs = System.Configuration.ConfigurationManager.ConnectionStrings["NKD"];
                    if (xs == null)
                        xs = System.Configuration.ConfigurationManager.ConnectionStrings["NKDC"];
                    if (xs == null)
                        xs = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"];
                    if (xs != null)
                        xStringDefault = xs.ConnectionString;
                    else
                        xStringDefault = "Server=(LocalDB)\v11.0;Integrated Security=true;Initial Catalog=NKD";
                }
                return xStringDefault;
            }
        }

        private static Object defaultVersionLock = new Object();
        private static int? defaultVersionOnInit = DefaultVersionAsync;
        private static int? defaultVersionAsync = null;
        public static int? DefaultVersionAsync
        {
            get
            {
                lock (defaultVersionLock)
                {
                    if (defaultVersionAsync == null)
                    {
                        defaultVersionAsync = -1; //Invalid, Searching
                        getVersionAsync();
                    }
                }
                return defaultVersionAsync;
            }           
        }
        private static int? getVersion()
        {
            try
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var x = new NKDC(XStringDefault, EF_METADATA, false);
                    var v = (from o in x.PrivateDatas where o.UniqueID=="NKDSchemaVersion" orderby o.VersionUpdated select o.Value).FirstOrDefault();
                    int i;
                    if (int.TryParse(v, out i))
                        return i;
                    else
                        return 0; //Invalid, Bad Result
                }
            }
            catch { }
            finally
            {
                //xStringDefault = null; //Reset this for incoming application //TODO Check 
            }
            
            return null;
        }

        private static async System.Threading.Tasks.Task<int?> getVersionAsync(bool checkOnly = false)
        {
            var task = new System.Threading.Tasks.Task<int?>(getVersion);
            if (!checkOnly)
                task.ContinueWith((antecedent) =>
                {
                    lock (defaultVersionLock)
                    {
                        defaultVersionAsync = antecedent.Result;
                    }
                });
            task.Start();
            return await task;
        }

        
        public NKDC(string providerConnectionString, string metadata, bool checkPrimaryKey = true) 
            : base((string.IsNullOrWhiteSpace(metadata)) ? GetEnityConnectionString(providerConnectionString ?? XStringDefault, EF_METADATA) : GetEnityConnectionString(providerConnectionString ?? XStringDefault, metadata), "NKDC" ) 
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            _checkPrimaryKey = checkPrimaryKey;
            if (_checkPrimaryKey)
                OnContextCreated();
        }

        private bool _checkPrimaryKey = true;
        public bool CheckPrimaryKey
        {
            get { return _checkPrimaryKey; }
            set
            {
                if (value != _checkPrimaryKey)
                {
                    if (value)
                        this.SavingChanges += NKDC_SavingChanges;
                    else
                        this.SavingChanges -= NKDC_SavingChanges;
                }
                _checkPrimaryKey = value;
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
        partial void OnContextCreated()
        {
            this.SavingChanges += NKDC_SavingChanges;
        }

        void NKDC_SavingChanges(object sender, EventArgs e)
        {

            var c = this.ObjectStateManager
            .GetObjectStateEntries(System.Data.EntityState.Added)
            .Select(entry => (System.Data.Objects.DataClasses.EntityObject)entry.Entity);

            var t = this.MetadataWorkspace.GetEntityContainer(this.DefaultContainerName, System.Data.Metadata.Edm.DataSpace.CSpace);
            foreach (var o in c)
            {
                var key = t.BaseEntitySets[o.EntityKey.EntitySetName].ElementType.KeyMembers[0];
                if (((System.Data.Metadata.Edm.PrimitiveType)key.TypeUsage.EdmType).ClrEquivalentType == typeof(Guid))
                {
                    PropertyInfo prop = o.GetType().GetProperty(key.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (null != prop && prop.CanWrite && (Guid)prop.GetValue(o) == Guid.Empty)
                    {
                        prop.SetValue(o, Guid.NewGuid(), null);
                    }
                }
            }
        }
    }

}
