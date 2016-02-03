using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.DependencyResolution
{
    internal class HashingProviderService
    {
        private readonly IConfigSettings _configSettings;

        public HashingProviderService(IConfigSettings configSettings)
        {
            _configSettings = configSettings;
        }

        public IHashingServiceProvider RegisterHashingProviderImplementer()
        {
            Type hashingServiceProviderType = typeof(IHashingServiceProvider);
            IEnumerable<Type> hashingServiceProviderImplementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract && p.IsClass && hashingServiceProviderType.IsAssignableFrom(p));

            HashingAlgorithm hashingAlgorithmToUse = HashingAlgorithm.FromName(_configSettings.HashAlgorithm);

            IHashingServiceProvider instance = hashingServiceProviderImplementers
                .Select(Activator.CreateInstance).OfType<IHashingServiceProvider>()
                .Where(instanceOfType => hashingAlgorithmToUse == instanceOfType.HashingAlgorithm)
                .FirstOrDefault();

            if (instance == null)
            {
                throw new Exception(string.Format("Could not find implementation for the HashingAlgorithm {0}.", hashingAlgorithmToUse));
            }
            return instance;
        }
    }
}