using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Module.BusinessObjects;

namespace NKD.Import.ImportUtils
{
    public class UnitQueries
    {


        internal DictionaryUnit FindUnits(string theUnit)
        {
            var entityObj = new NKDC(BaseImportTools.XSTRING, null);
            
            DictionaryUnit xd = (from c in entityObj.DictionaryUnits where c.StandardUnitName.Trim().Equals(theUnit) select c).FirstOrDefault();
            return xd;

        }
    }
}
