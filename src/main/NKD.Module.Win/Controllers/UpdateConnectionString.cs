using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.ExpressApp;
using System.Configuration;
namespace NKD.Module.Win.Controllers
{
    public partial class UpdateConnectionString : DevExpress.XtraEditors.XtraForm
    {
        private XafApplication Application { get; set; }

        public UpdateConnectionString()
        {
            InitializeComponent();
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            txtXString.Text = connectionStringsSection.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        public UpdateConnectionString(XafApplication application)
            : this()
        {
            Application = application;
        }
      
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DoUpdate();
        }

        private void txtXString_KeyPress(object sender, KeyPressEventArgs e)
        {
            //pressing return should send the email
            if (e.KeyChar == (Char)Keys.Return)
            {
                //return pressed, process the send
                DoUpdate();
            }
        }

        private bool DoUpdate()
        {
            if (Application != null)
                Application.SaveModelChanges();
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
                connectionStringsSection.ConnectionStrings["ConnectionString"].ConnectionString = txtXString.Text;
                config.Save();
                System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return true;
            }
            //Do nothing if it fails
            catch
            {
                return false;
            }
        }
    }
}