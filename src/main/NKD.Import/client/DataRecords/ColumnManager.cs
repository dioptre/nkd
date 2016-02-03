using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NKD.Import.Client.Processing;
using NKD.Import.Client.Definitions;
using System.ComponentModel;
using System.IO;
using NKD.Import.FormatSpecification;

namespace NKD.Import.Client.DataRecords
{
    public class ColumnManager
    {

        public ArrayList columnData = null;
        DataColumn<int> indexCol = new DataColumn<int>("Index");   
        Type stringTypeColumn = typeof(DataColumn<string>);
        Type floatTypeColumn = typeof(DataColumn<float>);
        private float columnNullVal = -99999.99f;
        ModelColumnDefinitions colDefs = null;
        public List<string> columnNames = new List<string>();

        public ColumnManager(int numCols) {

            allowedNull = "-";
            columnData = new ArrayList(numCols);
            
            for (int i = 0; i < numCols; i++)
            {
                DataColumn<string> col = new DataColumn<string>("Column " + i);                
                columnData.Add(col);
                columnNames.Add("");
            }

            
        }

        public void SetColumnDefinitions(ModelColumnDefinitions defs) {
            colDefs = defs;
        }

        internal void SetColumnName(string s, int columnIndex) {
            columnNames[columnIndex] = s;
        }


        internal void AddDataPointToColumn(string s, int columnIndex, int recordNumber)
        {
            //get the column
            object ob = columnData[columnIndex];
            // cast the opbject ot desired tyupe
            
            if(ob.GetType() == stringTypeColumn){

                ((DataColumn<string>)ob).AddValueAt(recordNumber, s);

            }
            else if (ob.GetType() == floatTypeColumn)
            {
                float fl = columnNullVal;// float.MinValue;
                if(!s.Trim().Equals("-")){
                    float.TryParse(s, out fl);
                }
                ((DataColumn<float>)ob).AddValueAt(recordNumber, fl);
            }

        }

        
           

        internal void AddIndexColumn(int recordNumber)
        {

            indexCol.AddValueAt(recordNumber, recordNumber);

        }

        internal string ShowInfo()
        {
            string buff = "";
            int num = this.columnData.Count;
            for (int i = 0; i < num; i++) {
                object ob = columnData[i];
                buff += "Column "+i;
                if (ob.GetType() == stringTypeColumn)
                {

                    int numItems = ((DataColumn<string>)ob).GetNumValues();
                    buff += " "+numItems + ", ";    

                }
                else if (ob.GetType() == floatTypeColumn)
                {


                    int ff = ((DataColumn<float>)ob).GetNumValues();
                    buff +=  " "+ ff + ", ";    

                }
                buff += "\n";
            }

            return buff;
        }


        internal List<string> DetermineColumnDataTypes()
        {
            string buff = "";
            int num = this.columnData.Count;
            List<string> allRes = new List<string>();
            for (int i = 0; i < num; i++)
            {
                object ob = columnData[i];
                buff += "Column " + i;
                if (ob.GetType() == stringTypeColumn)
                {

                    int numItems = ((DataColumn<string>)ob).GetNumValues();
                    buff += " " + numItems + ", ";
                    DataColumn<string> col = (DataColumn<string>)ob;
                    double result = 0;
                    int parsedCount = 0;
                    int failCount = 0;
                    int totalCount = col.GetNumValues();
                    foreach (string ss in col.GetDataList())
                    { 
                        // attempt conversion to double values
                        // ignore nulls specified as empty or dashes.

                        if (ss != null && ss.Trim().Length > 0)
                        {
                            //attempt conversion to double

                            bool parsed = double.TryParse(ss, out result);
                            if (parsed)
                            {
                                parsedCount++;
                            }
                            else {
                                // check to make sure the fal is not an allowed 'null'
                                if (!ss.Trim().Equals(allowedNull))
                                {
                                    failCount++;
                                }
                                else {
                                    parsedCount++;
                                }
                            }
                        }
                        
                    }
                    if (parsedCount > (totalCount / 2))
                    {
                        col.SetDataType(ImportDataMap.NUMERICDATATYPE);
                    }
                    else {
                        col.SetDataType(ImportDataMap.NUMERICDATATYPE);
                    }
                    string res = col.name + ": numeric = " + parsedCount + " fal count = " + failCount;
                    allRes.Add(res);
                }
            }
            return allRes;


        }

        internal string GetRowAsStringAt(int rowNum)
        {
            string res = "";
            foreach (object ob in columnData) {


                if (ob.GetType() == stringTypeColumn)
                {
                    DataColumn<string> col = (DataColumn<string>)ob;
                    string rec = col.GetRecordAt(rowNum);
                    res += rec + ", ";
                }
                else if (ob.GetType() == floatTypeColumn)
                {

                    DataColumn<float> col = (DataColumn<float>)ob;
                    float rec = col.GetRecordAt(rowNum);
                    res += rec + ", ";

                }
            }
            return res;
        }



        internal string GetColumnData(int colNum)
        {
            string res = "";
            Object ob = columnData[colNum];
            if(ob.GetType() == floatTypeColumn){
                DataColumn<float> col = (DataColumn<float>)ob;
                string ss = GetColumnStats(col);
                res += ss;
            }else{
                DataColumn<string> col = (DataColumn<string>)ob;
                string ss = GetColumnStats(col);
                res += ss;
            }
            return res;


        }

        private string GetColumnStats(DataColumn<string> col)
        {
            throw new NotImplementedException();
        }

        private string GetColumnStats(DataColumn<float> col)
        {
            string stats = "";
            float total = 0;
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (float cv in col.GetDataList())
            {
                min = Math.Min(cv, min);
                max = Math.Max(cv, max);
                total += cv;
            }
            stats += "Min " + min + ", max " + max + ", total " + total;
            return stats;
        }




        internal ColumnStats GetXYZColumnData(int colNum)
        {
            
            ColumnStats stats = null;
            try
            {
                Object ob = columnData[colNum];
                if (ob.GetType() == floatTypeColumn)
                {
                    DataColumn<float> col = (DataColumn<float>)ob;
                    stats = GetColumnStatsXYZ(col);

                }
                else
                {

                    DataColumn<string> col = (DataColumn<string>)ob;
                    stats = GetColumnStatsXYZ(col);

                }
            }
            catch (Exception ex) { }
            return stats;
        }

        private ColumnStats GetColumnStatsXYZ(DataColumn<string> col)
        {
            ColumnStats stats = new ColumnStats();
            List<float> vals = new List<float>();
            // try convert to float columns then run stats
            foreach (string v in col.GetDataList()) {
                float val = float.MinValue;
                bool parsed = float.TryParse(v, out val);
                if (parsed) {
                    vals.Add(val);
                }
            }
            if (vals.Count > 0) {
                DataColumn<float> dcf = new DataColumn<float>("tmp");
                dcf.SetDataList(vals);
                stats = GetColumnStatsXYZ(dcf);
            }
            

            return stats;
        }

        private ColumnStats GetColumnStatsXYZ(DataColumn<float> col)
        {
            ColumnStats stats = new ColumnStats();
            //float total = 0;
            float min = float.MaxValue;
            float max = float.MinValue;
            int ct = 0;
            foreach (float cv in col.GetDataList())
            {
                ct++;
                min = Math.Min(cv, min);
                max = Math.Max(cv, max);
              //  total += cv;
            }
            float diff = max - min;
            stats.min = min;
            stats.max = max;
            stats.diff = diff;
            stats.count = ct;
            stats.message = "Min " + min + ", max " + max + ", length: " + diff + " ,samples counted " + ct;
            
            return stats;
        }

        internal List<SliceData> SliceBy(int colNum, float sliceWidth, float startPosition, float endPosition, out string messages, BackgroundWorker workerProcessData, int startPerc, int endPerc, string statsusTagMessage)
        {
            List<SliceData> rowIDsBySlice = new List<SliceData>();
            Object ob = columnData[colNum];
            messages = "";

            

            if (ob.GetType() == floatTypeColumn)
            {
                DataColumn<float> col = (DataColumn<float>)ob;
                int numSlices = 0;
                float currentPosition = startPosition;
                float estNumSlices = (endPosition - startPosition) / sliceWidth;

                int tot = (int)(col.ColumnCount() * estNumSlices);
                float scaleWidth = endPerc - startPerc;
                float topScale = scaleWidth / tot;
                int reportingInterval = 10;
                int reportCounter = 0;

                int pCount = 0;
                while (currentPosition <= endPosition)
                {
                    reportCounter++;
                    List<int> rowIDs = new List<int>();
                    int rowNum = 0;
                    float halfWidth = sliceWidth / 2.0f;

                    float limA = currentPosition; // (currentPosition - halfWidth);
                    float limB = currentPosition + sliceWidth; //(currentPosition + halfWidth);
                    
                    messages += "Examining slice " + numSlices + " bewtween " + limA + " and " + limB + "\n";
                    foreach (float cv in col.GetDataList())
                    {
                        pCount++;
                        if (cv > limA && cv <= limB)
                        {
                            // match
                            rowIDs.Add(rowNum);
                        }
                        rowNum++;
                    }
                    SliceData sd = new SliceData();
                    sd.sliceMin = limA;
                    sd.sliceMax = limB;
                    sd.sliceCentre = currentPosition;
                    sd.rowIDList = rowIDs;
                    rowIDsBySlice.Add(sd);
                    currentPosition += sliceWidth;

                    messages += "Slice " + numSlices + " has " + rowIDs.Count + " records\n";
                    if (reportCounter == reportingInterval)
                    {
                        float perc = (pCount * topScale) + startPerc;
                        workerProcessData.ReportProgress((int)(perc), "Slice " + (numSlices) + " of " + estNumSlices);
                        reportCounter = 0;
                    }
                    numSlices++;
                }

                messages += numSlices + " slices.";
            }

            return rowIDsBySlice;

        }

       

        internal SliceDataResults QuerySliceData(string direction, string sliceType,  SliceData aSlice, List<int> variableColumnsToCalculate, int sliceNum, float domainToQuery, bool weightByVol, bool weightByTonnes, int domainColumnID, bool weightByLength)
        {
            SliceDataResults sres = new SliceDataResults();
            
            // pick out all records for the slice
            
            // get the variable columns
            int numCols = variableColumnsToCalculate.Count;
            sres.Initialise(numCols,sliceNum);
            Object[] cols = new Object[numCols];

            for (int i = 0; i < numCols; i++) {
                cols[i] = columnData[variableColumnsToCalculate[i]];
            }

            List<int> sliceRowIDs = aSlice.rowIDList;
            
            sres.sliceCentrePoint = aSlice.sliceCentre;
            
            
         


                    for (int col = 0; col < numCols; col++)
                    {

                        DataColumn<float> varColumn = (DataColumn<float>)cols[col];
                        double tot = 0;
                        double totVol = 0;
                        double totVolAdjustedWt = 0;
                        int dataRecords = 0;
                        
                        double tonnes = 0;
                        foreach (int zz in sliceRowIDs)
                        {
                            int domain = 0;
                            domain = GetDomainForRow(zz, domainColumnID);
                            double ff = varColumn.GetRecordAt(zz);
                            if (ff != columnNullVal && domain == domainToQuery)
                            {

                                float weighting = 1;
                                if (weightByVol == true && weightByTonnes == false)
                                {
                                    weighting = GetVolumeForRow(zz);

                                }
                                else if (weightByVol == true && weightByTonnes == true)
                                {
                                    weighting = GetTonnesForRow(zz);
                                }
                                else
                                {
                                    // no weighting for composites
                                    weighting = 1.0f;
                                    if (weightByLength) {
                                        GetWeightForRow(zz);
                                    }
                                }

                                tot += ff;
                                dataRecords++;
                                double vaw = weighting * ff;
                                totVolAdjustedWt += vaw;
                                totVol += weighting;
                                if (weightByTonnes)
                                {
                                    tonnes += GetTonnesForRow(zz);
                                }

                                // write the data to a datafile
                               // string dtLine = "";
                               // dtLine =  ff + ", " + weighting;
                               // dtLine = GetDebugDataPrintout(zz) + ", Val, " + ff + " , Vol, " + weighting;
                             //   sw.WriteLine(dtLine);


                            }
                        }

                        sres.wtAvg[col] = totVolAdjustedWt / totVol;
                        sres.total[col] = tot;
                        sres.ave[col] = tot / dataRecords;
                        sres.totVolArr[col] = totVol;
                        sres.totTonneArr[col] = tonnes;

                        sres.dataRecordsUsed[col] = dataRecords;
                        sres.maxCoord = aSlice.sliceMax;
                        sres.minCoord = aSlice.sliceMin;
                        sres.midCoord = aSlice.sliceCentre;


                    }
                    //sw.WriteLine("\nSummary: ");
                    //sw.WriteLine("Ave, " + sres.ave[0]);
                    //sw.WriteLine("wtAvg, " + sres.wtAvg[0]);
                    //sw.WriteLine("Records, " + sres.dataRecordsUsed[0]);
                    //sw.WriteLine("Tot volume, " + sres.totVolArr[0]);
                    //sw.WriteLine("Tot tonnes, " + sres.totTonneArr[0]);
               // }
            
            return sres;


        }

        private string GetDebugDataPrintout(int zz)
        {
            string res  ="";
           
            return res;
        }

        /// <summary>
        /// Get the domain for a row
        /// </summary>
        /// <param name="zz"></param>
        /// <returns></returns>
        private int GetDomainForRow(int zz, int domainColumnID)
        {
            float dom = ((DataColumn<float>)columnData[domainColumnID]).GetRecordAt(zz);
            int domain = (int)dom;
            return domain;
        }

        private float GetWeightForRow(int zz) {
            float weight = ((DataColumn<float>)columnData[colDefs.GetIndexOf("Length")]).GetRecordAt(zz);
            
            return weight;
        }

        private float GetVolumeForRow(int zz)
        {

            // get teh width, height and depth and calculate volumn of cell
            float xInc = ((DataColumn<float>)columnData[colDefs.GetIndexOf("bmXINC")]).GetRecordAt(zz);
            float yInc = ((DataColumn<float>)columnData[ colDefs.GetIndexOf("colDefs.bmYINC")]).GetRecordAt(zz);
            float zInc = ((DataColumn<float>)columnData[ colDefs.GetIndexOf("colDefs.bmZINC")]).GetRecordAt(zz);

            float vol = xInc * yInc * zInc;
            return vol;
        }

        private float GetTonnesForRow(int zz)
        {

            // get teh width, height and depth and calculate volumn of cell
            float xInc = ((DataColumn<float>)columnData[colDefs.GetIndexOf("bmXINC")]).GetRecordAt(zz);
            float yInc = ((DataColumn<float>)columnData[colDefs.GetIndexOf("bmYINC")]).GetRecordAt(zz);
            float zInc = ((DataColumn<float>)columnData[colDefs.GetIndexOf("bmZINC")]).GetRecordAt(zz);
            float dens = ((DataColumn<float>)columnData[colDefs.GetIndexOf("bmDensity")]).GetRecordAt(zz);
            float vol = xInc * yInc * zInc;
            float tonnes = vol * dens;
            return tonnes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainColumnID"></param>
        /// <returns></returns>
        internal List<float> QueryAllDomains(int domainColumnID)
        {
            DataColumn<float> domainColumn = ((DataColumn<float>)columnData[domainColumnID]);
            List<float> dlist = domainColumn.GetDataList();
            List<float> uniqueDomains = new List<float>();

            foreach (float fl in dlist)
            { 
                // check if fl is in the unique list.
                bool flInList = false;
                foreach (float funique in uniqueDomains) {
                    if (funique == fl) {
                        flInList = true;
                        break;
                    }
                }
                if (!flInList) {
                    uniqueDomains.Add(fl);
                }
                
            }
            return uniqueDomains;
        }



        public string allowedNull { get; set; }


        internal int GetNumLines()
        {
            int res = 0;
            if (columnData != null)
            {
                DataColumn<string> xx = ((DataColumn<string>)columnData[0]);
                int num = xx.GetDataList().Count;
            }
            return 0;
            
        }
    }
}
