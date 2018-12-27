using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Driver.Relay.I16O16
{
    interface IDeviceControl
    {
        void Connect_Device();

        void Disconnect_Device();

        void Close_Device();

        void Init_Device();
    }
}
