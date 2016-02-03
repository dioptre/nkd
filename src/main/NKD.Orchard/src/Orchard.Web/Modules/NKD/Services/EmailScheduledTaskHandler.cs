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
    public class EmailScheduledTaskHandler : IScheduledTaskHandler
    {
        public const string TASK_TYPE_EMAIL = "EmailScheduled";
        private readonly IUsersService _userService;

        public ILogger Logger { get; set; }

        public EmailScheduledTaskHandler(IUsersService userService)
        {
            _userService = userService;
            Logger = NullLogger.Instance;           
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TASK_TYPE_EMAIL && context.Task.ContentItem != null)
            {
                try
                {
                    var em = context.Task.ContentItem.As<EmailPart>();
                    _userService.EmailUsers(em.Recipients.SplitStringArray(), em.Subject, em.Body, em.Retry, em.ForwardSupport, em.FromAddress, em.FromName, em.HideRecipients);
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