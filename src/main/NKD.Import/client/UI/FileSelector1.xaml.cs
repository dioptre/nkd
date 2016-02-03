using NKD.Import.Client.UIUtils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for FileSelector1.xaml
    /// </summary>
    public partial class FileSelector1 : UserControl
    {
        public event EventHandler FileSelected;
        
        public string FileExtension { get; set; }
        public string FileDescription { get; set; }
        public string SelectedFile { get; set; }

        public FileSelector1()
        {
            InitializeComponent();
            // set default values to CSV
            FileExtension = "*.txt";
            FileDescription = "Data files (.txt)|*.txt|All files (*.*)|*.*"; 

        }

        /// <summary>
        /// Set the name of the label in the input file chooser
        /// </summary>
        /// <param name="p"></param>
        internal void SetLabelName(string p)
        {
            labelFileChooser.Content = p;
        }

        /// <summary>
        /// Choose a file to open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

           string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
           if (fileName == null)
           {
             //  SelectedInputFile.Text = "";
           }
           else {
               SelectedInputFile.Text = fileName;
               this.FileSelected(this, new EventArgs());
           }

        }

        /// <summary>
        /// Get the selected file
        /// </summary>
        /// <returns></returns>
        internal string GetSelectedFilename()
        {
            return SelectedInputFile.Text;           
        }
    }
}
