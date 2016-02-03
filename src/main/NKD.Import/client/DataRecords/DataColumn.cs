using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.DataRecords
{
    class DataColumn<T> : DataColumnBase
    {

        //List<int> id = new List<int>();
        List<T> dataRecords = new List<T>();

        public DataColumn(string columnName) : base() {
            name = columnName;
        }

        public T GetRecordAt(int i) {
            return dataRecords[i];
        }

         

        public void AddValueAt(int i, T data)
        {
            dataRecords.Insert(i,data);
        }

        public int GetNumValues() {
            return this.dataRecords.Count;
        }

        public List<T> GetDataList() {
            return dataRecords;
        }

        internal int ColumnCount()
        {
            return dataRecords.Count;
        }

        internal void SetDataType(string p)
        {
            this.SetColumnDataType = p;
        }

        public string SetColumnDataType { get; set; }

       

        internal void SetDataList(List<T> vals)
        {
            dataRecords = vals;
        }
    }
}
