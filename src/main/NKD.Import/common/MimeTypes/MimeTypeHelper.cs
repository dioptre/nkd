using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NKD.Import.MimeTypes
{
    //modified from http://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension
    public static class MimeTypeHelper
    {
        private static readonly Dictionary<string, string> MimeTypesDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"ai", "application/postscript"},
            {"bin", "application/octet-stream"},
            {"bmp", "image/bmp"},
            {"cgm", "image/cgm"},
            {"css", "text/css"},
            {"djv", "image/vnd.djvu"},
            {"djvu", "image/vnd.djvu"},
            {"doc", "application/msword"},
            {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
            {"docm","application/vnd.ms-word.document.macroEnabled.12"},
            {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
            {"eps", "application/postscript"},
            {"exe", "application/octet-stream"},
            {"gif", "image/gif"},
            {"gtar", "application/x-gtar"},
            {"hqx", "application/mac-binhex40"},
            {"htm", "text/html"},
            {"html", "text/html"},
            {"ico", "image/x-icon"},
            {"ief", "image/ief"},
            {"jp2", "image/jp2"},
            {"jpe", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"jpg", "image/jpeg"},
            {"las", "text/plain"},
            {"latex", "application/x-latex"},
            {"lha", "application/octet-stream"},
            {"lzh", "application/octet-stream"},
            {"mac", "image/x-macpaint"},
            {"pbm", "image/x-portable-bitmap"},
            {"pct", "image/pict"},
            {"pdf", "application/pdf"},
            {"pgm", "image/x-portable-graymap"},
            {"pic", "image/pict"},
            {"pict", "image/pict"},
            {"png", "image/png"}, 
            {"pnt", "image/x-macpaint"},
            {"pntg", "image/x-macpaint"},
            {"ppm", "image/x-portable-pixmap"},
            {"ppt", "application/vnd.ms-powerpoint"},
            {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
            {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
            {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
            {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
            {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
            {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
            {"ps", "application/postscript"},
            {"qti", "image/x-quicktime"},
            {"qtif", "image/x-quicktime"},
            {"ras", "image/x-cmu-raster"},
            {"rgb", "image/x-rgb"},
            {"rtf", "text/rtf"},
            {"rtx", "text/richtext"},
            {"sgm", "text/sgml"},
            {"sgml", "text/sgml"},
            {"sit", "application/x-stuffit"},
            {"svg", "image/svg+xml"},
            {"swf", "application/x-shockwave-flash"},
            {"tar", "application/x-tar"},
            {"tex", "application/x-tex"},
            {"texi", "application/x-texinfo"},
            {"texinfo", "application/x-texinfo"},
            {"tif", "image/tiff"},
            {"tiff", "image/tiff"},
            {"tsv", "text/tab-separated-values"},
            {"txt", "text/plain"},
            {"wbmp", "image/vnd.wap.wbmp"},
            {"wbmxl", "application/vnd.wap.wbxml"},
            {"wml", "text/vnd.wap.wml"},
            {"wmlc", "application/vnd.wap.wmlc"},
            {"wmls", "text/vnd.wap.wmlscript"},
            {"wmlsc", "application/vnd.wap.wmlscriptc"},
            {"xbm", "image/x-xbitmap"},
            {"xht", "application/xhtml+xml"},
            {"xhtml", "application/xhtml+xml"},
            {"xls", "application/vnd.ms-excel"},                        
            {"xml", "application/xml"},
            {"xpm", "image/x-xpixmap"},
            {"xsl", "application/xml"},
            {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
            {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
            {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
            {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
            {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
            {"xslt", "application/xslt+xml"},
            {"xwd", "image/x-xwindowdump"},
            {"zip", "application/zip"}
        };

        public static string GetMimeTypeByFileName(string fileName)
        {
            //get file extension
            string extension = Path.GetExtension(fileName);
            string mime = MimeType(extension);
            return mime;
        }

        public static string GetMimeTypeByExtension(string extension)
        {
            string mime = MimeType(extension);
            return mime;
        }

        private static string MimeType(string ext)
        {
            if (ext.Length > 0 &&
                MimeTypesDictionary.ContainsKey(ext.Remove(0, 1)))
            {
                //try dictionary lookup first
                return MimeTypesDictionary[ext.Remove(0, 1)];
            }
            if (ext.Length > 0 && !MimeTypesDictionary.ContainsKey(ext.Remove(0, 1)))
            {
                //try registry if dictionary is empty
                return MimeTypeRegistry(ext);
            }
            //all other methods have failed, return unknown
            return "application/unknown";
        }

        private static string MimeTypeRegistry(string ext)
        {
            string mimeType = "";
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }
}
