using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Essentials
{
    public enum ConnectionProfile
    {
        Bluetooth,
        Cellular,
        Ethernet,
        WiMAX,
        WiFi,
        Other
    }

    public enum NetworkAccess
    {
        Unknown = 0,
        None = 1,
        Local = 2,
        ConstrainedInternet = 3,
        Internet = 4
    }
}
