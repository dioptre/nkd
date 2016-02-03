using System.Web;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class SystemHttpRuntime : IHttpRuntime
    {
        public string AppDomainAppPath { get { return HttpRuntime.AppDomainAppPath; } }
    }
}