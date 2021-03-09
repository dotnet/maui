using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Geolocation
	{
		public static Task<Location> GetLastKnownLocationAsync() =>
			PlatformLastKnownLocationAsync();

		public static Task<Location> GetLocationAsync() =>
			PlatformLocationAsync(new GeolocationRequest(), default);

		public static Task<Location> GetLocationAsync(GeolocationRequest request) =>
			PlatformLocationAsync(request ?? new GeolocationRequest(), default);

		public static Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
			PlatformLocationAsync(request ?? new GeolocationRequest(), cancelToken);
	}
}
