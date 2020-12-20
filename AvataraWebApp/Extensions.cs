using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvataraWebApp
{
    public static class Extensions
    {
        public static bool IsNumeric(this string theValue)
        {
            long retNum;
            return long.TryParse(theValue, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        }
    }
}
