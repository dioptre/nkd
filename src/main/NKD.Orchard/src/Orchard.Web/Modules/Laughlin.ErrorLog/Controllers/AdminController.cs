using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Laughlin.ErrorLog.Models;
using Laughlin.ErrorLog.ViewModels;

namespace Laughlin.ErrorLog.Controllers
{
    public class AdminController : Controller {
        
        private string _basePath = string.Empty;
        private string[] _filenames = null;
        private string[] filenames
        {
            get
            {
                if (_filenames == null)
                    _filenames = (from p in Directory.GetFiles(_basePath, "*.log")
                            where p.IndexOf("orchard-") >= 0
                            select p.Substring(p.LastIndexOf('\\') + 1))
                          .OrderByDescending(x => x).DefaultIfEmpty()
                          .ToArray();
                return _filenames;
            }
        }

        public AdminController() {

        }

        public ActionResult Index(string SelectedLogFileName)
        {
            _basePath = Server.MapPath(@"~/App_Data/Logs");
            var model = new IndexViewModel();
            
            var logFileName = string.IsNullOrEmpty(SelectedLogFileName)
                                 //? string.Format("orchard-error-{0}.{1}.{2}.log",
                                 //                DateTime.Now.Year,
                                 //                DateTime.Now.Month.ToString().PadLeft(2, '0'),
                                 //                DateTime.Now.Day.ToString().PadLeft(2, '0'))
                                 ? filenames.FirstOrDefault()
                                 : SelectedLogFileName;

            var fullLogFilePath = _basePath + "/" + logFileName;
            model.LogDate = logFileName;

            return View(GetModel(fullLogFilePath, model));
        }

        private IndexViewModel GetModel(string fileName, IndexViewModel model)
        {

            var dates = filenames.Select(logFile => new SelectListItem
            {
                Text = logFile,
                Value = logFile
            }).ToList();

            model.Dates = dates;
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        model.LogText = reader.ReadToEnd();
                        var regex = new Regex(@"([0-9]{4}-[0-9]{2}-[0-9]{2}\s[0-9]{2}:[0-9]{2}:[0-9]{2})");

                        var matches = regex.Split(model.LogText).Where(s => s != String.Empty).ToArray();

                        for (var i = 0; i < matches.Count(); i++)
                        {
                            model.LogItems.Insert(0, new LogItem
                            {
                                Date = matches[i],
                                Text = matches[i + 1]
                            });

                            i++;
                        }
                    }
                }
            }
            catch { }

            return model;
        }
    }
}