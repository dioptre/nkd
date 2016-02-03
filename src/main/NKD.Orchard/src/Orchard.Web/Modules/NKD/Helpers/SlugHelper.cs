using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace NKD.Helpers
{
    public static class SlugHelper
    {
        public static string ToSlug(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = HttpUtility.UrlDecode(value);

            //First to lower case 
            value = value.ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("US-ASCII").GetBytes(value);

            value = Encoding.ASCII.GetString(bytes);

            //Replace spaces 
            value = Regex.Replace(value, @"\s", "_", RegexOptions.Compiled);

            //Remove invalid chars 
            value = Regex.Replace(value, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);

            //Trim dashes from end 
            value = value.Trim('-', '_');

            //Replace double occurences of - or \_ 
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }

        public static string FromSlug(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = HttpUtility.UrlDecode(value);

            //First to lower case 
            value = value.ToLowerInvariant();

            //Replace spaces 
            value = Regex.Replace(value, @"_", " ", RegexOptions.Compiled);

            value = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);

            return value;
        }

    }
}