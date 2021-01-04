using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avatara.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNumeric(this string theValue)
        {
            long retNum;
            return long.TryParse(theValue, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        }
    }
}
