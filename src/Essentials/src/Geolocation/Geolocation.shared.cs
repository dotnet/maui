#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices.Sensors
{
	public interface IGeolocation
	{
		Task<Location?> GetLastKnownLocationAsync();

		Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken);
	}

	public static class Geolocation
	{
		static IGeolocation? defaultImplementation;

		public static IGeolocation Default =>
			defaultImplementation ??= new GeolocationImplementation();

		internal static void SetDefault(IGeolocation? implementation) =>
			defaultImplementation = implementation;
	}

	public static class GeolocationExtensions
	{
		public static Task<Location?> GetLocationAsync(this IGeolocation geolocation) =>
			geolocation.GetLocationAsync(new GeolocationRequest(), default);

		public static Task<Location?> GetLocationAsync(this IGeolocation geolocation, GeolocationRequest request) =>
			geolocation.GetLocationAsync(request ?? new GeolocationRequest(), default);
	}
}