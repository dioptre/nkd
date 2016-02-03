using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.ExpressApp;
namespace NKD.Module.Win.Controllers
{
    public partial class SendConfig : DevExpress.XtraEditors.XtraForm
    {
        private XafApplication Application { get; set; }

        public SendConfig()
        {
            InitializeComponent();
        }

        public SendConfig(XafApplication application) : this()
        {
            Application = application;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            DoSend();
        }

        private void txtRecipients_KeyPress(object sender, KeyPressEventArgs e)
        {
            //pressing return should send the email
            if (e.KeyChar == (Char)Keys.Return)
            {
                //return pressed, process the send
                DoSend();
            }
        }

        private void DoSend()
        {
            if (Application != null)
                Application.SaveModelChanges();
            try
            {
                var path = System.Windows.Forms.Application.ExecutablePath.Substring(0, System.Windows.Forms.Application.ExecutablePath.LastIndexOf("\\"));
                if (txtRecipients.Text != null)
                {
                    var recipients = txtRecipients.Text.Split(new string[] { ",", ";", " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    path.SendUserConfig(recipients);
                }
            }
            //Do nothing if it fails
            catch { }
            this.Close();
        }
    }
}