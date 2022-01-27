using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public static partial class Geolocation
	{
		static Task<Location> PlatformLastKnownLocationAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
