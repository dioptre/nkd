using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UpdateConnectionString : Window
    {
        public UpdateConnectionString()
        {
            InitializeComponent();
            //populate the string with the current connection string
            txtXString.Text = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");

            if (txtXString.Text != connectionStringsSection.ConnectionStrings["ConnectionString"].ConnectionString) //ie we have changed
            {
                try
                {
                    connectionStringsSection.ConnectionStrings["ConnectionString"].ConnectionString = txtXString.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Could not update connection string: {0}", ex.ToString());
                }
            }
            else
            {
                this.Close();
            }
        }
    }
}
