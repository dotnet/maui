using System.Collections.Generic;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	public static partial class Connectivity
	{
		static NetworkAccess PlatformNetworkAccess =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static IEnumerable<ConnectionProfile> PlatformConnectionProfiles =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StartListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StopListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
