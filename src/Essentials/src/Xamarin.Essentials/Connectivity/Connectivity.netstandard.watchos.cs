using System.Collections.Generic;

namespace Xamarin.Essentials
{
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
