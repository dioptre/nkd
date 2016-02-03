using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NKD.Helpers
{
    public static class DateHelper
    {
        public const string DefaultDateFormat = "[yyyyMMdd-hhmmss]";
        public static string OnlineDateFormat { get { return DefaultDateFormat; } }
        public static string NowInOnlineFormat { get { return DateTime.UtcNow.ToString(OnlineDateFormat); } }
        /// <summary>
        /// Gets Unix Timestamp
        /// </summary>
        /// <param name="dateTime">UTC!</param>
        /// <returns></returns>
        public static double DateTimeToUnixTimestamp(DateTime dateTime) { return (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; }
        public static DateTime UnixTimestampToDate(this int seconds) { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds); }
        public static DateTime UnixTimestampToDate(this double seconds) { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds); }
        public static double NowToUnixTimestamp() { return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; }
        public static int Timestamp
        {
            get
            {
                return (int)NowToUnixTimestamp();
            }
        }

        public static string HexUnixTimestamp(this DateTime dateTime) {
                return ((int)DateTimeToUnixTimestamp(dateTime)).ToString("X");
        }

        public static string HexUnixTimestamp(DateTime? dateTime = null)
        {
            if (!dateTime.HasValue)
                return ((int)NowToUnixTimestamp()).ToString("X");
            else
                return ((int)DateTimeToUnixTimestamp(dateTime.Value)).ToString("X");
        }

        public static double HexToUnixTimestamp(this string hexTime)
        {
            return double.Parse(hexTime, System.Globalization.NumberStyles.HexNumber);
        }
    }
}