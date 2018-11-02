using System.Collections.Generic;
using System.Linq;
using Windows.Networking.Connectivity;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
        static void StartListeners() =>
             NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;

        static void NetworkStatusChanged(object sender) =>
            OnConnectivityChanged();

        static void StopListeners() =>
             NetworkInformation.NetworkStatusChanged -= NetworkStatusChanged;

        static NetworkAccess PlatformNetworkAccess
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

        static IEnumerable<ConnectionProfile> PlatformConnectionProfiles
        {
            get
            {
                var networkInterfaceList = NetworkInformation.GetConnectionProfiles();
                foreach (var interfaceInfo in networkInterfaceList.Where(nii => nii.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None))
                {
                    var type = ConnectionProfile.Unknown;

                    if (interfaceInfo.NetworkAdapter != null)
                    {
                        // http://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
                        switch (interfaceInfo.NetworkAdapter.IanaInterfaceType)
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
