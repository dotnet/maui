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
		event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;

		/// <summary>
		/// Occurs when an error during listening for location updates arises. When the event is
		/// fired, listening for further location updates has been stopped and no further
		/// <see cref="LocationChanged"/> events are sent.
		/// </summary>
		event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;

		/// <summary>
		/// Starts listening to location updates using the <see cref="LocationChanged"/> event. Events
		/// may only sent when the app is in the foreground. Requests
		/// <see cref="Permissions.LocationWhenInUse"/> from the user.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
		/// <exception cref="FeatureNotSupportedException">Thrown if listening is not supported on this platform.</exception>
		/// <exception cref="InvalidOperationException">Thrown if already listening and <see cref="IsListeningForeground"/> returns <see langword="true"/>.</exception>
		/// <param name="request">The listening request parameters to use.</param>
		/// <returns><see langword="true"/> when listening was started, or <see langword="false"/> when listening couldn't be started.</returns>
		Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request);

		/// <summary>
		/// Stop listening for location updates when the app is in the foreground.
		/// Has no effect when <see cref="IsListeningForeground"/> is currently <see langword="false"/>.
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
		public static event EventHandler<GeolocationLocationChangedEventArgs> LocationChanged
		{
			add => Current.LocationChanged += value;
			remove => Current.LocationChanged -= value;
		}

		/// <summary>
		/// Occurs when an error during listening for location updates arises. When the event is
		/// fired, listening for further location updates has been stopped and no further
		/// <see cref="LocationChanged"/> events are sent.
		/// </summary>
		public static event EventHandler<GeolocationListeningFailedEventArgs> ListeningFailed
		{
			add => Current.ListeningFailed += value;
			remove => Current.ListeningFailed -= value;
		}

		/// <summary>
		/// Starts listening to location updates using the <see cref="Geolocation.LocationChanged"/>
		/// event or the <see cref="Geolocation.ListeningFailed"/> event. Events may only sent when
		/// the app is in the foreground. Requests <see cref="Permissions.LocationWhenInUse"/>
		/// from the user.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
		/// <exception cref="FeatureNotSupportedException">Thrown if listening is not supported on this platform.</exception>
		/// <exception cref="InvalidOperationException">Thrown if already listening and <see cref="IsListeningForeground"/> returns <see langword="true"/>.</exception>
		/// <param name="request">The listening request parameters to use.</param>
		/// <returns><see langword="true"/> when listening was started, or <see langword="false"/> when listening couldn't be started.</returns>
		public static Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request) =>
			Current.StartListeningForegroundAsync(request);

		/// <summary>
		/// Stop listening for location updates when the app is in the foreground.
		/// Has no effect when not listening and <see cref="Geolocation.IsListeningForeground"/>
		/// is currently <see langword="false"/>.
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
		public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;

		public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;

		internal void OnLocationChanged(Location location) =>
			OnLocationChanged(new GeolocationLocationChangedEventArgs(location));

		internal void OnLocationChanged(GeolocationLocationChangedEventArgs e) =>
			LocationChanged?.Invoke(null, e);

		internal void OnLocationError(GeolocationError geolocationError) =>
			ListeningFailed?.Invoke(null, new GeolocationListeningFailedEventArgs(geolocationError));
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