using System;
using System.IO;
using System.Reflection;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl {
    public class MigrationBootstrapper : IDatabaseBootstrapper {
        private readonly IConfigSettings _configSettings;
        private string _provider;
        private string _connectionString;
        private string _migrationsAssemblyPath;

        public MigrationBootstrapper(IConfigSettings configSettings) {
            _configSettings = configSettings;
        }

        public void InitializeDatabase() {
            _provider = _configSettings.MigratorProvider;
            _connectionString = _configSettings.ConnectionStringSettings("GalleryFeedEntities").ConnectionString;
            _migrationsAssemblyPath = Path.Combine(_configSettings.PhysicalSitePath, _configSettings.RelativeAssemblyDirectory, "Gallery.Migrations.dll");

            if (ProviderIsSqlSeverCe()) {
                CreateSqlCeDatabaseFile(_connectionString);
            }
            Migrate();
        }

        private bool ProviderIsSqlSeverCe()
        {
            return _provider.ToUpper() == "SQLSERVERCE";
        }

        private void Migrate() {
            CheckArguments();
            Migrator.Migrator mig = GetMigrator();
            mig.MigrateToLastVersion();
        }

        private Migrator.Migrator GetMigrator() {
            var asm = Assembly.LoadFrom(_migrationsAssemblyPath);

            var migrator = new Migrator.Migrator(_provider, _connectionString, asm);
            return migrator;
        }

        private void CheckArguments() {
            if (_provider == null)
                throw new ArgumentException("Provider value missing", "Provider");
            if (_connectionString == null)
                throw new ArgumentException("Connection string missing", "ConnectionString");
            if (_migrationsAssemblyPath == null)
                throw new ArgumentException("Migrations assembly missing", "MigrationsAssembly");
        }

        private void CreateSqlCeDatabaseFile(string connectionString) {
            // We want to execute this code using Reflection, to avoid having a binary
            // dependency on SqlCe assembly, so it's not loaded unless necessary.

            const string assemblyName = "System.Data.SqlServerCe";
            const string typeName = "System.Data.SqlServerCe.SqlCeEngine";

            var sqlceEngineHandle = Activator.CreateInstance(assemblyName, typeName);
            var engine = sqlceEngineHandle.Unwrap();

            engine.GetType().GetProperty("LocalConnectionString").SetValue(engine, connectionString, null/*index*/);

            try {
                engine.GetType().GetMethod("CreateDatabase").Invoke(engine, null);
            }
            catch (TargetInvocationException ex) {
                if (ex.InnerException == null) throw;
                if (!ex.InnerException.Message.Contains("File already exists")) {
                    throw;
                }
            }
            finally {
                engine.GetType().GetMethod("Dispose").Invoke(engine, null);
            }
        }
    }
}