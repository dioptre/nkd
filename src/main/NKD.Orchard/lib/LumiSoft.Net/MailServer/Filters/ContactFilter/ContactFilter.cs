using System;
using System.IO;
using System.Data;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Configuration;
using System.Transactions;
using System.Linq;
using LumiSoft.MailServer;
using LumiSoft.MailServer.Filters;
using LumiSoft.Net;
using LumiSoft.Net.DNS;
using LumiSoft.Net.DNS.Client;
using LumiSoft.Net.SMTP.Server;

using NKD.Module.BusinessObjects;

namespace EXPEDIT.MailServer.Filters
{

	public class ContactFilter : ISmtpSenderFilter
	{
        public static int? dbVersion = NKDC.DefaultVersionAsync;
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ContactFilter()
		{
        }

        private string applicationConnectionString = null;
        public string ApplicationConnectionString //TODO: This needs to be multi-tenant?
        {
            get
            {
                if (applicationConnectionString == null)
                {
                    var temp = System.Configuration.ConfigurationManager.ConnectionStrings["NKD"];
                    if (temp != null)
                        applicationConnectionString = temp.ConnectionString;
                    else
                    {
                        var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                        if (config != null && config.ConnectionStrings != null && config.ConnectionStrings.ConnectionStrings["NKD"] != null)
                            applicationConnectionString = config.ConnectionStrings.ConnectionStrings["NKD"].ConnectionString;
                        else
                            throw new NoNullAllowedException("Connection String Required");
                    }
                }
                return applicationConnectionString;
                        
            }
        }

        #region method Filter

        /// <summary>
		/// Filters sender.
		/// </summary>
		/// <param name="from">Sender.</param>
		/// <param name="api">Reference to server API.</param>
		/// <param name="session">Reference to SMTP session.</param>
		/// <param name="errorText">Filtering error text what is returned to client. ASCII text, 100 chars maximum.</param>
		/// <returns>Returns true if sender is ok or false if rejected.</returns>
        public bool Filter(string from, IMailServerApi api, SMTP_Session session, out string errorText)
        {
            errorText = null;
            bool ok = false;

            // Don't check authenticated users or LAN IP
            if (session.IsAuthenticated || IsPrivateIP(session.RemoteEndPoint.Address))
            {
                return true;
            }

            try
            {                
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(ApplicationConnectionString, null);
                    if (!(from o in d.ContactEmailsViews where o.Email == @from select o).Any())
                    {
                        errorText = "You must be a registered user to use the email support service.";
                        WriteFilterLog("Sender:" + from + " IP:" + session.RemoteEndPoint.Address.ToString() + " unregistered.\r\n");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }


            }
            catch (Exception ex)
            {
                WriteFilterLog(string.Format("Sender:{0} IP:{1} caused exception.\r\nEX:{2}{3}\r\n__\r\n", from, session.RemoteEndPoint.Address, ex, ex.Message));
            }

            return ok;
        }

        #endregion

        #region method IsPrivateIP

        /// <summary>
        /// Gets if sepcified IP is private IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <returns>Returns true if IP address is private IP.</returns>
        private bool IsPrivateIP(IPAddress ip)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                byte[] ipBytes = ip.GetAddressBytes();

                /* Private IPs:
                    First Octet = 192 AND Second Octet = 168 (Example: 192.168.X.X) 
                    First Octet = 172 AND (Second Octet >= 16 AND Second Octet <= 31) (Example: 172.16.X.X - 172.31.X.X)
                    First Octet = 10 (Example: 10.X.X.X)
                    First Octet = 169 AND Second Octet = 254 (Example: 169.254.X.X)

                */

                if (ipBytes[0] == 127 && ipBytes[1] == 0 && ipBytes[2] == 0 && ipBytes[3] == 1)
                {
                    return true;
                }
                if (ipBytes[0] == 192 && ipBytes[1] == 168)
                {
                    return true;
                }
                if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31)
                {
                    return true;
                }
                if (ipBytes[0] == 10)
                {
                    return true;
                }
                if (ipBytes[0] == 169 && ipBytes[1] == 254)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region method WriteFilterLog

        /// <summary>
        /// Logs specified text to log file.
        /// </summary>
        /// <param name="text">Text to log.</param>
        private void WriteFilterLog(string text)
		{
			try{
				using(FileStream fs = new FileStream(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\EXPEDIT.MailServer.ContactFilter.log",FileMode.OpenOrCreate)){
					fs.Seek(0,SeekOrigin.End);
					byte[] data = System.Text.Encoding.ASCII.GetBytes(text);
					fs.Write(data,0,data.Length);					
				}
			}
			catch{
			}
        }

        #endregion
    }
}
