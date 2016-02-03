using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace NKD.Helpers
{
    public static class RegexHelper
    {
        public static bool IsMobile(this string mobile)
        {
            Regex r = new Regex(@"^\+[0-9]*$", RegexOptions.Compiled);
            return r.IsMatch(mobile);
        }

        public static bool IsEmail(this string email)
        {
            Regex r = new Regex(@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                                 + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                                 + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                                 + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,40})$", RegexOptions.Compiled);
            return r.IsMatch(email);
        }

        public static bool IsHref(this string href)
        {
            Regex r = new Regex("\\s*(?i)href\\s*=\\s*(\"([^\"]*\")|'[^']*'|([^'\">\\s]+))", RegexOptions.Compiled);
            return r.IsMatch(href);
        }

        public static bool IsAnchor(this string anchor)
        {
            Regex r = new Regex("(?i)<a([^>]+)>(.+?)</a>", RegexOptions.Compiled);
            return r.IsMatch(anchor);
        }

        public static string[] Hrefs(this string hrefs)
        {
             Regex r = new Regex("<a\\s+(?:[^>]*?\\s+)?href=\"([^\"]*)", RegexOptions.Compiled);
             List<string> matches = new List<string>();
             foreach (Match m in r.Matches(hrefs))
                 matches.Add(m.Groups[1].Value);
             return matches.ToArray();
        }

        public static bool IsGuid(this string guid)
        {
            Regex r = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            return r.IsMatch(guid);
        }

    }
}