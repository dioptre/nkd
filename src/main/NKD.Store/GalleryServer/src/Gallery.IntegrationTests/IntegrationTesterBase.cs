using System;
using Gallery.Core;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.DependencyResolution;
using Gallery.IntegrationTests.Helpers;
using NUnit.Framework;

namespace Gallery.IntegrationTests {
    [TestFixture]
    public abstract class IntegrationTesterBase<T> {
        protected readonly T Instance;

        protected IntegrationTesterBase(bool initializeDatabase = false) {
            RegisterDependencies();
            InitializeDatabase(initializeDatabase);
            RegisterMappings();
            Instance = IoC.Resolver.Resolve<T>();
        }

        private static void RegisterDependencies()
        {
            try
            {
                DependencyRegistrar.EnsureDependenciesRegistered();
                IoC.Resolver.Register<IHttpRuntime, FakeHttpRuntime>();
                IoC.Resolver.Register<IConfigSettings, IntegrationTestConfigSettings>();
                LoggerConfigurator.ConfigureLogging();
            }
            catch (Exception ex)
            {
                throw new IntegrationTesterBaseSetupFailedException("Dependency registration failed", ex);
            }
        }

        private static void InitializeDatabase(bool shouldInitialize)
        {
            if (!shouldInitialize) return;
            IDatabaseBootstrapper databaseBootstrapper;
            try
            {
                databaseBootstrapper = IoC.Resolver.Resolve<IDatabaseBootstrapper>();
            }
            catch (Exception ex)
            {
                throw new IntegrationTesterBaseSetupFailedException("IDatabaseBootstrapper resolution failed", ex);
            }
            try
            {
                databaseBootstrapper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                throw new IntegrationTesterBaseSetupFailedException("IDatabaseBootstrapper database initialization failed", ex);
            }
        }

        private static void RegisterMappings()
        {
            IMapperBootstrapper mapperBootstrapper;
            try
            {
                mapperBootstrapper = IoC.Resolver.Resolve<IMapperBootstrapper>();
            }
            catch (Exception ex)
            {
                throw new IntegrationTesterBaseSetupFailedException("IMapperBootstrapper resolution failed", ex);
            }
            try
            {
                mapperBootstrapper.RegisterMappings();
            }
            catch (Exception ex)
            {
                throw new IntegrationTesterBaseSetupFailedException("IMapperBootstrapper mapping registration failed", ex);
            }
        }
    }
}