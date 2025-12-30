#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		CLLocationManager? listeningManager;

		/// <summary>
		/// Indicates if currently listening to location updates while the app is in foreground.
		/// </summary>
		public bool IsListeningForeground { get => listeningManager != null; }

		public bool IsEnabled => CLLocationManager.LocationServicesEnabled;

		public async Task<Location?> GetLastKnownLocationAsync()
		{
			if (!IsEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			var manager = new CLLocationManager();
			var location = manager.Location;

			var reducedAccuracy = false;
#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				reducedAccuracy = manager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy;
			}
#endif
			return location?.ToLocation(reducedAccuracy);
		}

		public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (!IsEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			// the location manager requires an active run loop
			// so just use the main loop
			var manager = MainThread.InvokeOnMainThread(() => new CLLocationManager());

			var tcs = new TaskCompletionSource<CLLocation?>(manager);

			var listener = new SingleLocationListener();
			listener.LocationHandler += HandleLocation;

			cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
			cancellationToken.Register(Cancel);

			manager.DesiredAccuracy = request.PlatformDesiredAccuracy;
			manager.Delegate = listener;

#if __IOS__
			// we're only listening for a single update
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			manager.PausesLocationUpdatesAutomatically = false;
#pragma warning restore CA1416
#endif

			manager.StartUpdatingLocation();

			var reducedAccuracy = false;
#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				if (request.RequestFullAccuracy && manager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy)
				{
					await manager.RequestTemporaryFullAccuracyAuthorizationAsync("TemporaryFullAccuracyUsageDescription");
				}

				reducedAccuracy = manager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy;
			}
#endif

			var clLocation = await tcs.Task;

			return clLocation?.ToLocation(reducedAccuracy);

			void HandleLocation(CLLocation location)
			{
				manager.StopUpdatingLocation();
				tcs.TrySetResult(location);
			}

			void Cancel()
			{
				manager.StopUpdatingLocation();
				tcs.TrySetResult(null);
			}
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
		public async Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (IsListeningForeground)
				throw new InvalidOperationException("Already listening to location changes.");

			if (!IsEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			// the location manager requires an active run loop
			// so just use the main loop
			listeningManager = MainThread.InvokeOnMainThread(() => new CLLocationManager());

			var reducedAccuracy = false;
#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				reducedAccuracy = listeningManager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy;
			}
#endif

			var listener = new ContinuousLocationListener();
			listener.LocationHandler += HandleLocation;
			listener.ErrorHandler += HandleError;

			listeningManager.DesiredAccuracy = request.PlatformDesiredAccuracy;
			listeningManager.Delegate = listener;

#if __IOS__
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			// allow pausing updates
			listeningManager.PausesLocationUpdatesAutomatically = true;
#pragma warning restore CA1416
#endif

			listeningManager.StartUpdatingLocation();

			return true;

			void HandleLocation(CLLocation clLocation)
			{
				OnLocationChanged(clLocation.ToLocation(reducedAccuracy));
			}

			void HandleError(GeolocationError error)
			{
				StopListeningForeground();
				OnLocationError(error);
			}
		}

		/// <summary>
		/// Stop listening for location updates when the app is in the foreground.
		/// Has no effect when not listening and <see cref="Geolocation.IsListeningForeground"/>
		/// is currently <see langword="false"/>.
		/// </summary>
		public void StopListeningForeground()
		{
			if (!IsListeningForeground ||
				listeningManager is null)
				return;

			listeningManager.StopUpdatingLocation();

			if (listeningManager.Delegate is ContinuousLocationListener listener)
			{
				listener.LocationHandler = null;
				listener.ErrorHandler = null;
			}

			listeningManager.WeakDelegate = null;

			listeningManager = null;
		}
	}

	class SingleLocationListener : CLLocationManagerDelegate
	{
		bool wasRaised = false;

		internal Action<CLLocation>? LocationHandler { get; set; }

		/// <inheritdoc/>
		public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
		{
			if (wasRaised)
				return;

			wasRaised = true;

			var location = locations?.LastOrDefault();

			if (location == null)
				return;

			LocationHandler?.Invoke(location);
		}

		/// <inheritdoc/>
		public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => false;
	}

	class ContinuousLocationListener : CLLocationManagerDelegate
	{
		internal Action<CLLocation>? LocationHandler { get; set; }

		internal Action<GeolocationError>? ErrorHandler { get; set; }

		/// <inheritdoc/>
		public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
		{
			var location = locations?.LastOrDefault();

			if (location == null)
				return;

			LocationHandler?.Invoke(location);
		}

		/// <inheritdoc/>
		public override void Failed(CLLocationManager manager, NSError error)
		{
			if ((CLError)error.Code == CLError.Network)
				ErrorHandler?.Invoke(GeolocationError.PositionUnavailable);
		}

		/// <inheritdoc/>
		public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
		{
			if (status == CLAuthorizationStatus.Denied ||
				status == CLAuthorizationStatus.Restricted)
				ErrorHandler?.Invoke(GeolocationError.Unauthorized);
		}

		/// <inheritdoc/>
		public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => false;
	}
}
