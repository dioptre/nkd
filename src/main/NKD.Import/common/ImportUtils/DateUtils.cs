using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace NKD.Import.ImportUtils
{
    public static class DateUtils
    {
        public static DateTime? CleanDate(string date)
        {
            var sd = date.Split(new char[] {',', ' ', '\t', '\'', '"'}, StringSplitOptions.RemoveEmptyEntries);
            var r = new Regex("[0-9]");
            for(int i=0 ; i < sd.Length; i++)
            {
                if (r.IsMatch(sd[i]))
                    sd[i] = sd[i].ToLower()
                        .Replace("nd","")
                        .Replace("th","")
                        .Replace("rd","")
                        .Replace("st","");
            
            }
            DateTime? dt = null;
            DateTime dtr;
            if (DateTime.TryParse(string.Join(" ",sd), out dtr))
                return dtr;
            else 
                return null;
        }
    }
}
