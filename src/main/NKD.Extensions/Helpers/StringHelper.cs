using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace NKD.Helpers
{
    public static class StringHelper
    {

        public static string[] SplitStringArray(this string original)
        {
            return original.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static Guid[] SplitStringGuids(this string original)
        {
            return (from o in original.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries) select new System.Guid(o)).ToArray();
        }

        public static string FlattenStringArray(this string[] original)
        {
            return string.Join(";", original);
        }

        public static string FlattenGuidArray(this Guid[] original)
        {
            return string.Join(";", original);
        }

        public static string Capitalize(this string original, bool onlyFirst=false, string culture="en-US")
        {
            TextInfo textInfo = new CultureInfo(culture, false).TextInfo;
            original = original.Trim();
            if (string.IsNullOrWhiteSpace(original))
                return null;
            if (onlyFirst && original.Length != 1)
            {
                return textInfo.ToTitleCase(original.Substring(0, 1)) + original.Substring(1);
            }
            else
            {
                return textInfo.ToTitleCase(original);
            }
        }

    }
}