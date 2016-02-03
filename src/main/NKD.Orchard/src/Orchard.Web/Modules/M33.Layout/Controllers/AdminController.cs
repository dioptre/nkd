using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using M33.Layout.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.UI.Notify;
using System.Collections.Generic;
using Orchard.Themes.Services;
using Orchard;

/*
 * Author Rickard Magnusson, M33 2011
 */

namespace M33.Layout.Controllers {
    [ValidateInput(false)]
    public  class AdminController : Controller, IUpdateModel {
       
        private readonly ISiteThemeService _siteThemeService;
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        private string Current = string.Empty;
        private FileType TypeOfFile; 

        public AdminController(IOrchardServices services, ISiteThemeService siteThemeService) {
            Services = services;
            _siteThemeService = siteThemeService;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index()
        {
            if (!Services.Authorizer.Authorize(Permissions.EditFiles, T("Not authorized to manage files")))
                return new HttpUnauthorizedResult();

            TypeOfFile = FileType.CSS;

            try
            {
                var files = EnumerateFiles("");
                var filename = files.First();
                var file = new StreamReader(filename.Path);
                var data = file.ReadToEnd();
                file.Close();

                var viewModel = new FileViewModel { Content = data, FileName = filename.Name, Files = files};

                return View(viewModel);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ViewEditor()
        {
            if (!Services.Authorizer.Authorize(Permissions.EditFiles, T("Not authorized to manage css")))
                return new HttpUnauthorizedResult();

            TypeOfFile = FileType.RAZOR;

            try
            {
                var files = EnumerateFiles("");
                var filename = files.First();
                var file = new StreamReader(filename.Path);
                var data = file.ReadToEnd();
                file.Close();

                var viewModel = new FileViewModel { Content = data, FileName = filename.Name, Files = files };

                return View(viewModel);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult JsEditor()
        {
            if (!Services.Authorizer.Authorize(Permissions.EditFiles, T("Not authorized to manage script files")))
                return new HttpUnauthorizedResult();

            TypeOfFile = FileType.JS;

            try
            {
                var files = EnumerateFiles("");
                var filename = files.First();
                var file = new StreamReader(filename.Path);
                var data = file.ReadToEnd();
                file.Close();

                var viewModel = new FileViewModel { Content = data, FileName = filename.Name, Files = files };

                return View(viewModel);
            }
            catch
            {
                var f = new FileViewModel { FileType = ViewModels.FileType.JS, Content = string.Empty, FileName = string.Empty, Files = new List<FileModel> { new FileModel { Current = false, Name = "No js", Path = string.Empty } } };
                return View(f);
            }
        }

        public JsonResult Load(string fileName)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditFiles, T("Not authorized to manage files")))
                return null;

      
                var toload = new StreamReader(fileName);
                var data = toload.ReadToEnd();
                toload.Close();

                FileViewModel viewModel = new FileViewModel { Content = data, FileName = new FileInfo(fileName).Name, Files = EnumerateFiles(fileName) };
                return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Save(string fileName, string content)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditFiles, T("Not authorized to manage files")))
                return null;

            using (var f = new System.IO.StreamWriter(fileName))
                f.Write(content);

            return Load(fileName);
        }

        public ActionResult Add()
        {
            var viewModel = new AddFile { FileName = string.Empty, TypeOfFile = "" };
            return View(viewModel);
        }

        public ActionResult Settings()
        {
            var viewModel = new Settings { Description=string.Empty, Placement=string.Empty };
            string path = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/Themes/{0}/", _siteThemeService.GetCurrentThemeName()));

            var theme = System.IO.File.OpenText(Path.Combine(path, "Theme.txt"));
                viewModel.Description = theme.ReadToEnd();
                theme.Close();

            var place = System.IO.File.OpenText(Path.Combine(path, "Placement.info"));
                viewModel.Placement = place.ReadToEnd();
                place.Close();

            return View(viewModel);
        }

        [HttpPost, ActionName("Settings")]
        public ActionResult SettingsPOST()
        {
            var viewModel = new Settings { Description = string.Empty, Placement = string.Empty };
            string path = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/Themes/{0}/", _siteThemeService.GetCurrentThemeName()));
           
                UpdateModel(viewModel);

                using (var f = new System.IO.StreamWriter(Path.Combine(path, "Theme.txt"))){
                    f.Write(viewModel.Description);
                    Services.Notifier.Information(T("Updated Theme.txt successfully."));
                }

                using (var f = new System.IO.StreamWriter(Path.Combine(path, "Placement.info"))){
                    f.Write(viewModel.Placement);
                    Services.Notifier.Information(T("Updated Placement.info successfully."));
                }
           
            try
            {

                if (!String.IsNullOrWhiteSpace(Request.Files[0].FileName)){
                    Request.Files["ThemeImage"].SaveAs(Path.Combine(path, "Theme.png"));
                    Services.Notifier.Information(T("Updated/added Theme.png successfully."));
                }
                if (!String.IsNullOrWhiteSpace(Request.Files[1].FileName)){
                    Request.Files["ThemeZoneImage"].SaveAs(Path.Combine(path, "ThemeZonePreview.png"));
                    Services.Notifier.Information(T("Updated/added ThemeZonePreview.png successfully."));
                }

                if (!String.IsNullOrWhiteSpace(Request.Files[2].FileName))
                {
                    Request.Files["ThemeExtended"].SaveAs(Path.Combine(path, Request.Files["ThemeExtended"].FileName));
                    Services.Notifier.Information(T("Added image successfully."));
                }

            }catch (Exception exception){
                this.AddModelError("Upload failed", T("Uploading media file failed:" + exception.ToString()));

                return View(viewModel);
            }
            return Settings();
        }

        [FormValueRequired("submit")]
        [HttpPost, ActionName("Add")]
        public ActionResult AddPost()
        {
           var viewModel = new AddFile{ FileName=string.Empty, TypeOfFile = "" };
           string folder = string.Empty;
           string extension = string.Empty;
           string view = string.Empty;

            if (TryUpdateModel(viewModel))
            {
                switch (viewModel.TypeOfFile)
                {
                    case "css":
                        extension = ".css";
                        folder = "Styles";
                        view = "Index";
                        break;
                    case "js":
                        extension = ".js";
                        folder = "Scripts";
                        view = "JsEditor";
                        break;
                    case "razor":
                        extension = ".cshtml";
                        folder = "Views";
                        view = "ViewEditor";
                        break;
                }

                var filename = string.Format("{0}{1}", viewModel.FileName, extension);
                string path = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/Themes/{0}/{1}/{2}", _siteThemeService.GetCurrentThemeName(), folder, filename));

                if (System.IO.File.Exists(path))
                    AddModelError("Filename error", T("This file already exist.Choose another filename!", viewModel.FileName));

                using (var f = System.IO.File.CreateText(path)) { }
            }
            return RedirectToAction(view);
        }

        private IEnumerable<FileModel> EnumerateFiles(string filepath)
        {
            string extension = "*.css";
            string folder = "Styles";

            switch (TypeOfFile)
            {
                case FileType.CSS:
                    extension = "*.css";
                    folder = "Styles";
                    break;
                case FileType.JS:
                    extension = "*.js";
                    folder = "Scripts";
                    break;
                case FileType.RAZOR:
                    extension = "*.cshtml";
                    folder = "Views";
                    break;
            }

            string path = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/Themes/{0}/{1}/", _siteThemeService.GetCurrentThemeName(),folder));
            var files = Directory.EnumerateFiles(path, extension);
            var docs = new List<FileModel>();

            foreach (string f in files)
                docs.Add(new FileModel { Name = new FileInfo(f).Name, Path = f, Current = (f == filepath) });

            if (string.IsNullOrEmpty(filepath))
                docs.First().Current = true;
    
            return docs;
        }




        private FileType GetFileType(string path)
        {
            if (path.EndsWith(".css"))
                return FileType.CSS;
            else
                return FileType.RAZOR;
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}