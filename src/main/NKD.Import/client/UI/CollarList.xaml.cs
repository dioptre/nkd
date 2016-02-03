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
    /// Interaction logic for CollarList.xaml
    /// </summary>
    public partial class CollarList : Window
    {
        public CollarList()
        {
            InitializeComponent();
        }

        public void SetListData(List<string> collarsToShow) {

            listBoxCollars.ItemsSource = collarsToShow;
            int ct = collarsToShow.Count;
            labelTitleLabel.Content = "Collars in project (" + ct + ")";

        }


        private void buttonDismiss_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
