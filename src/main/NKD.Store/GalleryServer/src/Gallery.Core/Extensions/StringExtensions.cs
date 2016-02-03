using System;

namespace Gallery.Core.Extensions
{
    public static class StringExtensions
    {
        public static string NullifyEmpty(this string stringToCheck)
        {
            return string.IsNullOrWhiteSpace(stringToCheck) ? null : stringToCheck;
        }

        public static string AppendWithForwardSlash(this string front, string back)
        {
            if (front == null)
            {
                throw new ArgumentNullException("front");
            }
            if (back == null)
            {
                throw new ArgumentNullException("back");
            }
            return front.EndsWith("/") || back.StartsWith("/")
                ? front + back
                : string.Join("/", front, back);
        }
    }
}