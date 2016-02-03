using System;

namespace Gallery.Core.Exceptions
{
    public class PluginImplementationNotFoundException<T> : Exception
    {
        public PluginImplementationNotFoundException(string pathToAssemblies)
            : base(string.Format("Could not find a type implementing '{0}' in assemblies in the directory '{1}'.", typeof(T).Name, pathToAssemblies))
        { }
    }
}