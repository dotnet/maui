using System.Collections.Generic;
using Tizen.Network.Connection;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
        static IList<ConnectionProfile> profiles = new List<ConnectionProfile>();

        internal static void OnChanged(object sender, object e)
        {
            GetProfileListAsync();
        }

        internal static async void GetProfileListAsync()
        {
            var list = await ConnectionProfileManager.GetProfileListAsync(ProfileListType.Connected);
            profiles.Clear();
            foreach (var result in list)
            {
                switch (result.Type)
                {
                    case ConnectionProfileType.Bt:
                        profiles.Add(ConnectionProfile.Bluetooth);
                        break;

                    case ConnectionProfileType.Cellular:
                        profiles.Add(ConnectionProfile.Cellular);
                        break;

                    case ConnectionProfileType.Ethernet:
                        profiles.Add(ConnectionProfile.Ethernet);
                        break;

                    case ConnectionProfileType.WiFi:
                        profiles.Add(ConnectionProfile.WiFi);
                        break;
                }
            }
            OnConnectivityChanged();
        }

        static NetworkAccess PlatformNetworkAccess
        {
            get
            {
                Permissions.EnsureDeclared(PermissionType.NetworkState);
                var currentAccess = ConnectionManager.CurrentConnection;
                switch (currentAccess.Type)
                {
                    case ConnectionType.WiFi:
                    case ConnectionType.Cellular:
                    case ConnectionType.Ethernet:
                        return NetworkAccess.Internet;
                    default:
                        return NetworkAccess.None;
                }
            }
        }

        static IEnumerable<ConnectionProfile> PlatformConnectionProfiles
        {
            get
            {
                return profiles;
            }
        }

        static void StartListeners()
        {
            Permissions.EnsureDeclared(PermissionType.NetworkState);
            ConnectionManager.ConnectionTypeChanged += OnChanged;
            GetProfileListAsync();
        }

        static void StopListeners()
        {
            ConnectionManager.ConnectionTypeChanged -= OnChanged;
        }
    }
}
