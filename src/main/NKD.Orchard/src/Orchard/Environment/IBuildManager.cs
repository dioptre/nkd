using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using System;
using Orchard.Exceptions;

namespace Orchard.Environment {
    public interface IBuildManager : IDependency {
        IEnumerable<Assembly> GetReferencedAssemblies();
        bool HasReferencedAssembly(string name);
        Assembly GetReferencedAssembly(string name);
        Assembly GetCompiledAssembly(string virtualPath);
    }

    public class DefaultBuildManager : IBuildManager {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IAssemblyLoader _assemblyLoader;

        public ILogger Logger { get; set; }

        public DefaultBuildManager(
            IVirtualPathProvider virtualPathProvider, 
            IAssemblyLoader assemblyLoader) {

            _virtualPathProvider = virtualPathProvider;
            _assemblyLoader = assemblyLoader;

            Logger = NullLogger.Instance;
        }

        public IEnumerable<Assembly> GetReferencedAssemblies() {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>();
        }

        public bool HasReferencedAssembly(string name) {
            var assemblyPath = _virtualPathProvider.Combine("~/bin", name + ".dll");
            return _virtualPathProvider.FileExists(assemblyPath);
        }

        public Assembly GetReferencedAssembly(string name) {
            if (!HasReferencedAssembly(name))
                return null;

            return _assemblyLoader.Load(name);
        }


        public Assembly GetCompiledAssembly(string virtualPath)
        {
            try
            {
                //ignore files with .dll extension from dynamic compilation 
                var extension = System.IO.Path.GetExtension(virtualPath);
                if (extension != null && !extension.Equals(".dll"))
                {
                    return BuildManager.GetCompiledAssembly(virtualPath);

                }
            }
            catch (Exception ex)
            {
                //log compilation error
                Logger.Log(LogLevel.Error, ex, "Error during dynamic compilation. Error Message: {0} ", ex.ToString());

                return null;
            }

            return null;
        }
    }
}