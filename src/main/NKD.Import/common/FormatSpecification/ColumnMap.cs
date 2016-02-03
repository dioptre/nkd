using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.FormatSpecification
{
    public class ColumnMap
    {
        public ColumnMap() { }

        public ColumnMap(string _sourceColumnName, int _sourceColumnNumber, string _targetColumnTable, string _targetColumnName, string _importDataType, string _defaultValue, string _defaultText, string _units)
        {
            sourceColumnName = _sourceColumnName;
            sourceColumnNumber = _sourceColumnNumber;
            targetColumnTable = _targetColumnTable;
            targetColumnName = _targetColumnName;
            importDataType = _importDataType;
            defaultValue = _defaultValue;
            defaultText = _defaultText;
            targetUnits = _units;
        }

        public string sourceColumnName { get; set; }      //Parameter Description
        public int sourceColumnNumber { get; set; }       // the column number in the input file

        public string importDataType { get; set; }        // the column type in the input file
        public string importDataTypeName { get; set; }    // the column number in the input file
        public string targetColumnName { get; set; }
        public string targetColumnTable { get; set; }

        public string defaultValue { get; set; }          // default gets used if there is no import column, e.g. used for setting parent IDs that remain constant throught the transaction
        public string defaultText { get; set; }           //same as above (text value though)
        public string targetUnits { get; set; }
        public bool hasFKRelation { get; set; }
        public string fkRelationTable { get; set; }
        public string fkRelationColumn { get; set; }
    }
}
