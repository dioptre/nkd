using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.ColumnSpecs
{
    public class ColumnMetaInfo
    {

        public string columnName { get; set; }
        public bool hasFK { get; set; }
        public bool isMandatory { get; set; }
        public FKSpecification fkSpec { get; set; }


        public string dbTypeName { get; set; }
    }
}
