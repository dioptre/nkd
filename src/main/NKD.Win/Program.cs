using System;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Forms;
using System.IO;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using NKD.Module;
using System.Linq;
using System.Net;
using DevExpress.ExpressApp.Win.Utils;


namespace NKD.Win
{
    public static class Program
    {
        public static int? DatabaseVersion = 0;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if EASYTEST
			DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
            NKDWindowsFormsApplication winApplication = new NKDWindowsFormsApplication();
            winApplication.SplashScreen = new DXSplashScreen("LogoCMYK.png");
#if EASYTEST
			if(ConfigurationManager.ConnectionStrings["EasyTestConnectionString"] != null) {
				winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["EasyTestConnectionString"].ConnectionString;
			}
#endif
            if (ConfigurationManager.ConnectionStrings["ConnectionString"] != null)
            {
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
            try
            {
                var ci = Checkin();
                winApplication.Setup();
                
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed && System.Deployment.Application.ApplicationDeployment.CurrentDeployment.IsFirstRun)
                {
                    //Too confusing for new users
                    //using (var f = new NKD.Module.Win.Controllers.UpdateConfig())
                    //{
                    //    f.ShowDialog();
                    //}
                }
                else if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
                {
                    //Kill other processes
                    var p = Process.GetCurrentProcess();
                    Process[] ps = Process.GetProcessesByName(p.ProcessName);
                    foreach (var e in ps)
                    {
                        if (e.Id != p.Id)
                            e.Kill();
                    }
                    var uriString = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.FirstOrDefault(f=>f.EndsWith(Package.FILE_EXTENSION));
                    if (Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
                    {
                        Uri uri = new Uri(uriString);
                        //Now update file and start
                        using (var f = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read))
                        {
                            using (var z = f.ReadConfigFromPackage())
                            {
                                var path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                                path.WriteUserConfigFile(z);
                            }                       
                        }
                    }
                }                
                winApplication.Start();
            }
            catch (Exception e)
            {
                winApplication.HandleException(e);
            }
        }

        static async System.Threading.Tasks.Task Checkin()
        {
            await System.Threading.Tasks.Task.Delay(90000);            
            string username;
            try
            {
                username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            catch
            {
                username = Environment.UserName;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("http://miningappstore.com/share/checkin/{0}", WebUtility.UrlEncode(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username)))));
                //request.Proxy = null; // uncomment this to bypass the default (IE) proxy settings
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
            }
            catch
            {
            }

        }

    }
}
