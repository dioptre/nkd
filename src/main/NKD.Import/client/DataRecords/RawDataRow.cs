using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.DataRecords
{
    public class RawDataRow
    {
        int numColumns { get; set; }
        public List<string> dataItems  { get; set; }

        
        public RawDataRow()
        { 
            dataItems = new List<string>();
        }


        public void SetupRow(int cols) {
            dataItems = new List<string>(cols);
            for (int i = 0; i < cols; i++) {
                dataItems.Add("");
            }
        }



        public void AddCellAt(int idx, string val)
        {
            dataItems[idx] = val;
        }
        
        public void AddCell(string val) {
            dataItems.Add(val);
        }
    }
}
