//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于调用GPIB接口，需要NI-488.2支持
//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.NI4882;

namespace Driver.Port.GPIB
{
    public class GPIBClass
    {
        Device gpib = null;//new Device(0,1);

        public void open(int boardNumber,byte address)
        {
            gpib = new Device(boardNumber, address);
        }

        public void write(string cmd)
        {
            gpib.Write(cmd);
        }

        public void close()
        {
            gpib.Clear();
        }

        public string read()
        {
            return gpib.ReadString();
        }
    }
}
