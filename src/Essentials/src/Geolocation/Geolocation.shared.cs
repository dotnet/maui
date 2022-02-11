using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;


namespace Microsoft.Maui.Essentials
{
	public interface IGeolocation
	{
		Task<Location> LastKnownLocationAsync();

		Task<Location> LocationAsync();

		Task<Location> LocationAsync(GeolocationRequest request);

		Task<Location> LocationAsync(GeolocationRequest request, CancellationToken cancelToken);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public static partial class Geolocation
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLastKnownLocationAsync']/Docs" />
		public static Task<Location> GetLastKnownLocationAsync() =>
			Current.LastKnownLocationAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][0]/Docs" />
		public static Task<Location> GetLocationAsync() =>
			Current.LocationAsync(new GeolocationRequest(), default);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][1]/Docs" />
		public static Task<Location> GetLocationAsync(GeolocationRequest request) =>
			Current.LocationAsync(request ?? new GeolocationRequest(), default);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][2]/Docs" />
		public static Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
			Current.LocationAsync(request ?? new GeolocationRequest(), cancelToken);

#nullable enable
		static IGeolocation? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IGeolocation Current =>
			currentImplementation ??= new GeolocationImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IGeolocation? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
