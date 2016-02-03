using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for FileListUIControl.xaml
    /// </summary>
    public partial class FileListUIControl : UserControl
    {
        public static string lastFolder = Properties.Settings.Default.LasDirOpened;
        private List<string> fileList = new List<string>();

        //public string GetLastFolderUsed() { return lastFolder; }
        //public void SetLastFolderUsed(string lf) { lastFolder = lf; }



        public FileListUIControl()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Properties.Settings.Default.LasDirOpened;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog(this.GetIWin32Window());
            if (result == System.Windows.Forms.DialogResult.OK)
            {

                string path = dlg.SelectedPath;
                //MessageBox.Show("Path selected: " + path);
                //this.SetLastFolderUsed(path);
                Properties.Settings.Default.LasDirOpened = path;
                Properties.Settings.Default.Save();

                string[] filePaths = Directory.GetFiles(@path, "*.LAS");
                listBoxFiles.ItemsSource = filePaths;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            fileList.Clear();
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = Properties.Settings.Default.LasDirOpened;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog(this.GetIWin32Window());
            if (result == System.Windows.Forms.DialogResult.OK)
            {

                string path = dlg.SelectedPath;
                //   MessageBox.Show("Path selected: " + path);
                //this.SetLastFolderUsed(path);
                Properties.Settings.Default.LasDirOpened = path;
                Properties.Settings.Default.Save();

                DirSearch(@path);
                
                listBoxFiles.ItemsSource = fileList.ToArray();
            }
        }

        private void DirSearch(string dir)
        {
            try
            {
                foreach (var d in Directory.GetDirectories(dir))
                {
                    string[] filePaths = Directory.GetFiles(@d, "*.LAS");
                    fileList.AddRange(filePaths);
                    DirSearch(d);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string[] RetrieveFileList(){

            List<string> res = new List<string>();
            string[] rr = null;
            rr = (string[])listBoxFiles.ItemsSource;

            return rr;

        }

        private void buttonClearList_Click(object sender, RoutedEventArgs e)
        {
            string[] filePaths = new string[1];
            listBoxFiles.ItemsSource = filePaths;
        }

        private void buttonDelItemFromList_Click(object sender, RoutedEventArgs e)
        {
            string[] ns = deleteFromList();
            listBoxFiles.ItemsSource = ns;
        }

        private void listBoxFiles_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                string[] ns = deleteFromList();
                listBoxFiles.ItemsSource = ns;
            }
        }

        private string[] deleteFromList()
        {
            List<string> si = new List<string>();
            foreach (var item in listBoxFiles.SelectedItems)
            {
                string s = item.ToString();
                si.Add(s);
            }

            List<string> newList = new List<string>();

            string[] rr = null;
            rr = (string[])listBoxFiles.ItemsSource;

            foreach (string s in rr)
            {
                bool includeString = true;
                foreach (string excl in si)
                {
                    if (excl.Equals(s))
                    {
                        includeString = false;
                        break;
                    }
                }
                if (includeString)
                {
                    newList.Add(s);
                }
            }
            string[] ns = new string[newList.Count];

            for (int i = 0; i < newList.Count; i++)
            {
                ns[i] = newList[i];
            }

            return ns;
        }
    }

    static class MyWpfExtensions
    {
        public static System.Windows.Forms.IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            private readonly System.IntPtr _handle;
            public OldWindow(System.IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            System.IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }
    }

}
