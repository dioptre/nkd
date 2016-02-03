using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.ImportUtils
{
    public static class RegexUtils
    {
        public const string REGEX_IS_NUMBER = @"^\$?\-?\+?([1-9]{1}[0-9]{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\-?\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\(\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))\)$";
        public const string REGEX_IS_SINKFLOAT = @"^(S(?<sink>\+?\-?[,\.0-9]*))?-?(F(?<float>\+?\-?[,\.0-9]*))?";
        public const string REGEX_IS_CUMULATIVEFLOAT = @"^(CF(?<cumulative>\+?\-?[,\.0-9]*))";
    }
}
