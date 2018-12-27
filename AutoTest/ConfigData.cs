using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTest
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
