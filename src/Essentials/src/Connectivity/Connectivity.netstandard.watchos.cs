using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Networking
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	partial class ConnectivityImplementation : IConnectivity
	{
		public NetworkAccess NetworkAccess =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public IEnumerable<ConnectionProfile> ConnectionProfiles =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StartListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
