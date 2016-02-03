using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard;
using Orchard.UI.Admin;
using Contrib.Mod.ChangePassword.ViewModels;
using System;
using Orchard.Environment.Extensions;

namespace Contrib.Mod.ChangePassword.Controllers
{
    [Admin]
    public class ChangePasswordController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;

        private readonly INotifier notifier;

        public ChangePasswordController(
            IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            ISiteService siteService, 
            INotifier notifier)
        {
            this.notifier = notifier;
            Services = services;
            _membershipService = membershipService;
            _userService = userService;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        
        public ActionResult Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            //if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list users")))
            if (!Services.Authorizer.Authorize(Permissions.ChangePassword, T("Not authorized to list users")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new UserIndexOptions();

            var users = Services.ContentManager.Query<UserPart, UserPartRecord>();

            switch (options.Filter)
            {
                case UsersFilter.Approved:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
                case UsersFilter.Pending:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Pending);
                    break;
                case UsersFilter.EmailPending:
                    users = users.Where(u => u.EmailStatus == UserStatus.Pending);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                users = users.Where(u => u.UserName.Contains(options.Search) || u.Email.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(users.Count());

            switch (options.Order)
            {
                case UsersOrder.Name:
                    users = users.OrderBy(u => u.UserName);
                    break;
                case UsersOrder.Email:
                    users = users.OrderBy(u => u.Email);
                    break;
            }

            var results = users
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new UsersIndexViewModel
            {
                Users = results
                    .Select(x => new UserEntry { User = x.Record })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        public ActionResult EditUserPassword(int id) 
        {
            if (!Services.Authorizer.Authorize(Permissions.ChangePassword, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id);
            var viewModel = new ChangePasswordViewModel() { 
                UserPart = user,
                UserId = user.Id
            };

            return View(viewModel);    
        }

        [HttpPost, ActionName("EditUserPassword")]
        public ActionResult EditUserPasswordPost(ChangePasswordViewModel model)
        {
            if (!Services.Authorizer.Authorize(Permissions.ChangePassword, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(model.UserId);

            if (!this.ModelState.IsValid) 
            {
                var viewModel = new ChangePasswordViewModel()
                {
                    UserPart = user,
                    UserId = user.Id
                };

                return View(viewModel);
            }

            _membershipService.SetPassword(user, model.Password);

            notifier.Information(T("User password  for " + user.UserName + " changed!"));

            return RedirectToAction("Index");
        }
    }
}