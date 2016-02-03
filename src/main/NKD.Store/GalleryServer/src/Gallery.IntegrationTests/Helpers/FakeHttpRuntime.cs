using System;
using System.IO;
using System.Reflection;
using Gallery.Core.Interfaces;

namespace Gallery.IntegrationTests.Helpers
{
    public class FakeHttpRuntime : IHttpRuntime
    {
        public string AppDomainAppPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}