using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Tasks.Scheduling;
using Orchard.Logging;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using NKD.Models;
using NKD.Helpers;

namespace NKD.Services
{
    [UsedImplicitly]
    public class MailApiScheduledTaskHandler : IScheduledTaskHandler
    {
        public const string TASK_TYPE_MAIL_API = "MailApiScheduled";
        private readonly IMailApiService _mailApi;

        public ILogger Logger { get; set; }

        public MailApiScheduledTaskHandler(IMailApiService mailAPI)
        {
            _mailApi = mailAPI;
            Logger = NullLogger.Instance;           
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TASK_TYPE_MAIL_API && context.Task.ContentItem != null)
            {
                try
                {
                    var p = context.Task.ContentItem.As<MailApiPart>();
                    _mailApi.ProcessApiRequest(p);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, e.Message);
                }
                finally
                {
                  
                }
            }
        }

    }
}