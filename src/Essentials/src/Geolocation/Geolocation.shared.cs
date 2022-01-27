using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public static partial class Geolocation
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLastKnownLocationAsync']/Docs" />
		public static Task<Location> GetLastKnownLocationAsync() =>
			PlatformLastKnownLocationAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][0]/Docs" />
		public static Task<Location> GetLocationAsync() =>
			PlatformLocationAsync(new GeolocationRequest(), default);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][1]/Docs" />
		public static Task<Location> GetLocationAsync(GeolocationRequest request) =>
			PlatformLocationAsync(request ?? new GeolocationRequest(), default);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][2]/Docs" />
		public static Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
			PlatformLocationAsync(request ?? new GeolocationRequest(), cancelToken);
	}
}
