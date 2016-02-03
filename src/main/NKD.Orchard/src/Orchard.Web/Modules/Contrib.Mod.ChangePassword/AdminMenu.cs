using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;
using Orchard.Security;

namespace Contrib.Mod.ChangePassword
{
    public class AdminMenu : INavigationProvider
    { 

        public AdminMenu()
        {
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            //add in admin ability to change password. 
            builder.Add(T("Users"), menu => menu.Add(T("Passwords"), "3",
                item => item.Action("Index", "ChangePassword", new { area = "Contrib.Mod.ChangePassword" }).LocalNav().Permission(StandardPermissions.SiteOwner)));
        }
    }

    
}