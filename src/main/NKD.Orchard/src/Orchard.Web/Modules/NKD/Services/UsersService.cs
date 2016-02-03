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
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
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
using NKD.ViewModels;
using Newtonsoft.Json;
using System.Web.Mvc;
using EntityFramework.Extensions;
using Orchard.Mvc.Extensions;


namespace NKD.Services {
    
    [UsedImplicitly]
    public class UsersService : IUsersService, IAuthority, Orchard.Users.Events.IUserEventHandler
    {
        public static readonly Guid COMPANY_DEFAULT = new Guid("6887ABC9-E2D8-4A2D-B143-6C3E5245C565");

        private readonly IOrchardServices _orchardServices;
        private readonly IUserService _user;
        private readonly IMembershipService _membership;
        private readonly IContentManager _contentManager;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly IRoleService _roleService;
        private readonly IMessageService _messageService;
        private readonly IScheduledTaskManager _taskManager;
        private PrincipalContext _securityContext;
        private readonly ShellSettings _shellSettings;
        private readonly ISignals _signals;
        private readonly IClock _clock;
        private readonly ICacheManager _cache;
        private PrincipalContext securityContext
        {
            get
            {
                if (_securityContext == null)
                    _securityContext = new PrincipalContext(ContextType.Domain); //TODO: May look at others one day
                return _securityContext;
            }
        }
        private readonly IRepository<EmailPartRecord> _emailRepository;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IRepository<UserPartRecord> _userPartRepository;
        public ILogger Logger { get; set; }

        public UsersService(
            IContentManager contentManager,
            IContentManagerSession contentManagerSession,
            IOrchardServices orchardServices, 
            IRoleService roleService, 
            IMessageService messageService, 
            IScheduledTaskManager taskManager, 
            IRepository<EmailPartRecord> emailRepository, 
            ShellSettings shellSettings, 
            IRepository<UserRolesPartRecord> userRolesRepository, 
            ICacheManager cache, 
            IClock clock, 
            ISignals signals,
            IRepository<UserPartRecord> userPartRepository,
            IMembershipService membershipService,
            IUserService userService
            ) 
        {
            _signals = signals;
            _clock = clock;
            _cache = cache;
            _emailRepository = emailRepository;
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _contentManagerSession = contentManagerSession;
            _roleService = roleService;
            _messageService = messageService;
            _taskManager = taskManager;
            _shellSettings = shellSettings;
            _userRolesRepository = userRolesRepository;
            _userPartRepository = userPartRepository;
            _membership = membershipService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;      
            _user = userService;
        }

        public Localizer T { get; set; }

        private System.Web.Configuration.AuthenticationMode? authenticationMode = null;
        public System.Web.Configuration.AuthenticationMode AuthenticationMode
        {
            get 
            {
                if (authenticationMode == null)
                {
                    var configuration = WebConfigurationManager.OpenWebConfiguration("/");
                    authenticationMode = ((AuthenticationSection)configuration.GetSection("system.web/authentication")).Mode;
                }
                return authenticationMode.Value;
            }
        }

        public string Username
        {
            get
            {

                if (_orchardServices.WorkContext != null && _orchardServices.WorkContext.CurrentUser != null)
                    return _orchardServices.WorkContext.CurrentUser.UserName;
                else return null;
            }
        }

        private string email = null;
        public string Email
        {
            get
            {
                if (email == null)
                {
                    if (Contact != null && !string.IsNullOrWhiteSpace(Contact.DefaultEmail))
                        email = Contact.DefaultEmail;
                    else if (_orchardServices.WorkContext != null && _orchardServices.WorkContext.CurrentUser != null)
                        email = _orchardServices.WorkContext.CurrentUser.Email;
                    //TODO Could check full email list?
                }
                return email;                
            }
        }

        private Contact contact = null;
        public Contact Contact
        {
            get
            {
                if (contact == null)
                {
                    contact = GetContact(Username);
                    contactID = contact.ContactID;
                }
                return contact;
            }
        }

        private Guid? contactID = null;
        public Guid? ContactID
        {
            get
            {
                if (contactID == null)
                {
                    contactID = GetContactID(Username);
                }
                return contactID;
            }
        }

        private Guid? applicationCompanyID;
        public Guid ApplicationCompanyID
        {
            get
            {
                if (applicationCompanyID == null)
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var d = new NKDC(ApplicationConnectionString, null);
                        var c = (from o in d.Licenses orderby o.Expiry descending 
                                 where o.ApplicationID == ApplicationID && o.CompanyID != null
                                 && o.Version == 0 && o.VersionDeletedBy == null
                                 select o.CompanyID).FirstOrDefault();
                        if (c.HasValue)
                            applicationCompanyID = c.Value;
                        else
                            applicationCompanyID = COMPANY_DEFAULT; //DEFAULT company GUID
                    }
                }
                return applicationCompanyID.Value;

            }
        }

        private Guid? defaultContactCompanyID;
        public Guid DefaultContactCompanyID
        {
            get
            {
                if (defaultContactCompanyID == null)
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var d = new NKDC(ApplicationConnectionString, null);
                        var c = (from o in d.Experiences orderby o.VersionUpdated descending 
                                 where  
                                 (o.DateFinished==null || o.DateFinished > DateTime.UtcNow)
                                 && (o.Expiry == null || o.Expiry > DateTime.UtcNow)
                                 && o.CompanyID != null 
                                 && o.ContactID == ContactID
                                 && (o.DateStart <= DateTime.UtcNow || o.DateStart == null)
                                 && o.Version==0 && o.VersionDeletedBy==null
                                 select o.CompanyID).FirstOrDefault();
                        if (c.HasValue)
                            defaultContactCompanyID = c.Value;
                        else
                            defaultContactCompanyID = COMPANY_DEFAULT; //DEFAULT company GUID
                    }
                }
                return defaultContactCompanyID.Value;

            }
        }

        private Tuple<Guid,string> defaultContactCompany = null;
        public Tuple<Guid, string> DefaultContactCompany
        {
            get
            {
                if (defaultContactCompany == null)
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var d = new NKDC(ApplicationConnectionString, null);
                        var c = (from o in d.Experiences
                                 orderby o.VersionUpdated descending
                                 where
                                 (o.DateFinished == null || o.DateFinished > DateTime.UtcNow)
                                 && (o.Expiry == null || o.Expiry > DateTime.UtcNow)
                                 && o.CompanyID != null
                                 && o.ContactID == ContactID
                                 && (o.DateStart <= DateTime.UtcNow || o.DateStart == null)
                                 && o.Version == 0 && o.VersionDeletedBy == null
                                 select new { o.CompanyID.Value, o.Company.CompanyName }).FirstOrDefault();
                        if (c != null)
                        {

                            defaultContactCompanyID = c.Value;
                            defaultContactCompany = new Tuple<Guid, string>(c.Value, c.CompanyName);
                        }
                        else
                        {
                            defaultContactCompanyID = COMPANY_DEFAULT; //DEFAULT company GUID
                            defaultContactCompany = new Tuple<Guid, string>(COMPANY_DEFAULT, "PUBLIC");
                        }
                    }
                }
                return defaultContactCompany;
            }
        }


        private bool? hasPrivateCompanyID;
        public bool HasPrivateCompanyID
        {
            get
            {
                if (hasPrivateCompanyID == null)
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var d = new NKDC(ApplicationConnectionString, null);
                        hasPrivateCompanyID = (from o in d.Experiences
                                 where
                                 (o.DateFinished == null || o.DateFinished > DateTime.UtcNow)
                                 && (o.Expiry == null || o.Expiry > DateTime.UtcNow)
                                 && o.CompanyID != null
                                 && o.ContactID == ContactID
                                 && (o.DateStart <= DateTime.UtcNow || o.DateStart == null)
                                 && o.Version == 0 && o.VersionDeletedBy == null
                                 && o.CompanyID!=COMPANY_DEFAULT
                                 select o.CompanyID).Any();

                    }
                }
                return hasPrivateCompanyID.Value;
            }
        }



        private Guid?[] contactCompanies = null;
        /// <summary>
        /// Does not return recursive list.
        /// </summary>
        public Guid?[] ContactCompanies
        {
            get
            {
                if (contactCompanies == null)
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        Guid?[] defaultCompanies;
                        if (DefaultPublicDomain)
                        {
                            defaultCompanies = new Guid?[] { default(Guid?), COMPANY_DEFAULT };
                        }
                        else
                        {
                            defaultCompanies = new Guid?[] { COMPANY_DEFAULT };
                        }
                        var d = new NKDC(ApplicationConnectionString, null);

                        contactCompanies = (from o in d.Experiences
                                            orderby o.VersionUpdated descending
                                            where
                                            (o.DateFinished == null || o.DateFinished > DateTime.UtcNow)
                                            && (o.Expiry == null || o.Expiry > DateTime.UtcNow)
                                            && o.CompanyID != null
                                            && o.ContactID == ContactID
                                            && (o.DateStart <= DateTime.UtcNow || o.DateStart == null)
                                            && o.Version == 0 && o.VersionDeletedBy == null
                                            select o.CompanyID).AsEnumerable().Concat(defaultCompanies).Distinct().ToArray();
                        if (!contactCompanies.Any())
                            contactCompanies = defaultCompanies; //Default company GUID
                    }
                }
                return contactCompanies;
            }

            
        }


        private Guid? applicationID = null;
        public Guid ApplicationID
        {
            get
            {
                if (applicationID == null)
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var c = new NKDC(ApplicationConnectionString,null);
                        applicationID = (from o in c.Applications where o.ApplicationName == _shellSettings.Name select o.ApplicationId).FirstOrDefault();
                        if (applicationID == default(Guid))
                        {
                            applicationID = Guid.NewGuid();
                            var application = new Applications();
                            application.ApplicationId = applicationID.Value;
                            application.ApplicationName = _shellSettings.Name;
                            application.LoweredApplicationName = _shellSettings.Name.ToLower();
                            application.Description = "Orchard";
                            c.Applications.AddObject(application);
                            c.SaveChanges();
                        }
                    }
                }
                return applicationID.Value;
            }
        }

        private Guid? serverID = null;
        public Guid ServerID
        {
            get
            {
                if (!serverID.HasValue)
                {
                    try
                    {
                        var hostnames = new List<string>();
                        var ips = new List<string>();
                        string sid1 = "", sid2 = "", sid3 = "", pub = null;
                        string domain = null;

                        try
                        {
                            domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                        }
                        catch { }

                        var h = Dns.GetHostName();
                        var iph = Dns.GetHostEntry(h);
                        var ip = iph.AddressList;
                        hostnames.Add(h);
                        ips.AddRange(from o in ip select o.ToString());


                        h = Environment.MachineName;
                        iph = Dns.GetHostEntry(h);
                        ip = iph.AddressList;
                        hostnames.Add(h);
                        ips.AddRange(from o in ip select o.ToString());


                        var i = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                        h = i.Substring(i.LastIndexOf('/') + 1, i.Length - i.LastIndexOf('/') - 1);
                        hostnames.Add(h);
                        try
                        {
                            iph = Dns.GetHostEntry(h);
                            ip = iph.AddressList;
                            ips.AddRange(from o in ip select o.ToString());
                        }
                        catch { }

                        try
                        {
                            //Public IP
                            var check = "http://checkip.dyndns.org";
                            var p = WebRequest.GetSystemWebProxy();
                            var c = new WebClient();
                            c.Headers.Add("user-agent", "Lynx/2.8.8dev.3 libwww-FM/2.14 SSL-MM/1.4.1");
                            c.Proxy = p;
                            c.Credentials = CredentialCache.DefaultNetworkCredentials;
                            var d = c.OpenRead(check);
                            //HtmlWeb web = new HtmlWeb();
                            HtmlDocument doc = new HtmlDocument(); //web.Load(, "GET", , );
                            doc.Load(d);
                            var n = doc.DocumentNode.SelectSingleNode("/html/body");
                            pub = n.InnerText.Trim();
                            pub = pub.Substring(pub.LastIndexOf(' ') + 1, pub.Length - pub.LastIndexOf(' ') - 1);
                            ips.Add(pub);
                        }
                        catch { }

                        try
                        {
                            sid1 = new SecurityIdentifier((byte[])new DirectoryEntry(string.Format("WinNT://{0},Computer", Environment.MachineName)).Children.Cast<DirectoryEntry>().First().InvokeGet("objectSID"), 0).AccountDomainSid.Value;
                        }
                        catch { }

                        try
                        {
                            //using (var mo = new ManagementObject(String.Format("Win32_UserAccount.Name='{0}',Domain='{1}'", "administrator", Environment.MachineName)))
                            //{
                            //    mo.Get();
                            //    sid2 = mo["SID"].ToString();
                            //    sid2 = sid2.Substring(0, sid2.Length - 4);
                            //}

                            byte[] domainSid;

                            var directoryContext = new DirectoryContext(DirectoryContextType.Domain, domain);

                            using (var dom = Domain.GetDomain(directoryContext))
                            using (var directoryEntry = dom.GetDirectoryEntry())
                                domainSid = (byte[])directoryEntry.Properties["objectSid"].Value;
                            sid2 = new SecurityIdentifier(domainSid, 0).AccountDomainSid.Value;
                        }
                        catch { }


                        try
                        {
                            var cpus = new List<string>();
                            foreach (ManagementObject mo in new ManagementClass("win32_processor").GetInstances())
                            {
                                cpus.Add(mo.Properties["processorID"].Value.ToString());
                            }
                            sid3 = cpus.OrderBy(f => f).ToArray().FlattenStringArray();
                        }
                        catch { }

                        var servers = from xip in ips
                                      from xh in hostnames
                                      select new { domain, ip = xip, host = xh, sid1, sid2, sid3 };

                        var ss = (from o in servers.Distinct() select new { newid = Guid.NewGuid(), o.domain, o.ip, o.host, o.sid1, o.sid2, o.sid3 }).ToArray();

                        using (new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            var sw = new NKDC(ApplicationConnectionString,null);
                            //Update all server info
                            //Existing
                            var sos = (from o in sw.Servers
                                       where
                                       o.Domain == domain
                                       && o.ServerUniqueMachineCode1 == sid1
                                       && o.ServerUniqueMachineCode2 == sid2
                                       && o.ServerUniqueMachineCode3 == sid3
                                       select o).ToArray();
                            var os = (from o in sos
                                      from s in ss
                                      where
                                      o.Domain == s.domain
                                      && o.IP == s.ip
                                      && o.Hostname == s.host
                                      && o.ServerUniqueMachineCode1 == s.sid1
                                      && o.ServerUniqueMachineCode2 == s.sid2
                                      && o.ServerUniqueMachineCode3 == s.sid3
                                      select new { s.newid, o.ServerID });
                            //New
                            var ns = from x in ss where !(from o in os select o.newid).Contains(x.newid) select x;
                            //insert
                            foreach (var s in ns)
                            {
                                var xs = new Server();
                                xs.ServerID = s.newid;
                                xs.Domain = s.domain;
                                xs.Hostname = s.host;
                                xs.IP = s.ip;
                                xs.ServerUniqueMachineCode1 = s.sid1;
                                xs.ServerUniqueMachineCode2 = s.sid2;
                                xs.ServerUniqueMachineCode3 = s.sid3;
                                sw.Servers.AddObject(xs);
                            }
                            sw.SaveChanges();

                            //Update ServerApplication
                            //Merge new and existing
                            var sa = (from xsa in (from o in os select o.ServerID).Union(from o in ns select o.newid) select new { ServerID = xsa, ApplicationID }).Distinct();
                            var sai = (from o in sa select new { newid = Guid.NewGuid(), o.ServerID, o.ApplicationID }).ToArray();
                            //old
                            var osas = (from o in sw.ServerApplications
                                        where o.ApplicationID == ApplicationID
                                        select o).ToArray();


                            var osa = (from o in osas
                                       from s in sai
                                       where o.ApplicationID == s.ApplicationID && o.ServerID == s.ServerID
                                       select new { o.ServerApplicationID, s.newid });
                            //new
                            var nsa = from x in sai where !(from o in osa select o.newid).Contains(x.newid) select x;
                            //insert
                            foreach (var s in nsa)
                            {
                                var xs = new ServerApplication();
                                xs.ServerApplicationID = s.newid;
                                xs.ServerID = s.ServerID;
                                xs.ApplicationID = s.ApplicationID;
                                sw.ServerApplications.AddObject(xs);
                            }
                            sw.SaveChanges();

                            //Choose 1
                            var xsid = (from o in sw.ServerApplications
                                        from x in sw.Servers
                                        where o.ServerID == x.ServerID && o.ApplicationID == ApplicationID
                                        select x).ToArray();
                            if (string.IsNullOrEmpty(pub))
                                serverID = (from o in xsid orderby NetworkHelper.IsLocal(o.IP) ascending, IPAddress.Parse(o.IP).IPAsLong(), o.Domain, o.Hostname descending where o.Hostname == Environment.MachineName select o.ServerID).FirstOrDefault();
                            else
                                serverID = (from o in xsid orderby NetworkHelper.IsLocal(o.IP) ascending, IPAddress.Parse(o.IP).IPAsLong(), o.Domain, o.Hostname descending where o.Hostname == Environment.MachineName && o.IP == pub select o.ServerID).FirstOrDefault();
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Information(ex, "Could not update server fingerprint. Corresponding licenses may fail.");
                    }
                }
                return serverID.Value;

            }
        }

        private string applicationConnectionString = null;
        public string ApplicationConnectionString //TODO: This needs to be multi-tenant?
        {
            get
            {
                if (applicationConnectionString == null)
                    applicationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NKD"].ConnectionString;

                return applicationConnectionString;
            }
        }

        //Default True
        private bool? defaultPublicDomain = null;
        public bool DefaultPublicDomain //TODO: This needs to be multi-tenant?
        {
            get
            {
                if (!defaultPublicDomain.HasValue)
                    defaultPublicDomain = string.Format("{0}", System.Configuration.ConfigurationManager.AppSettings["AllowPublic"]).ToLowerInvariant() != "false";
                return defaultPublicDomain.Value;
            }
        }
        public void GetCreator(Guid? contact, Guid? company, out Guid? creatorContact, out Guid? creatorCompany)
        {

            if (DefaultPublicDomain && company == COMPANY_DEFAULT)
            {
                creatorContact = default(Guid?);
                creatorCompany = default(Guid?);
            }
            else
            {
                creatorContact = contact;
                creatorCompany = company;
            }

        }

        //private Object syncUsersLock = new Object();
        //private static Guid syncUsersFirstThread = Guid.Empty;
        //private static Guid syncUsersLastThread = Guid.Empty;
        //private static ManualResetEvent mre = new ManualResetEvent(false);
        //public void SyncUsersManual()
        //{
        //    //Thread t = new Thread(new ParameterizedThreadStart(this.SyncUsersManualThread));
        //    //t.Start(Guid.NewGuid());

        //    bool acquiredLock = false;
        //    Guid syncThread = Guid.NewGuid();
        //    Guid syncThreadGroup;
        //    try
        //    {
        //        lock (syncUsersLock)
        //        {
        //            syncUsersLastThread = syncThread;
        //            Monitor.TryEnter(syncUsersFirstThread, ref acquiredLock);
        //        }
        //        if (acquiredLock)
        //        {
        //            syncUsersFirstThread = syncThread;
        //            SyncUsers();
        //            lock (syncUsersLock)
        //            {
        //                Monitor.Exit(syncUsersFirstThread);
        //                mre.Set();
        //                mre.Reset();
        //            }
        //        }
        //        else
        //        {
        //            lock (syncUsersLock)
        //            {
        //                syncThreadGroup = syncUsersFirstThread;
        //            }
        //            mre.WaitOne();
        //            bool repeat = false;
        //            lock (syncUsersLock)
        //            {
        //                if (syncUsersLastThread == syncThread)
        //                    Monitor.Enter(syncUsersFirstThread, ref acquiredLock);
        //            }
        //            if (repeat)
        //                SyncUsersManual();

        //        }
        //    }
        //    catch
        //    {
        //        if (acquiredLock)
        //        {
        //            Monitor.Exit(syncUsersFirstThread);
        //        }
        //    }

        //}


        public void SyncUsers()
        {

            //Get Orchard Users & Roles
            var orchardUsers = _contentManager.Query<UserPart, UserPartRecord>().List();
            var orchardRoles = _roleService.GetRoles().ToArray();
            var orchardUserRoles = (from xur in  _userRolesRepository.Table.ToArray()
                                   join xu in orchardUsers on xur.UserId equals xu.Id
                                   join xr in orchardRoles on xur.Role.Id equals xr.Id
                                   select new {xu.UserName, RoleName=xr.Name}).ToArray();
            //Get Authmode & Then Update
            if (AuthenticationMode == System.Web.Configuration.AuthenticationMode.Forms)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var c = new NKDC(ApplicationConnectionString,null);
                    c.Users.Where(f => !(from o in c.Contacts select o.AspNetUserID).Contains(f.UserId)).Delete();

                    var r = from o in c.Roles.Include("Users") where o.ApplicationId == ApplicationID select o;
                    var u = from o in c.Users.Include("Roles") where o.ApplicationId == ApplicationID select o;
                    var updated = DateTime.UtcNow;
                    //New User
                    var nu = (from o in orchardUsers where !(from ou in u select ou.UserName).Contains(o.UserName) select o);
                    foreach (var n in nu)
                    {
                        var user = new Users();
                        user.UserId = Guid.NewGuid();
                        user.UserName = n.UserName;
                        user.ApplicationId = ApplicationID;
                        user.LoweredUserName = n.UserName.ToLower();
                        user.LastActivityDate = updated;                       
                        c.Users.AddObject(user);
                        var contacts = (from o in c.Contacts where o.Username == user.UserName select o);
                        foreach (var nc in contacts)
                        {
                            nc.AspNetUserID = user.UserId;
                        }
                        if (!contacts.Any())
                        {
                            var contact = new Contact();
                            contact.ContactID = Guid.NewGuid();
                            contact.Username = user.UserName;
                            contact.AspNetUserID = user.UserId;
                            contact.DefaultEmail = n.Email;
                            contact.ContactName = string.Format("Site User: {0}", user.UserName);
                            contact.VersionUpdated = updated;
                            contact.Surname = "";
                            contact.Firstname = "";
                            c.Contacts.AddObject(contact);
                        }
                    }
                    //New Role
                    var nr = (from o in orchardRoles where !(from or in r select or.RoleName).Contains(o.Name) select o);
                    foreach (var n in nr)
                    {
                        var role = new Roles();
                        role.RoleName = n.Name;
                        role.ApplicationId = ApplicationID;
                        role.RoleId = Guid.NewGuid();
                        role.LoweredRoleName = n.Name.ToLower();
                        c.Roles.AddObject(role);
                    }
                    c.SaveChanges();
                    var users = c.Users.Include("Roles").Where(f => f.ApplicationId == ApplicationID).ToArray();
                    var roles = c.Roles.Where(f => f.ApplicationId == ApplicationID).ToArray();
                    foreach (var user in users)
                    {
                        foreach (var role in user.Roles.AsEnumerable())
                        {
                            //Remove
                            if (!orchardUserRoles.Any(f => f.RoleName == role.RoleName && f.UserName == user.UserName))
                            {
                                c.E_SP_DropUserRole(user.UserId, role.RoleId);
                            }                           
                        }

                        
                        var newRoleIds = (from o in orchardUserRoles where !user.Roles.Select(f=>f.RoleName).Contains(o.RoleName) && o.UserName==user.UserName
                                          join m in roles on o.RoleName equals m.RoleName select m);
                        foreach (var newRoleId in newRoleIds)                        
                        {
                            c.E_SP_AddUserRole(user.UserId, newRoleId.RoleId);
                            //user.Roles.Add(newRoleId);
                        }
                    }


                    c.SaveChanges();
                    
                    //TODO Update per application
                    //var ru = (from o in u.ToArray() where !(from ou in orchardUsers select ou.UserName).Contains(o.UserName) select o); //can just delete from users table
                    //foreach (var rem in ru)
                    //{
                    //    //c.Users.DeleteObject(rem); //Doesn't work for multitenancy
                    //}
                    //c.SaveChanges();
                }

            }
            else if (AuthenticationMode == System.Web.Configuration.AuthenticationMode.Windows)
            {
                //Module syncs only users - only all admin for now

                //Get AD Users
                // throw new NotImplementedException();
                // get a DirectorySearcher object
                DirectorySearcher search = new DirectorySearcher();

                // specify the search filter
                search.Filter = "(&(objectCategory=person)(objectClass=user))";
                //search.Filter = "(&(objectClass=user)(anr=agrosser))"; //TEST

                //// specify which property values to return in the search
                search.PropertiesToLoad.Add("name");   // first name
                search.PropertiesToLoad.Add("givenName");   // first name
                search.PropertiesToLoad.Add("sn");          // last name
                search.PropertiesToLoad.Add("mail");        // smtp mail address
                search.PropertiesToLoad.Add("samaccountname");        // account name
                search.PropertiesToLoad.Add("memberof");        // groups
                search.PropertiesToLoad.Add("objectsid");
                search.PropertiesToLoad.Add("objectguid");
                search.PropertiesToLoad.Add("title");

                // perform the search
                SearchResultCollection results = search.FindAll(); //.FindOne();

                var sessionRoleCache = new Dictionary<string, string>();
                var adusers = from SearchResult o in results
                              select new
                                  {
                                      name = o.Properties["name"] != null && o.Properties["name"].Count > 0 ? string.Format("{0}", o.Properties["name"][0]) : null,
                                      givenName = o.Properties["givenName"] != null && o.Properties["givenName"].Count > 0 ? string.Format("{0}", o.Properties["givenName"][0]) : null,
                                      sn = o.Properties["sn"] != null && o.Properties["sn"].Count > 0 ? string.Format("{0}", o.Properties["sn"][0]) : null,
                                      email = o.Properties["mail"] != null && o.Properties["mail"].Count > 0 ? string.Format("{0}", o.Properties["mail"][0]) : null,
                                      samaccountname = o.Properties["samaccountname"] != null && o.Properties["samaccountname"].Count > 0 ? string.Format("{0}", o.Properties["samaccountname"][0]) : null,
                                      username = o.Properties["objectsid"] != null && o.Properties["objectsid"].Count > 0 ? ((NTAccount)(new SecurityIdentifier((byte[])o.Properties["objectsid"][0], 0)).Translate(typeof(NTAccount))).ToString() : null,
                                      guid = o.Properties["objectguid"] != null && o.Properties["objectguid"].Count > 0 ? new Guid((byte[])o.Properties["objectguid"][0]) : (Guid?)null,
                                      title = o.Properties["title"] != null && o.Properties["title"].Count > 0 ? string.Format("{0}", o.Properties["title"][0]) : null,
                                      roles = o.Properties["memberof"] != null ? (from string m in o.Properties["memberof"] select getNameFromFQDN(m, sessionRoleCache)).ToArray() : new string[] { }
                                  };

                //Get NKD Users
                Contact[] nkdusers;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(ApplicationConnectionString,null);
                    nkdusers = (from o in d.Contacts select o).ToArray();

                    //Sync AD, Orchard, NKD
                    //New into NKD
                    //We need firstname, surname
                    var ad_new = (from o in adusers where o.givenName != null && o.sn != null && (o.guid.HasValue && !(from x in nkdusers select x.ContactID).Contains((Guid)o.guid)) || (!o.guid.HasValue && !(from x in nkdusers select x.Username.ToLowerInvariant()).Contains(o.username.ToLowerInvariant())) select o);

                    foreach (var o in ad_new)
                    {
                        Contact c = new Contact();
                        c.ContactID = o.guid.HasValue ? o.guid.Value : Guid.NewGuid();
                        c.Username = o.username;
                        c.Firstname = o.givenName;
                        c.ContactName = string.Join(string.Empty, string.Format("{0} [{1}]", o.name, o.username).Take(120));
                        c.Surname = o.sn;
                        c.DefaultEmail = o.email;
                        d.Contacts.AddObject(c);
                    }

                    //Updates into NKD
                    var ad_diff = from o in adusers
                                  from x in nkdusers
                                  where ((o.guid.HasValue && o.guid.Value == x.ContactID) || (o.username != null && x.Username != null && o.username.ToLowerInvariant() == x.Username.ToLowerInvariant()))
                                      //Things to update
                                          && (
                                          o.givenName != x.Firstname
                                          || o.sn != x.Surname
                                          || o.email != x.DefaultEmail
                                          || o.name != x.ContactName
                                  )
                                  select new { x.ContactID, o.givenName, o.sn, o.email, o.name, o.username };
                    foreach (var o in ad_diff)
                    {
                        var c = nkdusers.First(x => x.ContactID == o.ContactID);
                        c.Firstname = o.givenName;
                        c.ContactName = string.Join(string.Empty, string.Format("{0} [{1}]", o.name, o.username).Take(120));
                        c.Surname = o.sn;
                        c.DefaultEmail = o.email;
                    }
                    d.SaveChanges();

                }
            }


        }

        public IEnumerable<Contact> GetContacts()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var dataContext = new NKDC(ApplicationConnectionString,null);
                return dataContext.Contacts.OrderBy(x=>x.ContactName).ToArray();
            }
        }

        public Dictionary<Guid, string> GetRoles()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString,null);
                var r = (from o in c.Applications
                         join a in c.Roles on o.ApplicationId equals a.ApplicationId
                         select new { RoleName = a.RoleName + " (" + o.ApplicationName + ")", a.RoleId });
                return r.ToDictionary(f=>f.RoleId, f=>f.RoleName);
            }
        }

        public Dictionary<Guid,string> GetCompanies()
        {
            var allCompanies = new Dictionary<Guid,string>();
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString,null);
                using (DataTable table = new DataTable())
                {
                    using (var con = new SqlConnection(ApplicationConnectionString))
                    using (var cmd = new SqlCommand("X_SP_GetCompanies", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        da.Fill(table);
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        var rowRoot = (Guid)row[1];
                        var companyName = "";
                        Guid? lastKey = null;
                        for (int i = 2; i < table.Columns.Count; i += 2)
                        {
                            companyName += (string)row[i];
                            var checking = (Guid)row[i+1];
                            if (lastKey.HasValue && lastKey.Value == checking)
                                break;
                            lastKey = checking;
                            if (!allCompanies.ContainsKey(checking))
                                allCompanies.Add(checking, companyName);
                            companyName += " - ";
                        }
                    }
                }
            }
            return (from o in allCompanies orderby o.Value select o).ToDictionary(f=>f.Key, f=>f.Value);
        }

        public string[] GetUserEmails(Guid[] users)
        {
            if (users == null || users.Length == 0)
                return new string[] { };
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString,null);
                var o = from c in d.Contacts where users.Contains(c.ContactID) && c.DefaultEmail != null
                        select c.DefaultEmail;
                return o.ToArray();
            }
        }

        public Guid? GetEmailContactID(string email, bool validated = true)
        {
            if (email == null)
                return null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null);
                if (!validated)
                {
                    var id = (from c in d.Contacts join u in d.Users on c.AspNetUserID equals u.UserId 
                              where u.ApplicationId == ApplicationID && email == c.DefaultEmail && c.Version == 0 && c.VersionDeletedBy == null 
                              select c.ContactID).FirstOrDefault();
                    if (id != default(Guid))
                        return id;
                    id = (from c in d.Contacts 
                            join u in d.Users on c.AspNetUserID equals u.UserId 
                            join e in d.ContactEmailsViews on c.ContactID equals e.ContactID
                            where u.ApplicationId == ApplicationID && email == e.Email && c.Version == 0 && c.VersionDeletedBy == null 
                          select c.ContactID).FirstOrDefault();
                    if (id != default(Guid))
                        return id;
                    else
                        return null;                   
                }
                else
                    return (from c in d.Contacts join u in d.Users on c.AspNetUserID equals u.UserId 
                            where u.ApplicationId == ApplicationID && email == c.DefaultEmail && c.DefaultEmailValidated != null && c.Version == 0 && c.VersionDeletedBy == null select c.ContactID).Single();
            }
        }

        public Guid? GetContactID(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString,null);
                return (from c in d.Contacts join u in d.Users on c.AspNetUserID equals u.UserId where u.ApplicationId == ApplicationID where username==c.Username && c.Version == 0 && c.VersionDeletedBy == null select c.ContactID).SingleOrDefault();
                //return d.Contacts.Where(x=>x.Username == username).Select(x=>x.ContactID).FirstOrDefault();
            }
        }

        public Guid? GetContactID(Guid? userID)
        {
            if (Guid.Empty == userID || !userID.HasValue)
                return null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null);
                return (from c in d.Contacts join u in d.Users on c.AspNetUserID equals u.UserId where u.ApplicationId == ApplicationID where u.UserId == userID && c.Version == 0 && c.VersionDeletedBy == null select c.ContactID).SingleOrDefault();
                //return d.Contacts.Where(x=>x.Username == username).Select(x=>x.ContactID).FirstOrDefault();
            }
        }

        public ContactViewModel GetMyInfo()
        {
            string [] roles = new string[] {};
            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                roles = ((ContentItem)_orchardServices.WorkContext.CurrentUser.ContentItem).As<IUserRoles>().Roles.ToArray();
            }
            var application = ApplicationID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null);
                var m = (from o in d.E_SP_GetUserInfo(application, Username, null) select o).FirstOrDefault();
                if (m == null)
                    return null;
                else
                {
                    var emptyco = new[] { new { id = "", name = "" } };
                    var companies = emptyco.Where(f => 1 == 0);
                    if (m.companies != null)
                        companies = JsonConvert.DeserializeAnonymousType(m.companies,emptyco);
                    var emptyli = new[] { new { id = default(Guid?), expiry = default(DateTime?), modelid = default(Guid?), modelname = "", modelrestrictions = "", partid = default(Guid?), partname = "", partrestrictions = "" } };
                    var licenses = emptyli.Where(f => 1 == 0);
                    if (m.Licenses != null)
                        licenses = JsonConvert.DeserializeAnonymousType(m.Licenses, emptyli);
                    return new ContactViewModel
                    {
                        UserName = m.Username,
                        UserID = m.AspNetUserID,
                        CurrentCompanyID = m.CompanyID,
                        CurrentCompany = m.CompanyName,
                        ContactID = m.ContactID,
                        ContactName = m.ContactName,
                        IsPartner = (m.IsPartner > 0) ? true : false,
                        IsSubscriber = (m.IsSubscriber > 0) ? true : false,
                        Companies = (from o in companies select new SelectListItem { Text = o.name, Value = o.id, Selected = (string.Format("{0}", m.CompanyID).ToLower() == string.Format("{0}", o.id).ToLower()) }).ToArray(),
                        Licenses = (from o in licenses select new LicenseViewModel { 
                            LicenseID = o.id,
                            Expiry = o.expiry,
                            ModelID = o.modelid,
                            ModelName = o.modelname,
                            ModelRestrictions = o.modelrestrictions,
                            PartID = o.partid,
                            PartName = o.partname,
                            PartRestrictions = o.partrestrictions
                        }).AsEnumerable(),
                        Roles = roles
                    };
                }
                    
            }
        }

        public Guid? GetDefaultCompanyID(Guid? contactID)
        {
            if (!contactID.HasValue || contactID.Value == default(Guid))
                return null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null);
                return (from o in d.Experiences
                         orderby o.VersionUpdated descending
                         where
                         (o.DateFinished == null || o.DateFinished > DateTime.UtcNow)
                         && (o.Expiry == null || o.Expiry > DateTime.UtcNow)
                         && o.CompanyID != null
                         && o.ContactID == contactID
                         && (o.DateStart <= DateTime.UtcNow || o.DateStart == null)
                         && o.Version == 0 && o.VersionDeletedBy == null
                         select o.CompanyID).FirstOrDefault();
            }
        }

        public Contact GetContact(string username)
        {
            if (username == null)
                return null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null);
                return (from c in d.Contacts join u in d.Users on c.AspNetUserID equals u.UserId where u.ApplicationId == ApplicationID where username == c.Username && c.Version == 0 && c.VersionDeletedBy == null select c).Single();
                //return d.Contacts.Where(x=>x.Username == username).Select(x=>x.ContactID).FirstOrDefault();
            }
        }

        public bool IsValidInNKD(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString,null);
                if (default(Guid) == (from u in c.Users join contacts in c.Contacts on u.UserId equals contacts.AspNetUserID where u.UserName == username && contacts.Username == username && contacts.Version == 0 && contacts.VersionDeletedBy == null select contacts.ContactID).SingleOrDefault())
                    return false;
                else
                    return true;
            }
            
        }

        public bool IsValidInNKD()
        {
            return IsValidInNKD(Username);
        }

        public bool IsAuthorised(bool checkLicense,
          Authority.ActionType action,
          string dataType,
          string tableType,
          string field,
          Guid? referenceID,
          Guid? applicationID,
          Guid? licenseID, //Chcek license elsewhere too in binary form, optional implementation for 3rd party modules
          Guid? assetID,
          Guid? modelID,
          Guid? partID,
          Guid? companyID,
          Guid? contactID,
          Guid? projectID,
          Guid? roleID)
        {
            if (_orchardServices.WorkContext.CurrentUser.UserName == "admin")
                return AdminAuthority
                    .IsAuthorised(checkLicense, action, dataType, tableType, field, referenceID, applicationID, licenseID, assetID, modelID, partID, companyID, contactID, projectID, roleID);
            else
                return CachedAuthority
                    .IsAuthorised(checkLicense, action, dataType, tableType, field, referenceID, applicationID, licenseID, assetID, modelID, partID, companyID, contactID, projectID, roleID);
        }

        public List<SecurityWhitelist> AuthorisedList
        {
            get
            {
                if (_orchardServices.WorkContext.CurrentUser.UserName == "admin")
                    return AdminAuthority.AuthorisedList;
                else
                    return CachedAuthority.AuthorisedList;
            }
        }
        
        public Authority CachedAuthority
        {
            get
            {                
                var cachedAuthorityKey = string.Format("{0}_{1}_Authority", ApplicationID, ContactID); //TODO: can prob cache these values too
                try
                {
                    var auth =  _cache.Get<string, Authority>(cachedAuthorityKey, ctx =>
                    {
                        return BuildAuthority(ContactID);
                    });
                    if ((DateTime.UtcNow - auth.LastUpdated) > new TimeSpan(0, 5, 0)) //TODO: Nick suggests this is based on new white-list/black-list entry event/change in DB... good idea! Look at SQL Events for bl,wl,experiences etc.
                       throw new ExpiredAuthorityException();
                    return auth;
                }
                catch (ExpiredAuthorityException)
                {
                    _signals.Trigger(cachedAuthorityKey);  
                    Logger.Information(string.Format("User Authority Expired ({0}) - Renewing @ {1}", _orchardServices.WorkContext.CurrentUser.UserName, System.DateTime.UtcNow));
                    return _cache.Get<string, Authority>(cachedAuthorityKey, ctx =>
                    {
                        return BuildAuthority(ContactID);
                    });
                }

            }
        }

        private Authority AdminAuthority = new Authority(new Guid("370846E4-36DC-4BAC-8FB5-C788C730BB45"), "admin"); //Admin service account id, different from contact who is admin
        public Authority BuildAuthority(Guid? contactID)
        {            
            if (!IsValidInNKD() || !contactID.HasValue)
                throw new AuthorityException("No authority to connect to NKD.");

            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString,null);

                var username = (from o in c.Contacts where o.ContactID == contactID && o.Version==0 && o.VersionDeletedBy==null select o.Username).Single();
                var userID = (from o in c.Users where o.ApplicationId == ApplicationID && o.UserName == username select o.UserId).Single();
                var r = (from o in c.Users
                         from x in o.Roles where o.UserId==userID select x.RoleId);
                var myCompanies = (from o in c.Experiences where o.ContactID==contactID && o.CompanyID!=null select o.CompanyID).ToArray();
                var allCompanies = new List<Guid>();
                var rootCompanies = new List<Guid>();
                using (DataTable table = new DataTable()) 
                {
                    using (var con = new SqlConnection(ApplicationConnectionString))
                    using(var cmd = new SqlCommand("X_SP_GetCompanies", con))
                    using(var da = new SqlDataAdapter(cmd))
                    {
                       cmd.CommandType = CommandType.StoredProcedure;
                       da.Fill(table);
                    }

                    //Get the roots that the company is in and the children
                    foreach (DataRow row in table.Rows)
                    {
                        var found = false;
                        var rowRoot = (Guid)row[1];
                        for (int i=1; i < table.Columns.Count; i+=2)
                        {
                            var checking = (Guid)row[i];
                            if (found && checking==allCompanies.Last())
                                break;
                            if (!found && myCompanies.Contains(checking))
                            {
                                found = true;
                                rootCompanies.Add(rowRoot);
                            }
                            if (found)
                                allCompanies.Add(checking);
                        }
                    }
                }

                //Get my licenses & applications, assets, models, parts
                var licenses = (from o in c.Licenses where o.ContactID==contactID && o.LicenseID!=null select o);
                var assets = (from o in c.LicenseAssets where !(from x in c.Licenses where x.ContactID==contactID select x.LicenseID).Contains(o.LicenseID.Value) select o);
                var parts = (from o in c.LicenseAssetModelParts where !(from x in assets select x.LicenseAssetID).Contains(o.LicenseAssetID.Value) select o);
                var users = (from o in c.Users where o.UserName==username select o.UserId);
                var applications = (from o in c.Applications join x in c.Users on o.ApplicationId equals x.ApplicationId where x.UserName == username select o.ApplicationId);
                var experiences = (from o in c.Experiences where o.ContactID == contactID select o);
                //TODO: Do this based on experience instead of all!!! HACK!
                var bl = (from o in c.SecurityBlacklists select o);
                var wl = (from o in c.SecurityWhitelists select o);
                return new Authority(contactID.Value, username, userID, ApplicationID, bl, wl, applications, r, experiences, licenses, assets, parts, users, allCompanies, rootCompanies);
            }
            

        }

        private string getNameFromFQDN(string fqdn, Dictionary<string,string> cache)
        {
            string temp;
            if (cache != null && cache.TryGetValue(fqdn, out temp))
                return temp;
            try
            {
                temp = ((NTAccount)(GroupPrincipal.FindByIdentity(securityContext, string.Format("{0}", fqdn)).Sid).Translate(typeof(NTAccount))).ToString();
            }
            catch
            {
                temp = null;
            }
            cache.Add(fqdn, temp);
            return temp;
        }

        public void EmailContacts(Guid[] contacts, string subject, string body, bool retry = false, bool issueSupport = true)
        {

            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString, null);
                throw new NotImplementedException();
            }

        }

        public void EmailUsers(string[] emails, string subject, string body, bool retry = false, bool forwardToSupport = false, string from = null, string fromName = null, bool hideRecipients = false)
        {
            var data = new Dictionary<string,object>();
            data.Add("Subject", subject); 
            data.Add("Body", body);
            if (hideRecipients)
                data.Add("HideRecipients", true);
            if (from != null)
                data.Add("From", from);
            if (fromName != null)
                data.Add("FromName", fromName);
            try
            {
                if (emails == null)
                    emails = new string[] { };
                string[] recipients = null;
                if (forwardToSupport)
                {
                    Orchard.Email.Models.SmtpSettingsPart smtpSettings = null;
                    if (_orchardServices.WorkContext != null)
                        smtpSettings = _orchardServices.WorkContext.CurrentSite.As<Orchard.Email.Models.SmtpSettingsPart>();
                    if (smtpSettings == null || string.IsNullOrWhiteSpace(smtpSettings.Address))
                        recipients = emails;
                    else
                        recipients = emails.Union(new string[] { smtpSettings.Address }).Where(f => !string.IsNullOrEmpty(f)).ToArray();
                }
                else
                    recipients = emails;
                data.Add("Recipients", string.Join(";", recipients)); 
                _messageService.Send("Email", data);
            }
            catch
            {
                if (retry)
                {
                    EmailUsersAsync(emails, subject, body);
                }
            }
            finally
            {
                Logger.Information(string.Format("Attempted sending notification for: {0}.\r\n\r\n Regarding: \r\n\r\n {1}\n\nRetry:{2}", subject, body, retry));
            }
        }

        public void EmailUsersAsync(string[] emails, string subject, string body, bool retry = false, bool forwardToSupport = false, string from = null, string fromName = null, bool hideRecipients = false)
        {
            try
            {
                var em = _contentManager.New<EmailPart>("Email");
                em.Recipients = emails.FlattenStringArray();
                if (subject.Length > 254)
                    em.Subject = subject.Substring(0, 255);
                else
                    em.Subject = subject;
                if (body.Length > 3999)
                    em.Body = body.Substring(0, 4000);
                else 
                    em.Body = body;
                em.Retry = retry;
                em.Processed = DateTime.UtcNow;
                em.ForwardSupport = forwardToSupport;
                em.FromAddress = from;
                em.FromName = fromName;
                em.HideRecipients = hideRecipients;
                _contentManager.Create(em); //, VersionOptions.Published
                _taskManager.EmailAsync(em.ContentItem);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, string.Format("Failed Sending Notification - Couldn't assemble message.\n\nSubject:\n{0}\n\nBody:\n{1}\n\nRecipients:\n{2}\n\n", subject, body, emails == null ? "Unknown Recipients" : string.Join(";", emails)));
            }
        }


        public bool CheckPermission(ISecured secured, ActionPermission permission)
        {            
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString,null);
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(byte));
                c.X_SP_GetSecuredRight(secured.AccessorContactID, secured.AccessorApplicationID , secured.OwnerTableType, secured.OwnerReferenceID, secured.OwnerField,
                    secured.CanRead || ((ActionPermission.Read & permission) == ActionPermission.Read)
                    , secured.CanCreate || ((ActionPermission.Create & permission) == ActionPermission.Create)
                    , secured.CanUpdate || ((ActionPermission.Update & permission) == ActionPermission.Update)
                    , secured.CanDelete || ((ActionPermission.Delete & permission) == ActionPermission.Delete)
                    , verified);
                return (bool)verified.Value;
            }
        }

        public bool CheckOwnership(ISecured secured, ActionPermission permission)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString,null);
                var verified = new System.Data.Objects.ObjectParameter("verified", typeof(byte));
                c.X_SP_GetSecuredRight(secured.OwnerContactID, secured.OwnerApplicationID, secured.OwnerTableType, secured.OwnerReferenceID, secured.OwnerField,
                    secured.CanRead || ((ActionPermission.Read & permission) == ActionPermission.Read)
                    , secured.CanCreate || ((ActionPermission.Create & permission) == ActionPermission.Create)
                    , secured.CanUpdate || ((ActionPermission.Update & permission) == ActionPermission.Update)
                    , secured.CanDelete || ((ActionPermission.Delete & permission) == ActionPermission.Delete)
                    , verified);
                return (bool)verified.Value;
            }
        }

        public void UpdateSecurity(ISecured secured)
        {
            //TODO!!!: When writing security check for antecedentid = referenceid &/or version=0
            //First check user and owner rights, black list and white list against record
            //Then get ok
            if (secured.SecurityID.HasValue)
            {
                //Call Edit
                if (CheckOwnership(secured, ActionPermission.Read | ActionPermission.Update))
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var c = new NKDC(ApplicationConnectionString,null);
                        if (secured.IsBlack)
                        {
                            var s = (from o in c.SecurityBlacklists where o.SecurityBlacklistID==secured.SecurityID && o.Version==0 && o.VersionDeletedBy==null select o).Single();
                            s.AccessorContactID = secured.AccessorContactID;
                            s.AccessorApplicationID = secured.AccessorApplicationID;
                            s.AccessorCompanyID = secured.AccessorCompanyID;
                            s.AccessorProjectID = secured.AccessorProjectID;
                            s.AccessorRoleID = secured.AccessorRoleID;
                            s.CanCreate = secured.CanCreate;
                            s.CanRead = secured.CanRead;
                            s.CanDelete = secured.CanDelete;
                            s.CanUpdate = secured.CanUpdate;
                            s.VersionUpdated = DateTime.UtcNow;
                            s.VersionUpdatedBy = secured.OwnerContactID;
                        }
                        else
                        {
                            var s = (from o in c.SecurityWhitelists where o.SecurityWhitelistID == secured.SecurityID && o.Version== 0 && o.VersionDeletedBy == null select o).Single();
                            s.AccessorContactID = secured.AccessorContactID;
                            s.AccessorApplicationID = secured.AccessorApplicationID;
                            s.AccessorCompanyID = secured.AccessorCompanyID;
                            s.AccessorProjectID = secured.AccessorProjectID;
                            s.AccessorRoleID = secured.AccessorRoleID;
                            s.CanCreate = secured.CanCreate;
                            s.CanRead = secured.CanRead;
                            s.CanDelete = secured.CanDelete;
                            s.CanUpdate = secured.CanUpdate;
                            s.VersionUpdated = DateTime.UtcNow;
                            s.VersionUpdatedBy = secured.OwnerContactID;
                        }
                        c.SaveChanges();
                    }
                }
                else
                    throw new AuthorityException(string.Format("Incorrect permission for action: \"Update\" Contact: {0} Record: {1}", secured.AccessorContactID, secured.OwnerReferenceID));
                
            }
            else
            {
                //Call New
                if (CheckOwnership(secured, ActionPermission.Read | ActionPermission.Create))
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var c = new NKDC(ApplicationConnectionString,null);
                        if (secured.IsBlack)
                        {
                            var s = new SecurityBlacklist
                            {
                                SecurityBlacklistID = Guid.NewGuid(),
                                AccessorContactID = secured.AccessorContactID,
                                AccessorApplicationID = secured.AccessorApplicationID,
                                AccessorCompanyID = secured.AccessorCompanyID,
                                AccessorProjectID = secured.AccessorProjectID,
                                AccessorRoleID = secured.AccessorRoleID,
                                OwnerApplicationID = secured.OwnerApplicationID,
                                OwnerCompanyID = secured.OwnerCompanyID,
                                OwnerContactID = secured.OwnerContactID,
                                OwnerEntitySystemType = secured.OwnerEntitySystemType,
                                OwnerField = secured.OwnerField,
                                OwnerProjectID = secured.OwnerProjectID,
                                OwnerReferenceID = secured.OwnerReferenceID,
                                OwnerTableType = secured.OwnerTableType,
                                CanCreate = secured.CanCreate,
                                CanRead = secured.CanRead,
                                CanDelete = secured.CanDelete,
                                CanUpdate = secured.CanUpdate,
                                VersionOwnerContactID = secured.OwnerContactID,
                                VersionOwnerCompanyID = secured.OwnerCompanyID,
                                VersionUpdated = DateTime.UtcNow,
                                VersionUpdatedBy = secured.OwnerContactID
                            };
                            c.SecurityBlacklists.AddObject(s);
                        }
                        else
                        {
                            var s = new SecurityWhitelist
                            {
                                SecurityWhitelistID = Guid.NewGuid(),
                                AccessorContactID = secured.AccessorContactID,
                                AccessorApplicationID = secured.AccessorApplicationID,
                                AccessorCompanyID = secured.AccessorCompanyID,
                                AccessorProjectID = secured.AccessorProjectID,
                                AccessorRoleID = secured.AccessorRoleID,
                                OwnerApplicationID = secured.OwnerApplicationID,
                                OwnerCompanyID = secured.OwnerCompanyID,
                                OwnerContactID = secured.OwnerContactID,
                                OwnerEntitySystemType = secured.OwnerEntitySystemType,
                                OwnerField = secured.OwnerField,
                                OwnerProjectID = secured.OwnerProjectID,
                                OwnerReferenceID = secured.OwnerReferenceID,
                                OwnerTableType = secured.OwnerTableType,
                                CanCreate = secured.CanCreate,
                                CanRead = secured.CanRead,
                                CanDelete = secured.CanDelete,
                                CanUpdate = secured.CanUpdate,
                                VersionOwnerContactID = secured.OwnerContactID,
                                VersionOwnerCompanyID = secured.OwnerCompanyID,
                                VersionUpdated = DateTime.UtcNow,
                                VersionUpdatedBy = secured.OwnerContactID
                            };
                            c.SecurityWhitelists.AddObject(s);
                        }
                        c.SaveChanges();
                    }
                }
                else
                    throw new AuthorityException(string.Format("Incorrect permission for action: \"Create\" Contact: {0} Record: {1}", secured.AccessorContactID, secured.OwnerReferenceID));
            }

        }

        public void DeleteSecurity(ISecured secured)
        {
            //TODO!!!: When writing security check for antecedentid = referenceid &/or version=0
            if (secured.SecurityID.HasValue)
            {
                if (CheckOwnership(secured, ActionPermission.Read | ActionPermission.Delete))
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var c = new NKDC(ApplicationConnectionString,null);
                        if (secured.IsBlack)
                        {
                            var s = (from o in c.SecurityBlacklists where o.SecurityBlacklistID == secured.SecurityID && o.Version == 0 && o.VersionDeletedBy == null select o).Single();                          
                            c.SecurityBlacklists.DeleteObject(s);
                        }
                        else
                        {
                            var s = (from o in c.SecurityWhitelists where o.SecurityWhitelistID == secured.SecurityID && o.Version == 0 && o.VersionDeletedBy == null select o).Single();                           
                            c.SecurityWhitelists.DeleteObject(s);
                        }
                        c.SaveChanges();
                    }
                }
                else
                    throw new AuthorityException(string.Format("Incorrect permission for action: \"Delete\" Contact: {0} Record: {1}", secured.AccessorContactID, secured.OwnerReferenceID));
            }
            else
                throw new NotSupportedException("Can not delete a security record without an ID.");

        }

        public bool UpdateUserEmail(string email)
        {
            IUser user = _orchardServices.WorkContext.CurrentUser;
            if (!RegexHelper.IsEmail(email))
                return false;
            //_contentManager.Query<UserPart, UserPartRecord>().List();
            if (_userPartRepository.Fetch(f => f.Email == email).Any())
                return false;
            user.As<UserPart>().Email = email;
            return true;
        }

        public bool RequestLostPassword(string username) {
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if ( !registrationSettings.EnableLostPassword ) {
                return false;
            }

            if(String.IsNullOrWhiteSpace(username)){
                return false;
            }

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            if (String.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Current.Request.ToRootUrlString();
            }
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return _user.SendLostPasswordEmail(username, nonce => url.MakeAbsolute(url.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));
        }


        public bool DisableContact(Guid contactID)
        {
            string username = null;
            string email = null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString, null);
                var u = (from o in c.Contacts.Where(f => f.ContactID == contactID) select o).FirstOrDefault();
                if (u == null)
                    return false;
                u.VersionCertainty = -1;
                username = u.Username;
                email = u.DefaultEmail;
                c.SaveChanges();

            }
            var users =  _contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user =>
                                          user.NormalizedUserName == username ||
                                          user.Email == email)
                                   .List();
            foreach (var user in users) {
                user.RegistrationStatus = UserStatus.Pending;
            }
            return true;
        }

        public bool IsContactDisabled(Guid contactID)
        {
            string username = null;
            string email = null;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString, null);
                var u = (from o in c.Contacts.Where(f => f.ContactID == contactID && f.Version == 0) select new { o.Username, o.DefaultEmail, o.VersionCertainty, o.VersionDeletedBy }).FirstOrDefault();
                if (u == null)
                    return false;
                if ((u.VersionCertainty.HasValue && u.VersionCertainty < 0) || u.VersionDeletedBy != null)
                    return true;
                username = u.Username;
                email = u.DefaultEmail;

            }
            return _contentManager.Query<UserPart, UserPartRecord>()
                                  .Where(user =>
                                         (user.NormalizedUserName == username ||
                                         user.Email == email)
                                         && user.RegistrationStatus == UserStatus.Pending)
                                  .List().Any();
        }


        public bool VerifyUserUnicity(string userName, string email)
        {
            if (userName == null || email == null)
                return false;

            if (userName.Length < 2 || email.Length < 4)
                return false;

            string normalizedUserName = userName.ToLowerInvariant();

            if (_contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user =>
                                          user.NormalizedUserName == normalizedUserName ||
                                          user.Email == email)
                                   .List().Any())
            {
                return false;
            }

            return true;
        }

        public IUser Create(string email, string username, string password)
        {
            if (VerifyUserUnicity(username, email))
                return _membership.CreateUser(new CreateUserParams(username, password, email, null, null, true));
            else
                return null;
        }

        //Events

        public void Creating(UserContext context) {
            
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null, false);
                var exists = (from o in d.Contacts where (o.Username==context.UserParameters.Username && o.DefaultEmail != context.UserParameters.Email) select o).Any();
                if (exists)
                {
                    context.Cancel = true;
                }
            }
        }

        public void Created(UserContext context) {
            _contentManagerSession.Store(context.User.ContentItem);
            SyncUsers();
            var contact = GetContactID(context.User.UserName);
            if (contact == null)
                return;
            //Add user to default company
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(ApplicationConnectionString, null, false);
                if (!(from o in d.Experiences where o.ContactID == contact && o.CompanyID != null select o).Any())
                {
                    var e = new Experience
                    {
                        ExperienceID = Guid.NewGuid(),
                        ContactID = contact,
                        CompanyID = COMPANY_DEFAULT,
                        ExperienceName = string.Format("User - {0}", context.User.UserName),
                        VersionUpdated = DateTime.UtcNow
                    };
                    d.Experiences.AddObject(e);
                    d.SaveChanges();
                }
            } 
        }

        //TODO:Check
        public void LoggedIn(IUser user) { }

        public void LoggedOut(IUser user) { }

        public void AccessDenied(IUser user) { }

        public void ChangedPassword(IUser user) { }

        public void SentChallengeEmail(IUser user) { }

        public void ConfirmedEmail(IUser user) { }

        public void Approved(IUser user) { }

        public void WarnAdmins(IEnumerable<string> warnings)
        {
            try
            {
                Logger.Warning(string.Join("\r\n\r\n", warnings));
            }
            catch { }
            var adminRoles = new string[] {"Administrator"};
            var m = _contentManager.Query<UserPart, UserPartRecord>().Where(f => f.UserName == "admin").List();
            var application = ApplicationID;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var c = new NKDC(ApplicationConnectionString, null);
                var e = (from o in c.ApplicationUserRoles where o.ApplicationId==application && o.RoleName == "Administrator" select o.DefaultEmail).ToArray()
                    .Union(m.Select(f=>f.Email))
                    .Where(f=>!string.IsNullOrWhiteSpace(f)).ToArray();
                EmailUsers(e,
                    string.Format("WARNING! SITE: {0} USER: {1} EMAIL: {2}"
                        , _orchardServices.WorkContext.CurrentSite.SiteName
                        , _orchardServices.WorkContext.CurrentUser.UserName
                        , _orchardServices.WorkContext.CurrentUser.Email)
                    , string.Format("Warnings [UTC:{0}]: \r\n\r\n {1}", DateTime.UtcNow, string.Join("\r\n\r\n", warnings))
                    );
            }
                    
        }

    }
}
