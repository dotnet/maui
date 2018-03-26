using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;

namespace Microsoft.Caboodle
{
    public partial class Connectivity
    {
        static ConnectivityBroadcastReceiver conectivityReceiver;
        static bool hasPermission;

        static void ValidatePermission()
        {
            if (hasPermission)
                return;

            var permission = Manifest.Permission.AccessNetworkState;
            if (!Platform.HasPermissionInManifest(permission))
                throw new PermissionException(permission);

            hasPermission = true;
        }

        static void StartListeners()
        {
            ValidatePermission();
            conectivityReceiver = new ConnectivityBroadcastReceiver(OnConnectivityChanged);
            Platform.CurrentContext.RegisterReceiver(conectivityReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        static void StopListeners()
        {
            Platform.CurrentContext.UnregisterReceiver(conectivityReceiver);
            conectivityReceiver?.Dispose();
            conectivityReceiver = null;
        }

        static NetworkAccess IsBetterAccess(NetworkAccess currentAccess, NetworkAccess newAccess) =>
            newAccess > currentAccess ? newAccess : currentAccess;

        public static NetworkAccess NetworkAccess
        {
            get
            {
                ValidatePermission();
                try
                {
                    var currentAccess = NetworkAccess.None;
                    var manager = Platform.ConnectivityManager;

                    if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                    {
                        foreach (var network in manager.GetAllNetworks())
                        {
                            try
                            {
                                var capabilities = manager.GetNetworkCapabilities(network);

                                if (capabilities == null)
                                    continue;

                                // Check to see if it has the internet capability
                                if (!capabilities.HasCapability(NetCapability.Internet))
                                {
                                    // Doesn't have internet, but local is possible
                                    currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Local);
                                    continue;
                                }

                                var info = manager.GetNetworkInfo(network);

                                ProcessNetworkInfo(info);
                            }
                            catch
                            {
                                // there is a possibility, but don't worry
                            }
                        }
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        foreach (var info in manager.GetAllNetworkInfo())
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            ProcessNetworkInfo(info);
                        }
                    }

                    void ProcessNetworkInfo(NetworkInfo info)
                    {
                        if (info == null || !info.IsAvailable)
                            return;

                        if (info.IsConnected)
                            currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Internet);
                        else if (info.IsConnectedOrConnecting)
                            currentAccess = IsBetterAccess(currentAccess, NetworkAccess.ConstrainedInternet);
                    }

                    return currentAccess;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to get connected state - do you have ACCESS_NETWORK_STATE permission? - error: {0}", e);
                    return NetworkAccess.Unknown;
                }
            }
        }

        public static IEnumerable<ConnectionProfile> Profiles
        {
            get
            {
                ValidatePermission();
                var manager = Platform.ConnectivityManager;
                if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                {
                    foreach (var network in manager.GetAllNetworks())
                    {
                        NetworkInfo info = null;
                        try
                        {
                            info = manager.GetNetworkInfo(network);
                        }
                        catch
                        {
                            // there is a possibility, but don't worry about it
                        }

                        var p = ProcessNetworkInfo(info);
                        if (p.HasValue)
                            yield return p.Value;
                    }
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    foreach (var info in manager.GetAllNetworkInfo())
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        var p = ProcessNetworkInfo(info);
                        if (p.HasValue)
                            yield return p.Value;
                    }
                }

                ConnectionProfile? ProcessNetworkInfo(NetworkInfo info)
                {
                    if (info == null || !info.IsAvailable || !info.IsConnectedOrConnecting)
                        return null;

                    return GetConnectionType(info.Type, info.TypeName);
                }
            }
        }

        internal static ConnectionProfile GetConnectionType(ConnectivityType connectivityType, string typeName)
        {
            switch (connectivityType)
            {
                case ConnectivityType.Ethernet:
                    return ConnectionProfile.Ethernet;
                case ConnectivityType.Wimax:
                    return ConnectionProfile.WiMAX;
                case ConnectivityType.Wifi:
                    return ConnectionProfile.WiFi;
                case ConnectivityType.Bluetooth:
                    return ConnectionProfile.Bluetooth;
                case ConnectivityType.Mobile:
                case ConnectivityType.MobileDun:
                case ConnectivityType.MobileHipri:
                case ConnectivityType.MobileMms:
                    return ConnectionProfile.Cellular;
                case ConnectivityType.Dummy:
                    return ConnectionProfile.Other;
                default:
                    if (string.IsNullOrWhiteSpace(typeName))
                        return ConnectionProfile.Other;

                    var typeNameLower = typeName.ToLowerInvariant();
                    if (typeNameLower.Contains("mobile"))
                        return ConnectionProfile.Cellular;

                    if (typeNameLower.Contains("wifi"))
                        return ConnectionProfile.WiFi;

                    if (typeNameLower.Contains("wimax"))
                        return ConnectionProfile.WiMAX;

                    if (typeNameLower.Contains("ethernet"))
                        return ConnectionProfile.Ethernet;

                    if (typeNameLower.Contains("bluetooth"))
                        return ConnectionProfile.Bluetooth;

                    return ConnectionProfile.Other;
            }
        }
    }

    class ConnectivityBroadcastReceiver : BroadcastReceiver
    {
        Action onChanged;

        public ConnectivityBroadcastReceiver(Action onChanged) =>
            this.onChanged = onChanged;

        public override async void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != ConnectivityManager.ConnectivityAction)
                return;

            // await 500ms to ensure that the the connection manager updates
            await Task.Delay(500);
            onChanged?.Invoke();
        }
    }
}
