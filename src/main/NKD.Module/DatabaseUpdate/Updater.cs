using System;
using System.IO;
using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Security;
using Ionic.Zip;
using System.Data.SqlClient;

namespace NKD.Module.DatabaseUpdate
{
    public class Updater : ModuleUpdater
    {
        public static int CurrentVersion { get { return 22; } }      
        public static string DefaultConnectionString { get; set; }
        public static bool UpdateData { get; set; }
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) 
        { 
            //var x = "c:\\Program Files\\Microsoft SQL Server\\120\\Tools\\Binn\\SqlLocalDB.exe create \"NKD\" -s";
            DefaultConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;        
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
        }

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            object o = null;
            if (IsTableExists("X_PrivateData"))
                o = ExecuteScalarCommand("select [value] from X_PrivateData where UniqueID='NKDSchemaVersion'", false);
            int nkdSchemaVersion = (o == null) ? -1 : Convert.ToInt32(o);
            if (nkdSchemaVersion == -1)
            {
                //Restore from clean DB
                //ExecuteNonQueryCommand(Properties.Resources.NKDSchema1, false);
                //ExecuteNonQueryCommand(Properties.Resources.NKDSchema1Data, false);
                //Do backup
                RestoreSQLFromZip(DefaultConnectionString);
                TryReboot();
                return;
            }
            else if (nkdSchemaVersion < CurrentVersion && System.Windows.Forms.Application.ProductName.Contains("Win") && System.Windows.Forms.Application.ProductName.Contains("NKD"))
            {
                //Do backup
                try
                {
                    var f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "nkd.bak");
                    foreach (var s in string.Format(Properties.Resources.NKDBackup, f).Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries)) ExecuteNonQueryCommand(s, false);
                }
                catch { }
            }
            //if (nkdSchemaVersion < 2)
            //    foreach (var s in Properties.Resources.NKDSchema2.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries)) ExecuteNonQueryCommand(s, true);
            //if (nkdSchemaVersion < 3)
            //    foreach (var s in Properties.Resources.NKDSchema3.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries)) ExecuteNonQueryCommand(s, true); //This may have errors
            //if (nkdSchemaVersion < 4)
            //{
            //    ExecuteSQLFromZip("v4.schema.sql.zip");
            //    if (UpdateData)
            //        ExecuteSQLFromZip("v4.data.sql.zip"); //Query too large to execute - users can do it themselves
            //}

            if (nkdSchemaVersion < CurrentVersion)
                TryReboot();
            if (nkdSchemaVersion > CurrentVersion)
            {
                var result = System.Windows.Forms.MessageBox.Show("Your database version is newer than your application.\r\nThis could lead to an error or data corruption.", "Shutdown Application?", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.OK)
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
          
            base.UpdateDatabaseBeforeUpdateSchema();
        }

        protected override System.Data.IDbCommand CreateCommand(string commandText)
        {
            return base.CreateCommand(commandText);
        }

        private void TryReboot()
        {
            //Reboot
            try
            {
                if (System.Windows.Forms.Application.ProductName.Contains("Win") && System.Windows.Forms.Application.ProductName.Contains("NKD"))
                {
                    System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
            catch { }
        }


        public static void RestoreSQLFromZip(string xstring)
        {
            string file = string.Format("v{0}.bak.zip", CurrentVersion);
            file = string.Format(@"{0}\Resources\{1}", AppDomain.CurrentDomain.BaseDirectory, file);
            // extract file to a temp location
            using (var fileInflater = ZipFile.Read(file))
            {
                foreach (ZipEntry entry in fileInflater)
                {
                    if (entry == null) { continue; }

                    if (!entry.IsDirectory && !string.IsNullOrEmpty(entry.FileName))
                    {
                        var guid = Guid.NewGuid();
                        var f = Path.Combine(System.IO.Path.GetTempPath(), string.Format("{0}nkd_.bak", guid));
                        var db = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), string.Format("{0}nkd.mdf", guid));
                        var log = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), string.Format("{0}nkd.ldf", guid));


                        using (Stream stream = entry.OpenReader())
                        using (FileStream bakf = new FileStream(f, FileMode.CreateNew, FileAccess.Write))
                        {
                            //Write
                            byte[] buffer = new byte[8 * 1024];
                            int len;
                            while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bakf.Write(buffer, 0, len);
                            }
                        }
                        using (SqlConnection sql = new SqlConnection())
                        {
                            sql.ConnectionString = xstring;
                            sql.ConnectionString = sql.ConnectionString.Replace("Initial Catalog=NKD", "Initial Catalog=master"); //TODO: Fix: hack atm
                            sql.Open();
                            //Restore
                            foreach (var s in string.Format(Properties.Resources.NKDRestore, f, db, log).Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var cmd = new SqlCommand(s, sql);
                                cmd.CommandTimeout = 60000;
                                cmd.ExecuteNonQuery();
                            }
                            sql.Close();
                        }

                    }
                    break; // Only handle 1 file
                }
            }

        }

        private void ExecuteSQLFromZip(string file)
        {
            using (FileStream fs = new FileStream(string.Format(@"{0}\Resources\{1}", AppDomain.CurrentDomain.BaseDirectory, file), FileMode.Open, FileAccess.Read))
            {
                // extract file to a temp location
                using (var fileInflater = ZipFile.Read(fs))
                {
                    foreach (ZipEntry entry in fileInflater)
                    {
                        if (entry == null) { continue; }

                        if (!entry.IsDirectory && !string.IsNullOrEmpty(entry.FileName))
                        {
                            var paragraph = "";
                            using (Stream stream = entry.OpenReader())
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                int lines = 0;
                                while (sr.Peek() >= 0)
                                {
                                    var line = sr.ReadLine();
                                    if (line.Trim().ToUpper() == "GO")
                                    {
                                        ExecuteNonQueryCommand(paragraph, true);
                                        paragraph = "";
                                        lines = 0;
                                    }
                                    else if (lines > 1000 && line.ToLowerInvariant().IndexOf("insert") == 0)
                                    {
                                        ExecuteNonQueryCommand(paragraph, true);
                                        paragraph = line;
                                        lines = 0;
                                    }
                                    else
                                    {
                                        paragraph += string.Format("{0}\r\n", line);
                                        lines++;
                                    }
                                }
                            }
                            if (paragraph.Length > 0)
                                ExecuteNonQueryCommand(paragraph, true);
                        }
                        break; // Only handle 1 file
                    }
                }
            }
        }

    }
}
