using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace M33.Layout {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("themes")
                  .Add(T("Themes"), "10",
                   menu => {
                       menu.Add(T("Css Editor"), "4", item => item.Action("Index", "Admin", new { area = "M33.Layout" }).Permission(StandardPermissions.SiteOwner).LocalNav());
                       menu.Add(T("View Editor"), "5", item => item.Action("ViewEditor", "Admin", new { area = "M33.Layout" }).Permission(StandardPermissions.SiteOwner).LocalNav());
                       menu.Add(T("JS Editor"), "6", item => item.Action("JsEditor", "Admin", new { area = "M33.Layout" }).Permission(StandardPermissions.SiteOwner).LocalNav());
                       menu.Add(T("Settings"), "8", item => item.Action("Settings", "Admin", new { area = "M33.Layout" }).Permission(StandardPermissions.SiteOwner).LocalNav());
                    }
                );
        }
    }
}
  