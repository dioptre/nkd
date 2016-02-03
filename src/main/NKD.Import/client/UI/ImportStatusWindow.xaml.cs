using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NKD.Import;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for ImportStatusWindow.xaml
    /// </summary>
    public partial class ImportStatusWindow : Window
    {
        ModelImportStatus mos;

        public ImportStatusWindow()
        {
            InitializeComponent();
        }

        public void SetData(ModelImportStatus _mos)
        {
            mos = _mos;
            string message = "";
            ObservableCollection<ErrorMessages> messageList = new ObservableCollection<ErrorMessages>();
            GenerateMessages(out message, out messageList, true);
            //DataGridMessageList.Items.
            foreach(ErrorMessages em in messageList){
                em.SetStatusTextInfo();
            }

            DataGridMessageList.Items.Clear();
            DataGridMessageList.ItemsSource = messageList;

            MessageText.Text = message;

        }

        private void buttonViewAll_Click(object sender, RoutedEventArgs e)
        {
            
            string message = "";
            ObservableCollection<ErrorMessages> messageList = new ObservableCollection<ErrorMessages>();
            GenerateMessages(out message, out messageList, false);
            //DataGridMessageList.Items.
            foreach (ErrorMessages em in messageList)
            {
                em.SetStatusTextInfo();
            }
            DataGridMessageList.ItemsSource = null;
            DataGridMessageList.ItemsSource = messageList;

            MessageText.Text = message;
        }


        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            mos.SaveReportData();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        public void GenerateMessages(out string message, out ObservableCollection<ErrorMessages> messageList, bool limitResults)
        {
            message = "";
            messageList = new ObservableCollection<ErrorMessages>();

            ResultsLinesRead.Content = mos.linesReadFromSource;
            ResultsRecordsAdded.Content = mos.recordsAdded;
            ResultsRecordsUpdated.Content = mos.recordsUpdated;
            ResultsFailed.Content = mos.recordsFailed;

            if (mos.finalErrorCode != ModelImportStatus.OK)
            {
                message += "Import complete\n";
                /*.\n" + mos.linesReadFromSource + " data lines read\n" +
                        mos.recordsAdded + " new records added \n" +
                        mos.recordsUpdated + " existing records updated.\n\n";*/
            }
            else if (mos.finalErrorCode == ModelImportStatus.DATA_CONSISTENCY_ERROR)
            {
                message += "Import complete with data warnings present.\n";/* +mos.linesReadFromSource + " data lines read\n" +
                    mos.recordsAdded + " new records added \n" +
                    mos.recordsUpdated + " existing records updated.\n\n";*/

            }
            //else
            //{
            //    MessageBox.Show("Import did not complete as expected.");
            //}

            int ct = mos.errorMessages.Count;
            ct += mos.warningMessages.Count;

            if (limitResults && ct > ModelImportStatus.displayMaxRecords) { 
                message += "\nTable limited to first "+ModelImportStatus.displayMaxRecords+"\nTo view all warnings in table, click the 'View all' button below.\n";
            }


            if (mos.finalErrorCode == 0)
            {
                message += "Model imported into NKD";
            }
            if (mos.finalErrorCode == 1)
            {
                message += "Error loading data file";
            }
            else if (mos.finalErrorCode == 2)
            {
                message += "Error loading definition file";
            }
            else if (mos.finalErrorCode == 3)
            {
                message += "Error commuinicating with NKD database";
            }
            else if (mos.finalErrorCode == 3)
            {
                message += "Error writing blocks to NKD database";
            }
            else if (mos.finalErrorCode == 4)
            {
                message += "Error writing to NKD database";
            }
            else if (mos.finalErrorCode == 5)
            {
                message += "Error with data consistency";
            }



            int recordCount = 1;

            if (mos.errorMessages.Count > 0)
            {
                message += "\n\r" + mos.errorMessages.Count + " error messages during import";

                foreach (string m in mos.errorMessages)
                {
                    ErrorMessages em = new ErrorMessages();
                    em.Status = 0;
                    em.Message = m;
                    messageList.Add(em);
                    if (limitResults && recordCount > ModelImportStatus.displayMaxRecords)
                    {
                        break;
                    }
                    recordCount++;
                }

            }

            if (mos.warningMessages.Count > 0)
            {
                message += "\n\r" + mos.warningMessages.Count + " warning messages during import";
                recordCount = 1;

                foreach (string m in mos.warningMessages)
                {

                    ErrorMessages em = new ErrorMessages();
                    em.Status = 1;
                    em.Message = m;
                    messageList.Add(em);
                    if (limitResults && recordCount > ModelImportStatus.displayMaxRecords)
                    {
                        break;
                    }
                    recordCount++;
                }
            }

        }

        
    }


    public class ErrorMessages
    {

        public int LineNumber { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }

     

        public void SetStatusTextInfo()
        {
            
            if (Status == 0)
            {
                StatusText = "Error";
            }
            else if (Status == 1)
            {
                StatusText = "Warning";
            }
            
        }
    }

}