using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSImport
{
    public class Utils
    {
        public static Double MillimetersToFeet(double millimeters)
        {
            return 0.00328 * millimeters;
        }

        public static Double MetersToFeet(double meters)
        {
            return 3.28 * meters;
        }
    }
}
