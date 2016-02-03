using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.ColumnSpecs
{
    public class FKSpecification
    {

        public string parentTableName { get; set; }
        public string parentColumnName { get; set; }
        public string childTableName { get; set; }
        public string childColumnName { get; set; }
        

    }
}
