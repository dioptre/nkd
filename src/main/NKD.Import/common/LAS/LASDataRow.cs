using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.LAS
{
    public class LASDataRow
    {
        public double depth = 0;
        public double[] rowData;
        public int records = 0;

        public LASDataRow(int itemCount) {
            rowData = new double[itemCount];
            records = itemCount;
        }

    }
}
