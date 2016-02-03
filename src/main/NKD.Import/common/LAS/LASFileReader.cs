using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NKD.Import.LAS
{
    public class LASFileReader
    {

        double supportedLasVersion = 2.0;


        public LASFile ReadLASFile(string inputFile, int columnOffset, out int errorCode)
        {
            errorCode = 0;
            LASFile lf1 = null;
            if (inputFile.ToLower().EndsWith("las"))
            {
                lf1 = ReadDataLines(inputFile, out errorCode);
            }
            else {

                lf1 = ReadCSVLines(inputFile, columnOffset, out errorCode);
            }

            // make sure column header names are not duplicated
            List<string> headers = DeDuplicate(lf1.columnHeaders);
            lf1.columnHeaders = headers;
            return lf1;
        }

        private List<string> DeDuplicate(List<string> list)
        {

            Dictionary<string, string> colDic = new Dictionary<string, string>();
            List<string> newColNames = new List<string>();
            foreach (string h in list) {
                string keyName = h;
                bool hasEntry = colDic.ContainsKey(keyName);
                if (!hasEntry)
                {
                    colDic.Add(h, h);
                }
                else {
                    int counter = 1;
                    while (true) {

                        string newKey = keyName + " (" + counter + ")";
                        
                        bool hasNewEntry = colDic.ContainsKey(newKey);
                        if (!hasNewEntry) {
                            keyName = newKey;
                            colDic.Add(keyName, keyName);
                            break;
                        }
                        counter++;
                    }
                }
                newColNames.Add(keyName);

            }



            return newColNames;

        }



        public LASFile ReadDataLines(string inputFile, out int  errorCode)
        {
            //Pass the file path and file name to the StreamReader constructor
            LASFile res = new LASFile();
            errorCode = 0;
            List<string> errorInfo = new List<string>();

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(inputFile);
                res.filePath = inputFile;
            }
            catch (FileNotFoundException fex)
            {

            }
            catch (Exception ex)
            {

            }
         
            int columnCount = 0;
            List<string> columnHeaders = new List<string>();
            List<LASDataRow> dataRows = new List<LASDataRow>();
            if (sr != null)
            {
                //Read the first line of text
                string line = null;
                bool inDataSection = false;
                bool inCurveSection = false;
                bool inWellInfoSection = false;
                bool inVersionSection = false;
                bool inParameterSection = false;
                int lineCount = 0;
                List<string> curveHeaders = new List<string>();
                try
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineCount++;

                        if (!line.StartsWith("#"))
                        {



                            if (line.StartsWith("~"))
                            {
                                // this is the start/end of a seciton, so turn everything off
                                inDataSection = false;
                                inCurveSection = false;
                                inWellInfoSection = false;
                                inVersionSection = false;
                                inParameterSection = false;
                            }

                            if (line.ToUpper().StartsWith("~C"))
                            {
                                inCurveSection = true;

                            }
                            if (line.ToUpper().StartsWith("~P"))
                            {
                                inParameterSection = true;

                            }
                            if (line.ToUpper().StartsWith("~W"))
                            {
                                inWellInfoSection = true;

                            }
                            if (line.ToUpper().StartsWith("~V"))
                            {
                                inVersionSection = true;

                            }
                            if (line.ToUpper().StartsWith("~A"))
                            {
                                try
                                {

                                    inDataSection = true;
                                    if (curveHeaders.Count < 1)
                                    {
                                        columnHeaders = ParseHeaderLine(line);
                                    }
                                    else
                                    {
                                        columnHeaders = curveHeaders;
                                    }
                                    columnCount = columnHeaders.Count;
                                }
                                catch (Exception ex)
                                {
                                    errorCode = LASErrorCodes.ERROR_PARSING_LAS_HEADERS;
                                    errorInfo.Add(LASErrorCodes.LookupCode(errorCode));
                                    break;
                                }
                            }
                            else
                            {

                                if (inDataSection)
                                {
                                    if (line.Trim().Length > 0)
                                    {
                                        LASDataRow ldr = null;
                                        int err = 0;
                                        try
                                        {

                                            ldr = ParseDataLine(line, columnCount, out err, res.nullValue);

                                        }
                                        catch (Exception ex) { }
                                        if (ldr != null)
                                        {
                                            dataRows.Add(ldr);
                                        }
                                        else
                                        {
                                            errorInfo.Add("Unable to parse LAS data row at line: " + lineCount);
                                        }

                                    }
                                }
                            }

                            if (inWellInfoSection)
                            {
                                res.AddWellSectionHeaderLine(line);
                                // attempt to find the NULL value
                                if (line.Trim().StartsWith("NULL"))
                                {
                                    // parse the line to get the null value
                                    try
                                    {
                                        // NULL will appear like this:
                                        //NULL.    -999.25                        :NULL VALUE
                                        int idx = line.IndexOf('.');
                                        int endIdx = line.IndexOf(':');
                                        string leftOver = line.Substring(idx + 1, endIdx - idx - 1);
                                        double nullVal = Convert.ToDouble(leftOver.Trim());
                                        res.nullValue = nullVal;
                                    }
                                    catch (Exception ex)
                                    {
                                        errorInfo.Add("Could not find NULL value, assuming default of " + string.Format("{0:0.###}", res.nullValue));
                                    }
                                }
                            }
                            if (inParameterSection) {
                                res.AddParameterSectionHeaderLine(line);
                            }
                            if (inVersionSection)
                            {
                                res.AddVersionSectionHeaderLine(line);

                                if (line.Trim().StartsWith("VERS"))
                                {
                                    // parse the line to get the null value
                                    try
                                    {
                                        int idx = line.IndexOf('.');
                                        int endIdx = line.IndexOf(':');
                                        string leftOver = line.Substring(idx + 1, endIdx - idx - 1);
                                        double versionVal = Convert.ToDouble(leftOver.Trim());
                                        res.versionValue = versionVal;
                                        if (versionVal != supportedLasVersion)
                                        {
                                            errorInfo.Add("LAS file version '" + versionVal + "' is unsupported.  This software can only be used to view LAS 2.0 files.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                else if (line.Trim().StartsWith("WRAP."))
                                {
                                    // parse the line to get the wrap value
                                    try
                                    {
                                        int idx = line.IndexOf('.');
                                        int endIdx = line.IndexOf(':');
                                        string leftOver = line.Substring(idx + 1, endIdx - idx - 1);
                                        string wrap = leftOver.Trim();
                                        res.versionWrap = wrap;
                                        if (wrap.ToUpper().Equals("YES"))
                                        {
                                            errorInfo.Add("The selected LAS file has WRAP set to 'YES'.  This feature is not supported by this software, LAS files must have one depth step per line.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }


                            }
                            if (inCurveSection)
                            {
                                res.AddCurveSectionHeaderLine(line);
                                // decode a line and give it to the headers section
                                int endIdx = line.IndexOf(':');
                                if (endIdx > 0)
                                {
                                    string secondPart = line.Substring(0, endIdx).Trim();
                                    int ptIdx = secondPart.IndexOf('.');
                                    string units = "";
                                    string colName = "";
                                    if (ptIdx > 0)
                                    {
                                        colName = secondPart.Substring(0, ptIdx);
                                        units = secondPart.Substring(ptIdx, secondPart.Length - ptIdx);
                                        int firstSpaceAfterUnits = units.Trim().IndexOf(' ');
                                        if (firstSpaceAfterUnits > 0)
                                        {
                                            units = units.Substring(0, firstSpaceAfterUnits);
                                        }

                                    }
                                    string hName = colName.Trim() + units;
                                    //string leftOver = line.Substring(0, endIdx).Trim();
                                    if (!hName.ToUpper().StartsWith("DEPT"))
                                    {
                                        curveHeaders.Add(hName);
                                    }
                                }
                            }
                        }

                        }
                        if (dataRows.Count == 0)
                        {
                            errorCode = LASErrorCodes.NO_DATA_ROWS_LOADED;
                            errorInfo.Add(LASErrorCodes.LookupCode(errorCode));
                        }

                    
                }
                catch (Exception ex)
                {
                    errorCode = LASErrorCodes.ERROR_READING_LAS_DATA_SECTION;
                    errorInfo.Add(LASErrorCodes.LookupCode(errorCode));
                }

                
                res.columnHeaders = columnHeaders;
                res.dataRows = dataRows;
                res.errorDetails = errorInfo;
                sr.Dispose();
                sr.Close();

            }
            else {
                errorCode = LASErrorCodes.STREAM_OPEN_FAILED;
            }
           return res;

        }

        public LASFile ReadCSVLines(string inputFile, int columnOffset, out int errCode)
        {
            //Pass the file path and file name to the StreamReader constructor
            LASFile res = new LASFile();
            StreamReader sr = null;

            List<string> errorInfo = new List<string>();
            errCode = 0;
            try
            {
                sr = new StreamReader(inputFile);
            }
            catch (FileNotFoundException fex)
            {
                errCode = LASErrorCodes.CSV_FILE_NOT_FOUND;
            }
            catch (Exception ex)
            {
                errCode = LASErrorCodes.CSV_STREAM_ERROR; ;
            }

            
            if (errCode != 0) {
                errorInfo.Add("Error loading file.  Message: "+LASErrorCodes.LookupCode(errCode));
            }
           
            int columnCount = 0;
            int maxErrors = 20;
            List<string> columnHeaders = new List<string>();
            List<LASDataRow> dataRows = new List<LASDataRow>();
            
            if (sr != null)
            {

                string headLine = sr.ReadLine();
                try
                {
                    res.columnHeaders = ParseCSVHeaderLine(columnOffset, headLine);
                }
                catch (Exception ex) {
                    errCode = LASErrorCodes.CSV_HEADER_PARSE_FAILED;
                    
                }
                if (errCode == 0)
                {
                    columnCount = res.columnHeaders.Count;

                    string line;
                    //Read the first line of text

                    Dictionary<int, int> inputErrors = new Dictionary<int, int>();
                    int lineNumber = 1;

                    while ((line = sr.ReadLine()) != null)
                    {
                        lineNumber++;
                        int lineErr = 0;
                        LASDataRow ldr = ParseCSVLine(columnOffset, line, columnCount, out lineErr, res.nullValue);
                        if (lineErr == 0)
                        {
                            dataRows.Add(ldr);
                        }
                        else
                        {
                            
                            errorInfo.Add("Line: " + lineNumber + " : " + LASErrorCodes.LookupCode(lineErr));
                            if (errorInfo.Count == maxErrors) {
                                errCode = LASErrorCodes.MAERRORS_ENCOUNTERED;
                                errorInfo.Add(LASErrorCodes.LookupCode(errCode));
                                break;
                            }
                        }

                    }

                    res.dataRows = dataRows;
                }
                sr.Dispose();
                sr.Close();
                res.errorDetails = errorInfo;
            }
            return res;

        }

        private LASDataRow ParseDataLine(string line, int columnCount, out int errorCode, double nullValue)
        {
            // Parse a line such as this:
            //~A   DEPT[M]    Gamma[cp    Current[    R16[Ohm-    R32[Ohm-    R64[Ohm-    R8[Ohm-m      SP[mV]    SPR[Ohm]    Density[    Caliper[    Full Wav
            char[] splitArray = new char[3];
            errorCode = 0;
            // set up the delimiting characters
            splitArray[0] = '\t';
            splitArray[1] = ',';
            splitArray[2] = ' ';
            List<string> dataItemList = new List<string>();
            string[] items = line.Split(splitArray, StringSplitOptions.RemoveEmptyEntries);
            //if (items.Length < columnCount) {
            //    errorCode = LASErrorCodes.INSUFFICIENT_DATA_COLUMNS;
            //}
            // ignore index 0, as it is ~S
            LASDataRow ldr = new LASDataRow(columnCount);
            double dt = Convert.ToDouble(items[0].Trim()); 
            ldr.depth = dt;
            for (int i = 1; i < items.Length; i++)
            {

                double ii = nullValue;
                Double.TryParse(items[i].Trim(), out ii);
                //try { 
                //    ii = Convert.ToDouble(items[i].Trim()); }
                //catch (Exception ex) { 
                //}
                // first item shoudl always be depth
                ldr.rowData[i-1] = ii;

            }

            return ldr;
        }

        private LASDataRow ParseCSVLine(int columnOffset, string line, int columnCount, out int errorCode, double nullValue)
        {
            // Parse a line such as this:
            
            char[] splitArray = new char[3];
            // set up the delimiting characters
            splitArray[0] = '\t';
            splitArray[1] = ',';
            errorCode = 0;
            List<string> dataItemList = new List<string>();
            string[] items = line.Split(splitArray, StringSplitOptions.None);
            // ignore index 0, as it is ~S
            LASDataRow ldr = new LASDataRow(columnCount);
            if (items.Length - columnOffset != columnCount) { 
                // not enough items 
                int xxx = 0;
            } 
            try
            {
                string strDeptth = items[columnOffset];
                try
                {
                    ldr.depth = Convert.ToDouble(strDeptth.Trim());
                }
                catch (Exception ex)
                {
                    errorCode = LASErrorCodes.CSV_DEPTH_FIELD_PARSE_ERROR;
                   
                }
                if (errorCode == 0)
                {
                    for (int i = columnOffset + 1; i < items.Length; i++)
                    {

                        double ii = nullValue;
                        if (items[i].Trim().Length > 0)
                        {
                            try
                            {
                                ii = Convert.ToDouble(items[i].Trim());
                            }
                            catch (Exception ex) { };
                        }

                        ldr.rowData[i - (columnOffset + 1)] = ii;

                    }
                }
            }
            catch (IndexOutOfRangeException) {
                errorCode = LASErrorCodes.CSV_PARSE_INDEERROR;
            }
            catch (Exception ex)
            {
                errorCode = LASErrorCodes.CSV_PARSE_ERROR; ;
            }
            

            return ldr;
        }


        private LASDataRow ParseCSVLineFIXED(string line, int columnCount, double nullValue)
        {
            // Parse a line such as this:

            char[] splitArray = new char[3];
            // set up the delimiting characters
            splitArray[0] = '\t';
            splitArray[1] = ',';

            List<string> dataItemList = new List<string>();
            string[] items = line.Split(splitArray, StringSplitOptions.None);
            // ignore index 0, as it is ~S
            LASDataRow ldr = new LASDataRow(8);
            int columnOffset = 5;
            //HOLEID, PROJECTCODE,DEPTHSTEP, RUN, GEOPHYSGID, DEPTH , CALLMM, GAMMAP, DESSGC, DELSGC, RESHOM, NESSSN, SO2FUF, UCSMPA
            string depth = items[columnOffset + 0];
            string CALLMM = items[columnOffset + 1];
            string GAMMAP = items[columnOffset + 2];
            string DESSGC = items[columnOffset + 3];
            string DELSGC = items[columnOffset + 4];
            string RESHOM = items[columnOffset + 5];
            string NESSSN = items[columnOffset + 6];
            string SO2FUF = items[columnOffset + 7];
            string UCSMPA = items[columnOffset + 8];
            string[] newItems = { /*depth,*/ CALLMM, GAMMAP, DESSGC, DELSGC, RESHOM, NESSSN, SO2FUF, UCSMPA };

            for (int i = 0; i < newItems.Length; i++)
            {

                double ii = nullValue;
                if (newItems[i].Trim().Length > 0)
                {
                    try
                    {
                        ii = Convert.ToDouble(newItems[i].Trim());
                    }
                    catch (Exception ex) { };
                }

                ldr.rowData[i] = ii;

            }
            ldr.depth = Convert.ToDouble(depth.Trim());

            return ldr;
        }


        private List<string> ParseCSVHeaderLine(int columnOffset, string line)
        {
            // Parse a line such as this:
            //~A   DEPT[M]    Gamma[cp    Current[    R16[Ohm-    R32[Ohm-    R64[Ohm-    R8[Ohm-m      SP[mV]    SPR[Ohm]    Density[    Caliper[    Full Wav
            char[] splitArray = new char[3];
            // set up the delimiting characters
            splitArray[0] = '\t';
            splitArray[1] = ',';
            splitArray[2] = ' ';
            List<string> headerList = new List<string>();
            string[] items = line.Split(splitArray, StringSplitOptions.RemoveEmptyEntries);
            // ignore index 0, as it is ~S
            int ct = 0;
            for (int i = columnOffset+1; i < items.Length; i++)
            {
                headerList.Add(items[i].Trim());
                ct++;

            }

            return headerList;
        }

        private List<string> ParseHeaderLine(string line)
        {
            // Parse a line such as this:
            //~A   DEPT[M]    Gamma[cp    Current[    R16[Ohm-    R32[Ohm-    R64[Ohm-    R8[Ohm-m      SP[mV]    SPR[Ohm]    Density[    Caliper[    Full Wav
            char[] splitArray = new char[3];
            // set up the delimiting characters
            splitArray[0] = '\t';
            splitArray[1] = ',';
            splitArray[2] = ' ';
            List<string> headerList = new List<string>();
            string[] items = line.Split(splitArray, StringSplitOptions.RemoveEmptyEntries);
            // ignore index 0, as it is ~S
            for (int i = 2; i < items.Length; i++) {
                headerList.Add(items[i].Trim());
            
            }

            return headerList;
        }

    }
}

