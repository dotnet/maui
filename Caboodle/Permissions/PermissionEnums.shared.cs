using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    internal enum PermissionStatus
    {
        // Denied by user
        Denied,

        // Feature is disabled on device
        Disabled,

        // Granted by user
        Granted,

        // Restricted (only iOS)
        Restricted,

        // Permission is in an unknown state
        Unknown
    }

    internal enum PermissionType
    {
        Unknown,
        Battery,
        LocationWhenInUse,
        NetworkState,
    }
}
