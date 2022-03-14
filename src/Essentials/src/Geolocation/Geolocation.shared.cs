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
		Task<Location> GetLastKnownLocationAsync();

		Task<Location> GetLocationAsync();

		Task<Location> GetLocationAsync(GeolocationRequest request);

		Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public static partial class Geolocation
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLastKnownLocationAsync']/Docs" />
		public static Task<Location> GetLastKnownLocationAsync() =>
			Current.GetLastKnownLocationAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][1]/Docs" />
		public static Task<Location> GetLocationAsync() =>
			Current.GetLocationAsync(new GeolocationRequest(), default);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][2]/Docs" />
		public static Task<Location> GetLocationAsync(GeolocationRequest request) =>
			Current.GetLocationAsync(request ?? new GeolocationRequest(), default);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][3]/Docs" />
		public static Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
			Current.GetLocationAsync(request ?? new GeolocationRequest(), cancelToken);

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

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GeolocationImplementation : IGeolocation
	{
		public Task<Location> GetLocationAsync()
			=> GetLocationAsync(new GeolocationRequest(), default);

		public Task<Location> GetLocationAsync(GeolocationRequest request)
			=> GetLocationAsync(request ?? new GeolocationRequest(), default);
	}
}