using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;
using NKD.Module;

namespace NKD.Module.Win.Controllers
{
    

    public partial class UpdateConfig : DevExpress.XtraEditors.XtraForm
    {

        private List<Tuple<string, DateTime>> oldFiles = new List<Tuple<string, DateTime>>();

        public UpdateConfig()
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            findConfigFiles();
            gcFiles.DataSource = oldFiles;
        }


        private void findConfigFiles()
        {            
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Apps\2.0";
            ProcessDirectory(path);
        }

      
        private void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory. 
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory. 
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        private  void ProcessFile(string path)
        {
            if (path.EndsWith(".xafml"))
            {
                oldFiles.Add(new Tuple<string,DateTime>(path, File.GetLastWriteTime(path)));
            }
        }

        private void btnFileUpdate_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.DereferenceLinks = true;
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = Package.FILE_FILTER;
            if (openFileDialog.ShowDialog(Form.ActiveForm) == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                //Do something
                using (var f = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var z = f.ReadConfigFromPackage())
                    {
                        var path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                        path.WriteUserConfigFile(z);
                    }
                }
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gvFiles_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.IsGetData)
            {
                if (e.Column.FieldName == "item1")
                    e.Value = oldFiles[e.ListSourceRowIndex].Item1;
                if (e.Column.FieldName == "item2")
                    e.Value = oldFiles[e.ListSourceRowIndex].Item2;
                if (e.Column.FieldName == "#")
                    e.Value = oldFiles[e.ListSourceRowIndex].Item1;


            }
        }

        private void repositoryItemButtonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            var btn = (ButtonEdit)sender;
            using (var f = File.Open(btn.Text, FileMode.Open, FileAccess.Read))
            {
                var path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                path.WriteUserConfigFile(f);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

       
    

    }
}