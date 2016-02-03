using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.Processing
{
    class CreateArraysForCharts
    {
        private List<SliceDataResults> sliceResults;

        public CreateArraysForCharts(List<SliceDataResults> sliceResults)
        {
           
            this.sliceResults = sliceResults;
        }

        internal List<float> getXSeries()
        {
            List<float> xd = new List<float>();
            foreach(SliceDataResults sl in sliceResults){
                //xd.Add( (float)sl.sliceNum );
                xd.Add((float)sl.sliceCentrePoint);
            }
            return xd;
        }

        internal List<float> getYSeriesWtAvg(int variableNum)
        {
            List<float> yd = new List<float>();
            foreach (SliceDataResults sl in sliceResults)
            {
                yd.Add((float)sl.wtAvg[variableNum]);
            }
            return yd;
        }

        internal List<float> getYSeriesNumSamples(int variableNum)
        {
            List<float> yd = new List<float>();
            foreach (SliceDataResults sl in sliceResults)
            {
                yd.Add((float)sl.dataRecordsUsed[variableNum]);
            }
            return yd;
        }

    }
}
