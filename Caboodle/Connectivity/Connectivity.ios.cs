using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Connectivity
    {
        static void StartListeners() =>
            Reachability.ReachabilityChanged += ReachabilityChanged;

        static async void ReachabilityChanged(object sender, EventArgs e)
        {
            await Task.Delay(100);
            OnConnectivityChanged();
        }

        static void StopListeners() =>
            Reachability.ReachabilityChanged -= ReachabilityChanged;

        public static NetworkAccess NetworkAccess
        {
            get
            {
                var remoteHostStatus = Reachability.RemoteHostStatus();
                var internetStatus = Reachability.InternetConnectionStatus();

                var isConnected = (internetStatus == NetworkStatus.ReachableViaCarrierDataNetwork ||
                                internetStatus == NetworkStatus.ReachableViaWiFiNetwork) ||
                              (remoteHostStatus == NetworkStatus.ReachableViaCarrierDataNetwork ||
                                remoteHostStatus == NetworkStatus.ReachableViaWiFiNetwork);

                return isConnected ? NetworkAccess.Internet : NetworkAccess.None;
            }
        }

        public static IEnumerable<ConnectionProfile> Profiles
        {
            get
            {
                var statuses = Reachability.GetActiveConnectionType();
                foreach (var status in statuses)
                {
                    switch (status)
                    {
                        case NetworkStatus.ReachableViaCarrierDataNetwork:
                            yield return ConnectionProfile.Cellular;
                            break;
                        case NetworkStatus.ReachableViaWiFiNetwork:
                            yield return ConnectionProfile.WiFi;
                            break;
                        default:
                            yield return ConnectionProfile.Other;
                            break;
                    }
                }
            }
        }
    }
}
