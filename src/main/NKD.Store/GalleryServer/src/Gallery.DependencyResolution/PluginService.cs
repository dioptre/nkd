using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;

namespace Gallery.DependencyResolution
{
    public class PluginService
    {
        private readonly IConfigSettings _configSettings;

        public PluginService(IConfigSettings configSettings)
        {
            _configSettings = configSettings;
        }

        public TBase FindPluginImplementation<TBase>()
           where TBase : class
        {
            string pathToAssemblies = Path.Combine(_configSettings.PhysicalSitePath, _configSettings.RelativePluginAssemblyDirectory);
            string[] assemblyFileNames = Directory.GetFileSystemEntries(pathToAssemblies, "*.dll");
            var types = from t in assemblyFileNames.Where(afn => !afn.Contains("Gallery.Core")).Select(Assembly.LoadFrom).SelectMany(a => a.GetTypes())
                        where !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(TBase))
                        select t;
            if (!types.Any())
            {
                throw new PluginImplementationNotFoundException<TBase>(pathToAssemblies);
            }
            if (types.Count() > 1)
            {
                string baseTypeName = typeof (TBase).Name;
                string matchingTypes = string.Join("\n", types.Select(t => string.Format("{0} in {1}", t.Name, t.Assembly.FullName)));
                throw new AmbiguousMatchException(string.Format("Multiple implementations of {0} were found.\nMatching types: {1}", baseTypeName, matchingTypes));
            }
            return Activator.CreateInstance(types.Single()) as TBase;
        }
    }
}