using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Networking.Connectivity;

namespace Microsoft.Caboodle
{
    public static partial class Connectivity
    {
        static void StartListeners() =>
             NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;

        static void NetworkStatusChanged(object sender) =>
            OnConnectivityChanged();

        static void StopListeners() =>
             NetworkInformation.NetworkStatusChanged -= NetworkStatusChanged;

        public static NetworkAccess NetworkAccess
        {
            get
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null)
                    return NetworkAccess.Unknown;

                var level = profile.GetNetworkConnectivityLevel();
                switch (level)
                {
                    case NetworkConnectivityLevel.LocalAccess:
                        return NetworkAccess.Local;
                    case NetworkConnectivityLevel.InternetAccess:
                        return NetworkAccess.Internet;
                    case NetworkConnectivityLevel.ConstrainedInternetAccess:
                        return NetworkAccess.ConstrainedInternet;
                    default:
                        return NetworkAccess.None;
                }
            }
        }

        public static IEnumerable<ConnectionProfile> Profiles
        {
            get
            {
                var networkInterfaceList = NetworkInformation.GetConnectionProfiles();
                foreach (var networkInterfaceInfo in networkInterfaceList.Where(networkInterfaceInfo => networkInterfaceInfo.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None))
                {
                    var type = ConnectionProfile.Other;

                    if (networkInterfaceInfo.NetworkAdapter != null)
                    {
                        // http://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
                        switch (networkInterfaceInfo.NetworkAdapter.IanaInterfaceType)
                        {
                            case 6:
                                type = ConnectionProfile.Ethernet;
                                break;
                            case 71:
                                type = ConnectionProfile.WiFi;
                                break;
                            case 243:
                            case 244:
                                type = ConnectionProfile.Cellular;
                                break;

                            // xbox wireless, can skip
                            case 281:
                                continue;
                        }
                    }

                    yield return type;
                }
            }
        }
    }
}
