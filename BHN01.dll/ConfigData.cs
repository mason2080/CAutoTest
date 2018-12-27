using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHN01.Dll
{
    class ConfigData
    {
        public uint id;
        public byte len;
        public byte cmd;
        public byte[] data;
        public byte setStep;
        public Object objectSender;
        public byte retryTimes;
    }
}
