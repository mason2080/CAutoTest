using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Driver.BMS.BHN01
{
    class Global
    {
        public static double[,] slaveVolt = new double[30, 60];
        public static int[,] slaveTemp = new int[30, 12];
    }
}
