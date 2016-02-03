using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NKD.Import.Client.Definitions;
using System.ComponentModel;

namespace NKD.Import.Client.Processing
{
    class ColumnProcessing
    {
        internal string QueryColumn(int colNum, DataRecords.ColumnManager columnManager)
        {
            string ss = columnManager.GetColumnData(colNum);
            return ss;
        }

        internal PhysicalDimensions QueryLocationColumns(int xCol, int yCol, int zCol, DataRecords.ColumnManager columnManager)
        {
            ColumnStats x = columnManager.GetXYZColumnData(xCol);
            ColumnStats y = columnManager.GetXYZColumnData(yCol);
            ColumnStats z = columnManager.GetXYZColumnData(zCol);
            PhysicalDimensions dim = new PhysicalDimensions();
            dim.originX = x.min;
            dim.originY = y.min;
            dim.originZ = z.min;

            dim.maximumX = x.max;
            dim.maximumY = y.max;
            dim.maximumZ = z.max;

            return dim;
        }

      

        internal List<float> GetDistinctDomains(int columnNumber, DataRecords.ColumnManager columnManager)
        {

            List<float> domains = null;
            domains =  columnManager.QueryAllDomains(columnNumber);
            return domains;
        }



        internal List<SliceDataResults> SliceAndQueryAll(string direction, string sliceType, int colNum, float sliceWidth, float startPosition, 
                                                            float endPosition, DataRecords.ColumnManager columnManager, List<int> variableColumnsToCalculate, 
                                                            float domainToQuery, out string messages, bool weightByVol, bool weightByTonnes, int domainColumnID, 
                                                            BackgroundWorker workerProcessData, int startPerc, int endPerc, string statsusTagMessage, bool weightByLength)
        {
            messages = "";
            workerProcessData.ReportProgress(startPerc, "Gathering slice data for " + statsusTagMessage);
            List<SliceData> allSlices = columnManager.SliceBy(colNum, sliceWidth, startPosition, endPosition, out messages, workerProcessData, startPerc, endPerc, statsusTagMessage);         
            // now pick out all the sample IDs for the desired slice
            messages += "\n\nSlice num";
            foreach (int vc in variableColumnsToCalculate)
            {
                messages += ",Wt avg (" + vc + "), Vol (" + vc + ") , Num blocks (" + vc + ")";
            }
            messages += "\n";

            int sliceID = 0;
            List<SliceDataResults> sliceResults = new List<SliceDataResults>();
         
            foreach (SliceData aSlice in allSlices)
            {             
                SliceDataResults sres = columnManager.QuerySliceData(direction, sliceType, aSlice, variableColumnsToCalculate, sliceID, domainToQuery, weightByVol,weightByTonnes, domainColumnID, weightByLength);
                sliceResults.Add(sres);
                messages += sres.GetMessages();
                sliceID++;
            }
            return sliceResults;
        }
    }
}
