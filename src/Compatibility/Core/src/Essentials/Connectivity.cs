#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Networking;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	public static partial class Connectivity
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='NetworkAccess']/Docs" />
		public static NetworkAccess NetworkAccess => Current.NetworkAccess;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='ConnectionProfiles']/Docs" />
		public static IEnumerable<ConnectionProfile> ConnectionProfiles => Current.ConnectionProfiles.Distinct();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='ConnectivityChanged']/Docs" />
		public static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add => Current.ConnectivityChanged += value;
			remove => Current.ConnectivityChanged -= value;
		}

		static IConnectivity Current => Networking.Connectivity.Current;
	}
}
