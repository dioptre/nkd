using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Settings;
using Orchard.Tasks.Scheduling;
using Orchard.ContentManagement;
using System.Transactions;
using System.Threading.Tasks;
using Autofac.Integration.Mvc;
using System.Dynamic;
using NKD.Models;
using NKD.Services;

namespace NKD.Helpers
{
    public static class TaskHelper
    {

        public static void MailApiAsync(this IScheduledTaskManager _taskManager, ContentItem contentItem)
        {
            var tasks = _taskManager.GetTasks(Services.MailApiScheduledTaskHandler.TASK_TYPE_MAIL_API);
            if (tasks == null || tasks.Count() < 100)
                _taskManager.CreateTask(Services.MailApiScheduledTaskHandler.TASK_TYPE_MAIL_API, DateTime.UtcNow, contentItem);
        }

        public static void EmailAsync(this IScheduledTaskManager _taskManager, ContentItem contentItem)
        {
           
            var tasks = _taskManager.GetTasks(Services.EmailScheduledTaskHandler.TASK_TYPE_EMAIL);
            if (tasks == null || tasks.Count() < 100)
                _taskManager.CreateTask(Services.EmailScheduledTaskHandler.TASK_TYPE_EMAIL, DateTime.UtcNow, contentItem);
        }

       
        public static void ProcessAsync(this ShellTask taskInfo)
        {
            ConcurrentTaskService.ProcessAsync(taskInfo);
        }

     
     
    }

}