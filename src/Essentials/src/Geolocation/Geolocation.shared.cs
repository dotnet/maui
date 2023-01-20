#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Provides a way to get the current location of the device.
	/// </summary>
	public interface IGeolocation
	{
		/// <summary>
		/// Returns the last known location of the device.
		/// </summary>
		/// <returns>A <see cref="Location"/> object containing recent location information or <see langword="null"/> if no location is known.</returns>
		/// <remarks>
		/// <para>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</para>
		/// <para>This location may be a recently cached location.</para>
		/// </remarks>
		Task<Location?> GetLastKnownLocationAsync();

		/// <summary>
		/// Returns the current location of the device.
		/// </summary>
		/// <param name="request">The criteria to use when determining the location of the device.</param>
		/// <param name="cancelToken">A token that can be used for cancelling the operation.</param>
		/// <remarks>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</remarks>
		/// <returns>A <see cref="Location"/> object containing current location information or <see langword="null"/> if no location could be determined.</returns>
		Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken);
	}

	/// <summary>
	/// Provides a way to get the current location of the device.
	/// </summary>
	public static partial class Geolocation
	{
		/// <summary>
		/// Returns the last known location of the device.
		/// </summary>
		/// <returns>A <see cref="Location"/> object containing recent location information or <see langword="null"/> if no location is known.</returns>
		/// <remarks>
		/// <para>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</para>
		/// <para>This location may be a recently cached location.</para>
		/// </remarks>
		public static Task<Location?> GetLastKnownLocationAsync() =>
			Current.GetLastKnownLocationAsync();

		/// <summary>
		/// Returns the current location of the device.
		/// </summary>
		/// <returns>A <see cref="Location"/> object containing current location information or <see langword="null"/> if no location could be determined.</returns>
		/// <remarks>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</remarks>
		public static Task<Location?> GetLocationAsync() =>
			Current.GetLocationAsync();

		/// <summary>
		/// Returns the current location of the device.
		/// </summary>
		/// <param name="request">The criteria to use when determining the location of the device.</param>
		/// <returns>A <see cref="Location"/> object containing current location information or <see langword="null"/> if no location could be determined.</returns>
		/// <remarks>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</remarks>
		public static Task<Location?> GetLocationAsync(GeolocationRequest request) =>
			Current.GetLocationAsync(request);

		/// <summary>
		/// Returns the current location of the device.
		/// </summary>
		/// <param name="request">The criteria to use when determining the location of the device.</param>
		/// <param name="cancelToken">A token that can be used for cancelling the operation.</param>
		/// <returns>A <see cref="Location"/> object containing current location information or <see langword="null"/> if no location could be determined.</returns>
		/// <remarks>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</remarks>
		public static Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
			Current.GetLocationAsync(request, cancelToken);

		static IGeolocation Current => Devices.Sensors.Geolocation.Default;

		static IGeolocation? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IGeolocation Default =>
			defaultImplementation ??= new GeolocationImplementation();

		internal static void SetDefault(IGeolocation? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="IGeolocation"/> APIs.
	/// </summary>
	public static class GeolocationExtensions
	{
		/// <summary>
		/// Returns the current location of the device.
		/// </summary>
		/// <param name="geolocation">The object this method is invoked on.</param>
		/// <returns>A <see cref="Location"/> object containing current location information or <see langword="null"/> if no location could be determined.</returns>
		/// <remarks>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</remarks>
		public static Task<Location?> GetLocationAsync(this IGeolocation geolocation) =>
			geolocation.GetLocationAsync(new GeolocationRequest(), default);

		/// <summary>
		/// Returns the current location of the device.
		/// </summary>
		/// <param name="geolocation">The object this method is invoked on.</param>
		/// <param name="request">The criteria to use when determining the location of the device.</param>
		/// <returns>A <see cref="Location"/> object containing current location information or <see langword="null"/> if no location could be determined.</returns>
		/// <remarks>The location permissions will be requested at runtime if needed. You might still need to declare something in your app manifest.</remarks>
		public static Task<Location?> GetLocationAsync(this IGeolocation geolocation, GeolocationRequest request) =>
			geolocation.GetLocationAsync(request ?? new GeolocationRequest(), default);
	}
}