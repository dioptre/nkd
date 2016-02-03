using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Ionic.Zip;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using NKD.Module.BusinessObjects;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Validation;
using Orchard;
using System.Security.Principal;
using Orchard.Roles.Services;
using Orchard.Roles.Models;
using Orchard.Users.Services;
using Orchard.Users.Models;
using System.Text.RegularExpressions;
using System.Transactions;
using Orchard.Messaging.Services;
using Orchard.Logging;
using NKD.Helpers;
using Orchard.Tasks.Scheduling;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Utility.Extensions;
using System.Web.Configuration;
using HtmlAgilityPack;
using System.Net;
using System.Management;
using Orchard.Caching;
using Orchard.Services;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using NKD.Models;
using System.Threading;
using Orchard.Users.Events;



using ls = LumiSoft.MailServer.API.UserAPI;


namespace NKD.Services {
    
    [UsedImplicitly]
    public class MailApiService : Orchard.Users.Events.IUserEventHandler, IMailApiService
    {
        private readonly IContentManager _contentManager;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IConcurrentTaskService _concurrentTasks;
        public ILogger Logger { get; set; }

        public MailApiService(
            IContentManager contentManager, 
            IScheduledTaskManager taskManager, 
            IConcurrentTaskService concurrentTasks) 
        {
            _contentManager = contentManager;
            _taskManager = taskManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _concurrentTasks = concurrentTasks;
        }

        public Localizer T { get; set; }

        private string mailIP = null;
        private string _mailIP { get { if (mailIP == null) { mailIP = System.Configuration.ConfigurationManager.AppSettings["MailIP"] ?? "localhost"; } return mailIP; } }
        private string mailAdminUsername = null;
        private string _mailAdminUsername { get { if (mailAdminUsername == null) { mailAdminUsername = System.Configuration.ConfigurationManager.AppSettings["MailAdminUsername"] ?? "Administrator"; } return mailAdminUsername; } }
        private string mailAdminPassword = null;
        private string _mailAdminPassword { get { if (mailAdminPassword == null) { mailAdminPassword = System.Configuration.ConfigurationManager.AppSettings["MailAdminPassword"] ?? ""; } return mailAdminPassword; } }
      

        public void AddMemberToMailingList(Contact c, string mailingListName)
        {
            throw new NotImplementedException();
        }

        public void AddEmailAlias(string mailUsername, string alias)
        {
            throw new NotImplementedException();
        }


        public void RemoveEmailAlias(string mailUsername, string alias)
        {
            throw new NotImplementedException();
        }


        public void ProcessApiRequest(MailApiPart request)
        {
            using (ls.Server lsMailServer = new ls.Server())
            {
                lsMailServer.Connect(_mailIP, _mailAdminUsername, _mailAdminPassword);
                if (request.ApiMethod == EnumHelper.EnumToString(MailApiCall.AliasAdd))
                {
                    //add new email (val) to alias username(=email) (key)
                    // Loop all virtual servers
                    foreach (ls.VirtualServer virtualServer in lsMailServer.VirtualServers)
                    {
                        foreach (ls.User user in virtualServer.Users)
                        {
                            if (user != null)
                            {
                                try
                                {
                                    user.EmailAddresses.Add(request.ApiValue);
                                }
                                catch { }
                                break;
                            }
                        }
                    }
                }
                else if (request.ApiMethod == EnumHelper.EnumToString(MailApiCall.AliasRemove))
                {
                }
                else if (request.ApiMethod == EnumHelper.EnumToString(MailApiCall.MailingListAdd))
                {
                    //add new email (val) to list (key)
                }
                else if (request.ApiMethod == EnumHelper.EnumToString(MailApiCall.MailingListRemove))
                {
                }
                
            }
            request.Completed = DateTime.UtcNow;
        }


        public void ProcessApiRequestAsync(MailApiCall apiMethod, string apiKey, string apiValue = null, string json = null, int status = 0, DateTime? processed = default(DateTime?), DateTime? completed = default(DateTime?), bool shortLived = true)
        {
            try
            {
                var m = _contentManager.New<MailApiPart>("MailApi");
                m.Status = status;
                m.Processed = DateTime.UtcNow;
                m.ApiKey = apiKey;
                m.ApiValue= apiValue;
                m.Json = json;
                m.ApiMethod = EnumHelper.EnumToString(apiMethod);
                _contentManager.Create(m);

                if (!shortLived)
                {
                    _concurrentTasks.ExecuteAsyncTask(ProcessApiRequest, m.ContentItem);
                }
                else
                {
                    _taskManager.MailApiAsync(m.ContentItem);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, string.Format("Failed Mail Api Request.\n\nKey:\n{0}\n\nValue:\n{1}\n\nJson:\n{2}\n\n", apiMethod, apiKey, apiValue, json));
            }

        }

        private void ProcessApiRequest(ContentItem c)
        {
            var m = c.As<MailApiPart>();
            ProcessApiRequest(m);
        }

        //Events

        public void Creating(UserContext context) { }

        public void Created(UserContext context) {
            //_contentManager.Flush();
            //Add users to Mailing List
        }

        //TODO:Check
        public void LoggedIn(IUser user) { }

        public void LoggedOut(IUser user) { }

        public void AccessDenied(IUser user) { }

        public void ChangedPassword(IUser user) { }

        public void SentChallengeEmail(IUser user) { }

        public void ConfirmedEmail(IUser user) { }

        public void Approved(IUser user) { }

         

    }
}
