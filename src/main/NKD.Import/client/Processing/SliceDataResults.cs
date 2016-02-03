using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.Processing
{
    public class SliceDataResults
    {
        
        public string message { get; set; }
        public float sliceNum { get; set; }
        public int numberOfVariables { get; set; }
        public double[] total { get; set; }
        public double[] wtAvg { get; set; }
        public double[] totVolArr { get; set; }
        public double[] totTonneArr { get; set; }
        public double[] ave { get; set; }
        public double[] dataRecordsUsed { get; set; }
        public float minCoord { get; set; }
        public float maxCoord { get; set; }
        public float midCoord { get; set; }

        internal void Initialise(int numCols, int sliceN)
        {
            numberOfVariables = numCols;
            sliceNum = sliceN;
            total = new double[numCols];
            wtAvg = new double[numCols];
            totVolArr = new double[numCols];
            totTonneArr = new double[numCols];
            ave = new double[numCols];
            dataRecordsUsed = new double[numCols];

        }

        public string GetMessages() {

            string msg = ""+sliceNum+", ";
            for (int i = 0; i < numberOfVariables; i++)
            {
                msg += minCoord + ", " + maxCoord + ", " + midCoord + ", " + wtAvg[i] + ", " + ave +", "+ totVolArr[i] + ", " + dataRecordsUsed[i] ;
            }

            msg += "\n";
            return msg;
        }




        public float sliceCentrePoint { get; set; }
    }
}
