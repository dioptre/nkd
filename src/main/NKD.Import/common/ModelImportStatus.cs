using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NKD.Import
{
    /// <summary>
    /// Class to store data about the import progress
    /// </summary>
    public class ModelImportStatus
    {
        public Guid modelID { get; set; }

        public List<string> errorMessages = new List<string>();
        public List<string> warningMessages = new List<string>();
        public int finalErrorCode {get; set;}
        public static int OK = 0; 
        public static int ERROR_LOADING_FILE = 1;
        public static int ERROR_LOADING_DEFINITION_FILE = 2;
        public static int ERROR_CONNECTING_TO_DB = 3;
        public static int ERROR_WRITING_TO_DB = 4;
        public static int DATA_CONSISTENCY_ERROR = 5;
        public static int GENERAL_LOAD_ERROR = 6; 
        public static int displayMaxRecords = 50;

        private int _recordsImported = 0;
        
        public int RecordsImported { get { return _recordsImported; } set { _recordsImported = value; } }

        public ModelImportStatus()
        {
            modelID = new Guid();
        }
         public ModelImportStatus(Guid _id) {
             modelID = _id;
         }

            
     
        public void AddErrorMessage(string str){
            errorMessages.Add(str);
        }



        public void AddWarningMessage(string p)
        {
            warningMessages.Add(p);
        }

        public int linesReadFromSource { get; set; }

        //All possible lines //TODO: read all lines from source 
        public int TotalLines { get; set; }

        public string importTextFileName { get; set; }

        public string targetModelName { get; set; }


         /// <summary>
        /// generate a human readable message  - support simple full request option
        /// </summary>
        /// <returns></returns>
        public string GenerateStringMessage()
        {
            return GenerateStringMessage(false);
        }
        
        /// <summary>
        /// generate a human readable message 
        /// </summary>
        /// <returns></returns>
        public string GenerateStringMessage(bool trimReport)
        {
            string res = "";
            if (finalErrorCode == 0)
            {
                res += "Model imported into NKD";
            }
            if (finalErrorCode == 1) {
                res += "Error loading data file";
            }
            else if (finalErrorCode == 2)
            {
                res += "Error loading definition file";
            }else if (finalErrorCode == 3)
            {
                res += "Error commuinicating with NKD database";
            }
            else if (finalErrorCode == 3)
            {
                res += "Error writing blocks to NKD database";
            }
            else if (finalErrorCode == 4)
            {
                res += "Error writing to NKD database";
            }
            else if (finalErrorCode == 5)
            {
                res += "Error with data consistency";
            }

         
            
            int ct = 1;
            if (errorMessages.Count > 0)
            {
                res += "\n\r" + errorMessages.Count + " error messages during import";
                if (errorMessages.Count > displayMaxRecords)
                {
                    res += "\n\rDisplaying the first " + displayMaxRecords + " only.\n\r";
                }
                foreach (string m in errorMessages)
                {
                    if (trimReport && ct == displayMaxRecords)
                    {
                        break;
                    }
                    res += "\n\r" + ct + ") " + m ;
                    //res += "\n---------------------------------------------------\n";
                    
                    
                    ct++;
                }
            }

            if (warningMessages.Count >0)
            {
                res += "\n\r"+warningMessages.Count + " warning messages during import";
                ct = 1;
                 if (warningMessages.Count > displayMaxRecords)
                {
                    res += "\n\rDisplaying the first " + displayMaxRecords + " only.\n\r";
                }
                foreach (string m in warningMessages)
                {
                    if (trimReport && ct == displayMaxRecords)
                    {
                        break;
                    }
                    res += "\n\r" + ct + ") " + m;
                   // res += "\n---------------------------------------------------\n";
                    
                    
                    ct++;
                }
            }
            res += string.Format("\n\n For File: {0}", importTextFileName);

            res += string.Format("\n\n For Model: {0}", targetModelName);

            res += string.Format("\n\n Lines Read (including headers): {0}", linesReadFromSource);

            res += string.Format("\n\n Rows Imported: {0}", _recordsImported);

            res += "\n\n";
            return res;


        }


        public void SaveReportData() {
            string res = GenerateStringMessage(false);
            if (errorMessages.Count > 0 || warningMessages.Count > 0 || finalErrorCode > 0)
            {
                string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // attempt to write messages out to a text file somewhere
                string ts = DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString();
                ts= ts.Replace('/', '_');
                ts = ts.Replace(':', '_');
                string fileName = mydocpath + @"\NKDImportErrorMessages_" + ts + ".txt";
                using (StreamWriter outfile = new StreamWriter(fileName))
                {
                    outfile.Write(res);
                }

                Process.Start(fileName);
            }
        
        }

        public int recordsAdded { get; set; }

        public int recordsUpdated { get; set; }

        public int recordsFailed { get; set; }
    }
}
