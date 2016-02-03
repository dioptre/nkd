using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Ninject.Extensions.Logging;

namespace Gallery.Server
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class PackageLogService : ServiceBase
    {
        private readonly IRepository<PackageLogEntry> _packageLogRepository;
        private readonly IConfigSettings _configSettings;

        public PackageLogService(IRepository<PackageLogEntry> packageLogRepository, IWebFaultExceptionCreator webFaultExceptionCreator,
            IConfigSettings configSettings, ILogger logger)
            : base(webFaultExceptionCreator, logger)
        {
            _packageLogRepository = packageLogRepository;
            _configSettings = configSettings;
        }

        [WebGet(UriTemplate = "{lastLogId}", ResponseFormat = WebMessageFormat.Json)]
        public List<PackageLogEntry> GetNewPackageLogEntries(string lastLogId)
        {
            return ExecuteAction(() => GetNewPackageLogs(lastLogId), "Could not retrieve list of new Package Log Entries");
        }

        private List<PackageLogEntry> GetNewPackageLogs(string lastLogId)
        {
            int parsedLogId;
            if (!int.TryParse(lastLogId, out parsedLogId))
            {
                throw new WebFaultException<string>("Invalid integer value given for the last log Id", HttpStatusCode.BadRequest);
            }
            return _packageLogRepository.Collection.Where(plr => plr.Id > parsedLogId).Take(_configSettings.MaxPackageLogEntryRecordCount).ToList();
        }
    }
}