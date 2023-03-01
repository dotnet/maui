using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Networking
{
	partial class ConnectivityImplementation : IConnectivity
	{
		void StartListeners()
        {
            NetworkChange.NetworkAddressChanged += NetworkStatusChanged;
            NetworkChange.NetworkAvailabilityChanged += NetworkStatusChanged;
        }

        void NetworkStatusChanged(object sender, EventArgs e) => OnConnectivityChanged();

        void StopListeners()
        {
            NetworkChange.NetworkAddressChanged -= NetworkStatusChanged;
            NetworkChange.NetworkAvailabilityChanged -= NetworkStatusChanged;
        }

        public NetworkAccess NetworkAccess
        {
            get
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                    return NetworkAccess.None;

                var allLoopback = NetworkInterface.GetAllNetworkInterfaces()
                    .All(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Loopback);
                if (allLoopback)
                    return NetworkAccess.None;

                var gateways = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(ni => ni.GetIPProperties().GatewayAddresses);

                if (gateways.Any())
                    return NetworkAccess.Internet;

                return NetworkAccess.None;
            }
        }

        ConnectionProfile ToConnectionType(NetworkInterface networkInterface)
        {
            switch (networkInterface.NetworkInterfaceType)
            {
                case NetworkInterfaceType.TokenRing:
                case NetworkInterfaceType.Ethernet:
                case NetworkInterfaceType.Ethernet3Megabit:
                case NetworkInterfaceType.IPOverAtm:
                case NetworkInterfaceType.FastEthernetT:
                case NetworkInterfaceType.GigabitEthernet:
                case NetworkInterfaceType.FastEthernetFx:
                case NetworkInterfaceType.GenericModem:
                    return ConnectionProfile.Ethernet;

                case NetworkInterfaceType.Wireless80211:
                case NetworkInterfaceType.Wman:
                    return ConnectionProfile.WiFi;

                case NetworkInterfaceType.Wwanpp2:
                case NetworkInterfaceType.Wwanpp:
                    return ConnectionProfile.Cellular;

                case NetworkInterfaceType.Ppp:
                case NetworkInterfaceType.Fddi:
                case NetworkInterfaceType.Isdn:
                case NetworkInterfaceType.BasicIsdn:
                case NetworkInterfaceType.PrimaryIsdn:
                case NetworkInterfaceType.Tunnel:
                case NetworkInterfaceType.VeryHighSpeedDsl:
                case NetworkInterfaceType.AsymmetricDsl:
                case NetworkInterfaceType.RateAdaptDsl:
                case NetworkInterfaceType.SymmetricDsl:
                case NetworkInterfaceType.Loopback:
                case NetworkInterfaceType.Slip:
                case NetworkInterfaceType.Atm:
                case NetworkInterfaceType.MultiRateSymmetricDsl:
                case NetworkInterfaceType.HighPerformanceSerialBus:
                case NetworkInterfaceType.Unknown:
                    return ConnectionProfile.Unknown;

                default:
                    return ConnectionProfile.Unknown;
            }
        }

        public IEnumerable<ConnectionProfile> ConnectionProfiles
        {
            get => NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback).Select(ToConnectionType);
        }		

	}
}