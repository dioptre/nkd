using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.Web.ASPxUploadControl;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;

namespace NKD.Helpers
{
    public class UIHelper
    {
        public const string TempUploadDirectory = "~/Modules/NKD/Media";
        public const string ThumbnailFormat = "Thumbnail{0}{1}";

        public static readonly DevExpress.Web.ASPxUploadControl.ValidationSettings AppendModelValidationSettings = new DevExpress.Web.ASPxUploadControl.ValidationSettings
        {
            AllowedFileExtensions = new string[] { ".csv" },
            MaxFileSize = 209715200
        };

        public static readonly DevExpress.Web.ASPxUploadControl.ValidationSettings AppendAnythingValidationSettings = new DevExpress.Web.ASPxUploadControl.ValidationSettings
        {           
            MaxFileSize = 209715200
        };

        public static void ucCallbacks_AppendComplete(object sender, FileUploadCompleteEventArgs e)
        {
            if (e.UploadedFile.IsValid)
            {
                //TODO:Fix
                //string resultFilePath = TempUploadDirectory + string.Format(ThumbnailFormat, "", Path.GetExtension(e.UploadedFile.FileName));
                IUrlResolutionService urlResolver = sender as IUrlResolutionService;
                if (urlResolver != null)
                    e.CallbackData = "false";//urlResolver.ResolveClientUrl(resultFilePath) + "?refresh=" + Guid.NewGuid().ToString();
                //TODO:
                //Get working with nick's tool
            }
        }
    }
}