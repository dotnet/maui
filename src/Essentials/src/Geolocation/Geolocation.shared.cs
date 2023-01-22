#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

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

		/// <summary>
		/// Indicates if currently listening to location updates while the app is in foreground.
		/// </summary>
		bool IsListeningForeground { get; }

		/// <summary>
		/// Occurs while listening to location updates.
		/// </summary>
		event EventHandler<LocationEventArgs>? LocationChanged;

		/// <summary>
		/// Occurs when an error during listening for location updates arises. When getting the event,
		/// listening for further updates may have been stopped.
		/// </summary>
		event EventHandler<GeolocationErrorEventArgs>? LocationError;

		/// <summary>
		/// Starts listening to location updates using the <see cref="LocationChanged"/> event. Events
		/// may only sent when the app is in the foreground. Requests
		///  <see cref="Permissions.LocationWhenInUse"/> from the user.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="ArgumentNullException"/> if request is null.
		/// Will throw <see cref="FeatureNotSupportedException"/> if listening is not supported on this platform.
		/// Will throw <see cref="InvalidOperationException"/> if already listening; check
		/// <see cref="IsListeningForeground"/> to see if already listening.
		/// </remarks>
		/// <param name="request">The listening request parameters to use.</param>
		/// <returns><see langword="true"/> when listening was started, or <see langword="false"/> when listening couldn't be started.</returns>
		Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request);

		/// <summary>
		/// Stop listening for location updates when the app is in the foreground.
		/// </summary>
		void StopListeningForeground();
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

		/// <summary>
		/// Indicates if currently listening to location updates while the app is in foreground.
		/// </summary>
		public static bool IsListeningForeground { get => Current.IsListeningForeground; }

		/// <summary>
		/// Occurs while listening to location updates.
		/// </summary>
		public static event EventHandler<LocationEventArgs> LocationChanged
		{
			add => Current.LocationChanged += value;
			remove => Current.LocationChanged -= value;
		}

		/// <summary>
		/// Occurs when an error during listening for location updates arises.
		/// </summary>
		public static event EventHandler<GeolocationErrorEventArgs> LocationError
		{
			add => Current.LocationError += value;
			remove => Current.LocationError -= value;
		}

		/// <summary>
		/// Starts listening to location updates using the <see cref="LocationChanged"/> event. Events
		/// may only be sent when the app is in the foreground. Requests
		///  <see cref="Permissions.LocationWhenInUse"/> from the user.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if listening is not supported on this platform.
		/// Will throw <see cref="InvalidOperationException"/> if already listening; check
		/// <see cref="IsListeningForeground"/> to see if already listening.
		/// </remarks>
		/// <param name="request">The listening request parameters to use.</param>
		/// <returns><see langword="true"/> when listening was started, or <see langword="false"/> when listening couldn't be started.</returns>
		public static Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request) =>
			Current.StartListeningForegroundAsync(request);

		/// <summary>
		/// Stop listening for location updates when the app is in the foreground.
		/// </summary>
		public static void StopListeningForeground() =>
			Current.StopListeningForeground();

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

	partial class GeolocationImplementation : IGeolocation
	{
		public event EventHandler<LocationEventArgs>? LocationChanged;

		public event EventHandler<GeolocationErrorEventArgs>? LocationError;

		internal void OnLocationChanged(Location location) =>
			OnLocationChanged(new LocationEventArgs(location));

		internal void OnLocationChanged(LocationEventArgs e) =>
			LocationChanged?.Invoke(null, e);

		internal void OnLocationError(GeolocationError geolocationError) =>
			LocationError?.Invoke(null, new GeolocationErrorEventArgs(geolocationError));
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