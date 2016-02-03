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

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();

            //var filename = "PairQA.mht";
            //var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            //webBrowser1.Navigate(path);
            Uri ui = new Uri("http://nakedenterprise.org", UriKind.RelativeOrAbsolute);
            webBrowser1.Navigate(ui);
            

        }
    }
}
