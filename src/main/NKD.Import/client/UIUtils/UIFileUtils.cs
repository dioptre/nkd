using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.Client.UIUtils
{
    /// <summary>
    ///  Some useful file utilities
    /// </summary>
    public static class UIFileUtils
    {

        public static string ShowOpenFileChoose(string fileTypes, string typeDescritpion, string presetFile){
            string res = null;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = fileTypes;// ".csv"; // Default file extension
            dlg.Filter = typeDescritpion;// "Data files (.csv, .txt)|*.csv;*.txt|All files (*.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                res = filename;                
            }

            return res;
        }

        internal static string ShowSaveFileChoose(string FormatFileExtension, string FormatFileDescription, string SelectedFormatFile)
        {
            string res = null;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = FormatFileExtension;// ".csv"; // Default file extension
            dlg.Filter = FormatFileDescription;// "Data files (.csv, .txt)|*.csv;*.txt|All files (*.*)|*.*"; // Filter files by extension
            dlg.FileName = SelectedFormatFile;
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                res = filename;                
            }

            return res;
        
        }
    }
}
