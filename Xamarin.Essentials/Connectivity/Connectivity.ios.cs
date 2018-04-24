using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
        static ReachabilityListener listener;

        static void StartListeners()
        {
            listener = new ReachabilityListener();
            listener.ReachabilityChanged += OnConnectivityChanged;
        }

        static void StopListeners()
        {
            if (listener == null)
                return;

            listener.ReachabilityChanged -= OnConnectivityChanged;
            listener.Dispose();
            listener = null;
        }

        public static NetworkAccess NetworkAccess
        {
            get
            {
                var internetStatus = Reachability.InternetConnectionStatus();
                if (internetStatus == NetworkStatus.ReachableViaCarrierDataNetwork || internetStatus == NetworkStatus.ReachableViaWiFiNetwork)
                    return NetworkAccess.Internet;

                var remoteHostStatus = Reachability.RemoteHostStatus();
                if (remoteHostStatus == NetworkStatus.ReachableViaCarrierDataNetwork || remoteHostStatus == NetworkStatus.ReachableViaWiFiNetwork)
                    return NetworkAccess.Internet;

                return NetworkAccess.None;
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
