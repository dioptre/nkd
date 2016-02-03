using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using Orchard.UI.Admin;

namespace NKD.Controllers {
    [Admin]
    public class AdminController : Controller {
        public IOrchardServices Services { get; set; }

        public AdminController(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}
