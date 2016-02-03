using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NKD.Import.Client.DataRecords;
using System.IO;
using NKD.Import.Client.Processing;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace NKD.Import.Client.IO
{
    public class RawFileReader
    {
        public List<string> previewLines = new List<string>();
        public int previewCount = 250;
        char[] splitArray = new char[3];
            // set up the delimiting characters
        public ColumnManager columnManager = null;
        public bool dataLoaded = false;
        public int SkipLines { get; set; }

        public RawFileReader(char delimeter){
        
            splitArray[0] = delimeter;
            SkipLines = 0;
        }

        

       

        internal void SetColumnDefinitions(Definitions.ModelColumnDefinitions columnDefs)
        {
            columnManager.SetColumnDefinitions(columnDefs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPreviewOnly"></param>
        /// <returns></returns>
        public List<RawDataRow> LoadRawDataForPreview(string inputDataFile, IOResults ares) {            
            // first read the raw lines
            List<string> dataLines = ReadDataLines(true, inputDataFile, ares);
            List<RawDataRow> data = ParseDataLines(dataLines, ares, splitArray[0]);
            int maxCols = 0;
            foreach (RawDataRow r in data) {
                int ct = r.dataItems.Count;
                maxCols = Math.Max(ct, maxCols);
            }
            this.MaxCols = maxCols;
            previewLines = dataLines;
            return data;                 
        }



        private List<string> ReadDataLines(bool readPreview, string inputFile, IOResults ares)
        {
            //Pass the file path and file name to the StreamReader constructor
            readPreview = true;
            List<string> allLines = null;
            StreamReader sr = null;
            FileStream fs = null;

            try
            {
                fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
                sr = new StreamReader(fs);
            }
            catch (FileNotFoundException fex)
            {
                ares.errorCondition = 1;
                ares.errorMessage = "Can't find the input file that was entered.  Please make sure the file exists and that you have permission to read the file.";
                ares.extendedErrorMessage = fex.ToString();
            }
            catch (Exception ex)
            {
                ares.errorCondition = 1;
                ares.errorMessage = "There has been a problem accessing the input data file.  Pleas emake sure the file is not being open in other applications and that you have permission to read the file.";
                ares.extendedErrorMessage = ex.ToString();
            }
            allLines = new List<string>();
            if (sr != null)
            {
                string line = null;
                
                //Continue to read until you reach end of file

                if (readPreview)
                {
                    int ct = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // bypass specified number of lines
                        if (ct >= SkipLines)
                        {

                            allLines.Add(line);
                            if (ct > this.previewCount)
                            {
                                break;
                            }
                        }
                        ct++;
                    }
                }
                else {
                    while ((line = sr.ReadLine()) != null)
                    {
                        allLines.Add(line);

                    }
                }
                sr.Dispose();
                sr.Close();
                if (fs != null) {
                    fs.Close();
                }
            }
            if(ares.errorCondition == 0){
                dataLoaded = true;
            }
            return allLines;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allLines"></param>
        /// <param name="ares"></param>
        /// <returns></returns>
        private List<RawDataRow> ParseDataLines(List<string> allLines, IOResults ares, char delimeter)
        {
            List<RawDataRow> dt = new List<RawDataRow>();

         


            char[] splitArray = new char[3];
            // set up the delimiting characters
            splitArray[0] = delimeter;
            
            int fileLineNumber = 1;
            foreach (string ln in allLines)
            {
                try
                {

                    string[] items = splitQuoted(ln, delimeter);
                   // string[] items = ln.Split(splitArray, StringSplitOptions.None);
                    RawDataRow rdr = new RawDataRow();
                    rdr.dataItems = new List<string>(items);
                    dt.Add(rdr);
                }
                catch (Exception ex)
                {
                    // error parsing the line, or casting the objects
                    ares.errorMessage += ex.ToString();
                    
                }
                fileLineNumber++;
            }
            return dt;
        }

        private string[] splitQuoted(string line, char delimeter)
        {
            string[] array;
            List<string> list = new List<string>();
            do
            {
                if (line.StartsWith("\"") && line.LastIndexOf('\"') != 0)
                {
                    line = line.Substring(1);
                    int idx = line.IndexOf("\"");
                    if (idx < 0)
                    {
                        //BrokenLine
                        list.Add(line.Substring(0, Math.Max(line.IndexOf(delimeter), 0)));
                        line = line.Substring(line.IndexOf(delimeter) + 1);
                        continue;
                    }
                    while (line.IndexOf("\"", idx) == line.IndexOf("\"\"", idx))
                    {
                        idx = line.IndexOf("\"\"", idx) + 2;
                    }
                    idx = line.IndexOf("\"", idx);
                    list.Add(line.Substring(0, idx));
                    line = line.Substring(idx + 2);
                }
                else
                {
                    list.Add(line.Substring(0, Math.Max(line.IndexOf(delimeter), 0)));
                    line = line.Substring(line.IndexOf(delimeter) + 1);
                }
            }
            while (line.IndexOf(delimeter) != -1);
            list.Add(line);
            array = new string[list.Count];
            list.CopyTo(array);
            return array;
        }


        internal List<RawDataRow> LoadRawData(bool firstLineIsHeader, string fileName, IOResults ares)
        {

            List<string> rawStringData = ReadDataLines(false, fileName, ares);


            return null;
        }


        public void PerformColumnLoad(string fileName, IOResults ares, int numColumns, bool firstLineIsHeader, BackgroundWorker bw)
        {
            columnManager = new ColumnManager(numColumns);
            int maxLoadCount = 5000;
            int ct = 0;
            StreamReader sr = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
                sr = new StreamReader(fs);
            }
            catch (FileNotFoundException fex)
            {
                ares.errorCondition = 1;
                ares.extendedErrorMessage = fex.ToString();
            }
            catch (Exception ex)
            {
                ares.errorCondition = 1;                
                ares.extendedErrorMessage = ex.ToString();
            }
            
            if (sr != null)
            {                 
                    string line = null;
                    //Continue to read until you reach end of file
                    int recordNumber = 0;
                    if (firstLineIsHeader) {
                        string headers = sr.ReadLine();
                        this.SetColumnHeaders(headers, columnManager);
                    }
                    int progressIndicator = 0;
                    int reportProgressOn = 10000;
                
                    while ((line = sr.ReadLine()) != null)
                    {
                        ct++;
                        if (ct == maxLoadCount) {
                            break;
                        }
                        ParseLineIntoColumns(recordNumber, line, numColumns);
                        recordNumber++;
                        progressIndicator++;
                        if (progressIndicator == reportProgressOn) {
                            progressIndicator = 0;
                            bw.ReportProgress(recordNumber, fileName);
                        }
                    }
                
                sr.Dispose();
                sr.Close();
                if (fs != null) {
                    fs.Close();
                }
            }
           

        }

        private void SetColumnHeaders(string headers, ColumnManager columnManager)
        {

            string[] items = headers.Split(splitArray, StringSplitOptions.None);
            int columnIndex = 0;
            foreach (string s in items)
            {
                columnManager.SetColumnName(s, columnIndex);
                columnIndex++;
            }
            //columnManager.SetColumnName(s, idx);
        }

        public List<string> GetColumnHeaders()
        {
            List<string> cnames = new List<string>();
            if (columnManager != null) {
                cnames = columnManager.columnNames;
            }
            return cnames;
        }

        private void ParseLineIntoColumns(int recordNumber, string line, int numColumns)
        {
            //check here to see if the line contains any quotes "", if so change the way the split operates
            string[] items = line.Split(splitArray, StringSplitOptions.None); ;
            if (line.Contains("\""))
            {
                List<string> templist = new List<string>();
                string tempstring = "";
                //int count = 0;
                bool insideQuotes = false;
                bool endQuotes = false;

                for (int i = 0; i < items.Count(); i++) //quick dirty approximate count
                {
                    //we know we have quotes inside the line, iterate through each item in the items array and add to templist
                    //count++;
                    if (items[i].Contains("\"") || insideQuotes)
                    {
                        if (!insideQuotes)
                        {
                            templist.Add(items[i]);
                            templist[templist.Count - 1] += splitArray[0];
                        }
                        else
                        {
                            tempstring += items[i];
                            if (!items[i].Contains("\""))
                                tempstring += splitArray[0];
                        }

                        if (items[i].Contains("\"") && insideQuotes)
                        {
                            insideQuotes = false;
                            templist[templist.Count - 1] += tempstring;
                            endQuotes = true;
                        }

                        if (!endQuotes)
                        {
                            insideQuotes = true;
                        }

                    }
                    else
                        templist.Add(items[i]);
                }
            }

            int columnIndex = 0;
            columnManager.AddIndexColumn(recordNumber);
            foreach (string s in items)
            {
                if (columnIndex >= columnManager.columnData.Count)
                    break;
                columnManager.AddDataPointToColumn(s, columnIndex, recordNumber);
                
                columnIndex++;
            }
        }

        public string GetColumnStats() {

            return columnManager.ShowInfo();
        }

        internal List<string> DetermineColumnDataTypes()
        {
            return columnManager.DetermineColumnDataTypes();
        }

        public int MaxCols { get; set; }



        internal List<float> GetDomains(string blockDomain)
        {
            
            // first find the column index contining the blockDomain
            int idx = columnManager.columnNames.IndexOf(blockDomain);
            
            ColumnProcessing sp = new ColumnProcessing();
            // now we need to go through the entire column and pick out unique domains
            List<float> doms = sp.GetDistinctDomains(idx, columnManager);

            return doms;
        }

        internal List<float> GetDomains(int domainColumn)
        {

            

            ColumnProcessing sp = new ColumnProcessing();
            // now we need to go through the entire column and pick out unique domains
            List<float> doms = sp.GetDistinctDomains(domainColumn, columnManager);

            return doms;
        }

        internal int GetColumnIDFor(string columnToFind)
        {
            int idx = columnManager.columnNames.IndexOf(columnToFind);
            return idx;
        }




        /// <summary>
        /// Search the mapped columns to find the specified column, and determine the min
        /// </summary>
        /// <param name="NKDColumnID"></param>
        /// <returns></returns>
        internal ColumnStats GetDimensions(int columnID)
        {
            ColumnStats cs = columnManager.GetXYZColumnData(columnID);
            return cs;
        }
    }
}
