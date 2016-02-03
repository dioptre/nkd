using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace NKD.Helpers
{
    public static class NetworkHelper
    {

        public static List<IPAddressRange> PublicRangeIP4 = new List<IPAddressRange>();
        public static List<IPAddressRange> PublicRangeIP6 = new List<IPAddressRange>();
        static NetworkHelper()
        {
            PublicRangeIP4.Add(new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.255.255.255")));
            PublicRangeIP4.Add(new IPAddressRange(IPAddress.Parse("169.254.0.0"), IPAddress.Parse("169.254.255.255")));
            PublicRangeIP4.Add(new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255")));
            PublicRangeIP4.Add(new IPAddressRange(IPAddress.Parse("172.16.0.0"), IPAddress.Parse("172.31.255.255")));
            PublicRangeIP4.Add(new IPAddressRange(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1")));
            
            PublicRangeIP6.Add(new IPAddressRange(IPAddress.Parse("::1"), IPAddress.Parse("::1")));

        }

        public static string GetIPAddress(this HttpRequestBase request)
        {
            string szRemoteAddr = request.UserHostAddress;
            string szXForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];
            string szIP = "";

            if (szXForwardedFor == null)
            {
                szIP = szRemoteAddr;
            }
            else
            {
                szIP = szXForwardedFor;
                if (szIP.IndexOf(",") > 0)
                {
                    string[] arIPs = szIP.Split(',');

                    foreach (string item in arIPs)
                    {
                        if (!IsLocal(item))
                        {
                            return item;
                        }
                    }
                }
            }
            return szIP;
        }

        public static bool IsLocal(IPAddress ipaddress)
        {
            if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {               
               return (from o in PublicRangeIP4 where o.IsInRange(ipaddress) == true select o).Any();
            }
            if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return (ipaddress.IsIPv6LinkLocal 
                    || ipaddress.IsIPv6Multicast 
                    || ipaddress.IsIPv6SiteLocal 
                    || ipaddress.IsIPv6Teredo
                    || (from o in PublicRangeIP6 where o.IsInRange(ipaddress) == true select o).Any());
            }
            return false;
        }

        public static bool IsLocal(string ipaddress)
        {
            return IsLocal(IPAddress.Parse(ipaddress));
        }
        public static long IPAsLong(this IPAddress ip)
        {
            if (ip == null)
                return 0;

            //lsb
            var ipb = ip.GetAddressBytes();
            long value = 0;
            for (int i = 0; i < ipb.Length; i++)
            {
               value += ((long) ipb[i] & 0xffL) << (8 * i);
            }
            return value;
            ////msb
            //for (int i = 0; i < ipb.Length; i++)
            //{
            //   value = (value << 8) + (ipb[i] & 0xff);
            //}

        }

        public static uint IPAsInt(this IPAddress ip)
        {
            if (ip == null)
                return 0;

            //lsb
            var ipb = ip.GetAddressBytes();
            uint value = 0;
            for (int i = 0; i < ipb.Length; i++)
            {
                value += ((uint)ipb[i] & 0xff) << (8 * i);
            }
            return value;
            ////msb
            //for (int i = 0; i < ipb.Length; i++)
            //{
            //   value = (value << 8) + (ipb[i] & 0xff);
            //}

        }

        public class IPAddressRange
        {
            readonly System.Net.Sockets.AddressFamily addressFamily;
            readonly byte[] lowerBytes;
            readonly byte[] upperBytes;

            public IPAddressRange(IPAddress lower, IPAddress upper)
            {
                // Assert that lower.AddressFamily == upper.AddressFamily

                this.addressFamily = lower.AddressFamily;
                this.lowerBytes = lower.GetAddressBytes();
                this.upperBytes = upper.GetAddressBytes();
            }

            public bool IsInRange(IPAddress address)
            {
                if (address.AddressFamily != addressFamily)
                {
                    return false;
                }

                byte[] addressBytes = address.GetAddressBytes();

                bool lowerBoundary = true, upperBoundary = true;

                for (int i = 0; i < this.lowerBytes.Length &&
                    (lowerBoundary || upperBoundary); i++)
                {
                    if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                        (upperBoundary && addressBytes[i] > upperBytes[i]))
                    {
                        return false;
                    }

                    lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                    upperBoundary &= (addressBytes[i] == upperBytes[i]);
                }

                return true;
            }
        }
    }
}