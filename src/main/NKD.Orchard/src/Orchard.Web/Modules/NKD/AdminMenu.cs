using System.Linq;
using NKD.Services;
using System.Web.Routing;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace NKD {
    public class AdminMenu : INavigationProvider {
        private readonly Work<RequestContext> _requestContextAccessor;
        
        public AdminMenu(Work<RequestContext> requestContextAccessor) {
            _requestContextAccessor = requestContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        
        public void GetNavigation(NavigationBuilder builder)
        {
            //Do this eventually
            //var requestContext = _requestContextAccessor.Value;
            //var idValue = (string)requestContext.RouteData.Values["id"];
            //var id = 0;

            //if (!string.IsNullOrEmpty(idValue))
            //{
            //    int.TryParse(idValue, out id);
            //}

            //builder

            //    // Image set
            //    .AddImageSet("nkd-projects")

            //    // "Root"
            //    .Add(item => item

            //        .Caption(T("NKD Projects"))
            //        .Position("2")
            //        .LinkToFirstChild(true)

            //        // "Top"
            //        .Add(subItem => subItem
            //            .Caption(T("Admin Projects"))
            //            .Position("2.1")
            //            .Action("Index", "Admin", new { area = "NKD" })

            //            .Add(T("Details"), i => i.Action("Edit", "Admin", new { id }).LocalNav())
            //            .Add(T("Models"), i => i.Action("Add", "Admin", new { id }).LocalNav())
            //        )

            //        // "Next"
            //        .Add(subItem => subItem
            //            .Caption(T("User Projects"))
            //            .Position("2.2")
            //            .Action("Index", "User", new { area = "NKD" })
            //        )
            //    );
        }
    }
}