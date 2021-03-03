using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Networking.Connectivity;

namespace Microsoft.Maui.Essentials
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

                    try
                    {
                        var adapter = interfaceInfo.NetworkAdapter;
                        if (adapter == null)
                            continue;

                        // http://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
                        switch (adapter.IanaInterfaceType)
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
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine($"Unable to get Network Adapter, returning Unknown: {ex.Message}");
                    }

                    yield return type;
                }
            }
        }
    }
}
