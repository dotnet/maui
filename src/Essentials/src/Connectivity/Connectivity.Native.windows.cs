using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Networking
{
#if WINDOWS
	internal class ConnectivityNativeHelper
	{
		internal static string CNetworkListManagerCoClassGuid = "DCB00C01-570F-4A9B-8D69-199FDBA5723B";

		internal enum NLM_ENUM_NETWORK : int
		{
			NLM_ENUM_NETWORK_CONNECTED = 0x01,
			NLM_ENUM_NETWORK_DISCONNECTED = 0x02,
			NLM_ENUM_NETWORK_ALL = 0x03
		}

		internal enum NLM_NETWORK_CATEGORY
		{
			NLM_NETWORK_CATEGORY_PUBLIC = 0x00,
			NLM_NETWORK_CATEGORY_PRIVATE = 0x01,
			NLM_NETWORK_CATEGORY_DOMAIN_AUTHENTICATED = 0x02
		}

		[Flags]
		internal enum NLM_CONNECTIVITY
		{
			NLM_CONNECTIVITY_DISCONNECTED = 0,
			NLM_CONNECTIVITY_IPV4_NOTRAFFIC = 0x1,
			NLM_CONNECTIVITY_IPV6_NOTRAFFIC = 0x2,
			NLM_CONNECTIVITY_IPV4_SUBNET = 0x10,
			NLM_CONNECTIVITY_IPV4_LOCALNETWORK = 0x20,
			NLM_CONNECTIVITY_IPV4_INTERNET = 0x40,
			NLM_CONNECTIVITY_IPV6_SUBNET = 0x100,
			NLM_CONNECTIVITY_IPV6_LOCALNETWORK = 0x200,
			NLM_CONNECTIVITY_IPV6_INTERNET = 0x400
		}

		[ComImport]
		[Guid("DCB00000-570F-4A9B-8D69-199FDBA5723B")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface INetworkListManager
		{
			IEnumNetworks GetNetworks(NLM_ENUM_NETWORK flags);

			INetwork GetNetwork(Guid guid);

			IEnumNetworkConnections GetNetworkConnections();

			void GetNetworkConnection();

			bool IsConnectedToInternet { get; }

			bool IsConnected { get; }

			void GetConnectivity();
		}

		[ComImport]
		[Guid("DCB00003-570F-4A9B-8D69-199FDBA5723B")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface IEnumNetworks : IEnumerable
		{
		}

		[ComImport]
		[Guid("DCB00006-570F-4A9B-8D69-199FDBA5723B")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface IEnumNetworkConnections : IEnumerable<int>
		{
		}

		[ComImport]
		[Guid("DCB00002-570F-4A9B-8D69-199FDBA5723B")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface INetwork
		{
			/// <summary>
			/// Get the name of the network.
			/// </summary>
			/// <returns>The network name.</returns>
			string GetName();

			/// <summary>
			/// Rename this network. This change takes effect immediately.
			/// </summary>
			/// <param name="szNetworkNewName"></param>
			void SetName(string szNetworkNewName);

			/// <summary>
			/// Get the network description.
			/// </summary>
			/// <returns>Network description.</returns>
			string GetDescription();

			/// <summary>
			/// Set the network description. This change takes effect immediately.
			/// </summary>
			/// <param name="szDescription">The network description.</param>
			///         /// <returns></returns>
			void SetDescription(string szDescription);

			/// <summary>
			/// Get the network ID.
			/// </summary>
			/// <returns>The network Id.</returns>
			Guid GetNetworkId();

			/// <summary>
			/// Returns the domain type of a network.
			/// </summary>
			/// <returns>Domain Type</returns>
			Int32 GetDomainType();

			/// <summary>
			/// Returns an enumeration of all network connections for a network
			/// </summary>
			/// <returns>Network Enumeration</returns>
			IEnumNetworkConnections GetNetworkConnections();

			/// <summary>
			/// Returns the local date and time when the network was created and connected.
			/// </summary>
			/// <param name="pdwLowDateTimeCreated"></param>
			/// <param name="pdwHighDateTimeCreated"></param>
			/// <param name="pdwLowDateTimeConnected"></param>
			/// <param name="pdwHighDateTimeConnected"></param>
			void GetTimeCreatedAndConnected(
				out uint pdwLowDateTimeCreated,
				out uint pdwHighDateTimeCreated,
				out uint pdwLowDateTimeConnected,
				out uint pdwHighDateTimeConnected);

			/// <summary>
			/// Specifies if the network has internet connectivity.
			/// </summary>
			/// <returns></returns>
			bool IsConnectedToInternet();

			/// <summary>
			/// Specifies if the network has any network connectivity.
			/// </summary>
			/// <returns></returns>
			bool IsConnected();

			/// <summary>
			/// Returns the connectivity state of the network.
			/// </summary>
			/// <returns></returns>
			NLM_CONNECTIVITY GetConnectivity();

			/// <summary>
			/// Returns the category of a network.
			/// </summary>
			/// <returns></returns>
			NLM_NETWORK_CATEGORY GetCategory();

			void SetCategory(NLM_NETWORK_CATEGORY NewCategory);
		}

		internal static INetworkListManager GetNetworkListManager()
		{
			Type netProfMgrClass = Type.GetTypeFromCLSID(new Guid(CNetworkListManagerCoClassGuid));

#pragma warning disable IL2072
			return (INetworkListManager)Activator.CreateInstance(netProfMgrClass);
#pragma warning restore
		}
	}
#endif
}
