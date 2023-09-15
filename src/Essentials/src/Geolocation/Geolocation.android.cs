#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using AndroidLocation = Android.Locations.Location;
using LocationPower = Android.Locations.Power;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		const long twoMinutes = 120000;
		static readonly string[] ignoredProviders = new string[] { LocationManager.PassiveProvider, "local_database" };

		static ContinuousLocationListener? continuousListener;
		static List<string>? listeningProviders;

		static LocationManager? locationManager;

		static LocationManager? LocationManager =>
			locationManager ??= Application.Context.GetSystemService(Context.LocationService) as LocationManager;

		/// <summary>
		/// Indicates if currently listening to location updates while the app is in foreground.
		/// </summary>
		public bool IsListeningForeground { get => continuousListener is not null; }

		public async Task<Location?> GetLastKnownLocationAsync()
		{
			if (LocationManager is null)
				throw new FeatureNotSupportedException("Android LocationManager is not available");

			await Permissions.EnsureGrantedOrRestrictedAsync<Permissions.LocationWhenInUse>();

			AndroidLocation? bestLocation = null;

			foreach (var provider in LocationManager.GetProviders(true))
			{
				var location = LocationManager.GetLastKnownLocation(provider);

				if (location is not null && IsBetterLocation(location, bestLocation))
					bestLocation = location;
			}

			return bestLocation?.ToLocation();
		}

		public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (LocationManager is null)
				throw new FeatureNotSupportedException("Android LocationManager is not available");

			await Permissions.EnsureGrantedOrRestrictedAsync<Permissions.LocationWhenInUse>();

			var enabledProviders = LocationManager.GetProviders(true);
			var hasProviders = enabledProviders.Any(p => !ignoredProviders.Contains(p));

			if (!hasProviders)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			// get the best possible provider for the requested accuracy
			var providerInfo = GetBestProvider(LocationManager, request.DesiredAccuracy);

			// if no providers exist, we can't get a location
			// let's punt and try to get the last known location
			if (string.IsNullOrEmpty(providerInfo.Provider))
				return await GetLastKnownLocationAsync();

			var tcs = new TaskCompletionSource<AndroidLocation?>();

			var allProviders = LocationManager.GetProviders(false);

			var providers = new List<string>();
			if (allProviders.Contains(Android.Locations.LocationManager.GpsProvider))
				providers.Add(Android.Locations.LocationManager.GpsProvider);
			if (allProviders.Contains(Android.Locations.LocationManager.NetworkProvider))
				providers.Add(Android.Locations.LocationManager.NetworkProvider);

			if (providers.Count == 0)
				providers.Add(providerInfo.Provider);

			var listener = new SingleLocationListener(LocationManager, providerInfo.Accuracy, providers);
			listener.LocationHandler = HandleLocation;

			cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
			cancellationToken.Register(Cancel);

			// start getting location updates
			// make sure to use a thread with a looper
			var looper = Looper.MyLooper() ?? Looper.MainLooper;

			foreach (var provider in providers)
				LocationManager.RequestLocationUpdates(provider, 0, 0, listener, looper);

			var androidLocation = await tcs.Task;

			if (androidLocation is null)
				return null;

			return androidLocation.ToLocation();

			void HandleLocation(AndroidLocation location)
			{
				RemoveUpdates();
				tcs.TrySetResult(location);
			}

			void Cancel()
			{
				RemoveUpdates();
				tcs.TrySetResult(listener?.BestLocation);
			}

			void RemoveUpdates()
			{
				if (LocationManager is null)
					return;

				for (var i = 0; i < providers.Count; i++)
					LocationManager.RemoveUpdates(listener);
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

			if (LocationManager is null)
				throw new FeatureNotSupportedException("Android LocationManager is not available");

			if (IsListeningForeground)
				throw new InvalidOperationException("Already listening to location changes.");

			await Permissions.EnsureGrantedOrRestrictedAsync<Permissions.LocationWhenInUse>();

			var enabledProviders = LocationManager.GetProviders(true);
			var hasProviders = enabledProviders is not null &&
				enabledProviders.Any(p => !ignoredProviders.Contains(p));

			if (!hasProviders)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			// get the best possible provider for the requested accuracy
			var providerInfo = GetBestProvider(LocationManager, request.DesiredAccuracy);

			// if no providers exist, we can't listen for locations
			if (string.IsNullOrEmpty(providerInfo.Provider))
				return false;

			var allProviders = LocationManager.GetProviders(false);

			listeningProviders = new List<string>();
			if (allProviders.Contains(Android.Locations.LocationManager.GpsProvider))
				listeningProviders.Add(Android.Locations.LocationManager.GpsProvider);
			if (allProviders.Contains(Android.Locations.LocationManager.NetworkProvider))
				listeningProviders.Add(Android.Locations.LocationManager.NetworkProvider);

			if (listeningProviders.Count == 0)
				listeningProviders.Add(providerInfo.Provider);

			var continuousListener = new ContinuousLocationListener(LocationManager, providerInfo.Accuracy, listeningProviders);
			continuousListener.LocationHandler = HandleLocation;
			continuousListener.ErrorHandler = HandleError;

			// start getting location updates
			// make sure to use a thread with a looper
			var looper = Looper.MyLooper() ?? Looper.MainLooper;

			var minTimeMilliseconds = (long)request.MinimumTime.TotalMilliseconds;

			foreach (var provider in listeningProviders)
				LocationManager.RequestLocationUpdates(provider, minTimeMilliseconds, providerInfo.Accuracy, continuousListener, looper);

			return true;

			void HandleLocation(AndroidLocation location)
			{
				OnLocationChanged(location.ToLocation());
			}

			void HandleError(GeolocationError geolocationError)
			{
				StopListeningForeground();
				OnLocationError(geolocationError);
			}
		}

		/// <summary>
		/// Stop listening for location updates when the app is in the foreground.
		/// Has no effect when not listening and <see cref="Geolocation.IsListeningForeground"/>
		/// is currently <see langword="false"/>.
		/// </summary>
		public void StopListeningForeground()
		{
			if (continuousListener is null)
				return;

			continuousListener.LocationHandler = null;
			continuousListener.ErrorHandler = null;

			if (listeningProviders is null ||
				LocationManager is null)
				return;

			for (var i = 0; i < listeningProviders.Count; i++)
			{
				LocationManager.RemoveUpdates(continuousListener);
			}

			continuousListener = null;
		}

// TODO: android.location.Criteria deprecated in API 34
// https://developer.android.com/reference/android/location/Criteria
#pragma warning disable CA1422
		static (string? Provider, float Accuracy) GetBestProvider(LocationManager locationManager, GeolocationAccuracy accuracy)
		{
			// Criteria: https://developer.android.com/reference/android/location/Criteria

			var criteria = new Criteria
			{
				BearingRequired = false,
				AltitudeRequired = false,
				SpeedRequired = false
			};

			var accuracyDistance = 100;

			switch (accuracy)
			{
				case GeolocationAccuracy.Lowest:
					criteria.Accuracy = Accuracy.NoRequirement;
					criteria.HorizontalAccuracy = Accuracy.NoRequirement;
					criteria.PowerRequirement = LocationPower.NoRequirement;
					accuracyDistance = 500;
					break;
				case GeolocationAccuracy.Low:
					criteria.Accuracy = Accuracy.Coarse;
					criteria.HorizontalAccuracy = Accuracy.Low;
					criteria.PowerRequirement = LocationPower.Low;
					accuracyDistance = 500;
					break;
				case GeolocationAccuracy.Default:
				case GeolocationAccuracy.Medium:
					criteria.Accuracy = Accuracy.Coarse;
					criteria.HorizontalAccuracy = Accuracy.Medium;
					criteria.PowerRequirement = LocationPower.Medium;
					accuracyDistance = 250;
					break;
				case GeolocationAccuracy.High:
					criteria.Accuracy = Accuracy.Fine;
					criteria.HorizontalAccuracy = Accuracy.High;
					criteria.PowerRequirement = LocationPower.High;
					accuracyDistance = 100;
					break;
				case GeolocationAccuracy.Best:
					criteria.Accuracy = Accuracy.Fine;
					criteria.HorizontalAccuracy = Accuracy.High;
					criteria.PowerRequirement = LocationPower.High;
					accuracyDistance = 50;
					break;
			}

			var provider = locationManager.GetBestProvider(criteria, true) ?? locationManager.GetProviders(true).FirstOrDefault();

			return (provider, accuracyDistance);
		}
#pragma warning restore CA1422

		internal static bool IsBetterLocation(AndroidLocation location, AndroidLocation? bestLocation)
		{
			if (bestLocation is null)
				return true;

			var timeDelta = location.Time - bestLocation.Time;

			var isSignificantlyNewer = timeDelta > twoMinutes;
			var isSignificantlyOlder = timeDelta < -twoMinutes;
			var isNewer = timeDelta > 0;

			if (isSignificantlyNewer)
				return true;

			if (isSignificantlyOlder)
				return false;

			var accuracyDelta = (int)(location.Accuracy - bestLocation.Accuracy);
			var isLessAccurate = accuracyDelta > 0;
			var isMoreAccurate = accuracyDelta < 0;
			var isSignificantlyLessAccurage = accuracyDelta > 200;

			var isFromSameProvider = location?.Provider?.Equals(bestLocation?.Provider, StringComparison.OrdinalIgnoreCase) ?? false;

			if (isMoreAccurate)
				return true;

			if (isNewer && !isLessAccurate)
				return true;

			if (isNewer && !isSignificantlyLessAccurage && isFromSameProvider)
				return true;

			return false;
		}
	}

	class SingleLocationListener : Java.Lang.Object, ILocationListener
	{
		readonly object locationSync = new object();

		float desiredAccuracy;

		internal AndroidLocation? BestLocation { get; set; }

		HashSet<string> activeProviders = new HashSet<string>();

		bool wasRaised = false;

		internal Action<AndroidLocation>? LocationHandler { get; set; }

		internal SingleLocationListener(LocationManager? manager, float desiredAccuracy, IEnumerable<string> activeProviders)
		{
			this.desiredAccuracy = desiredAccuracy;

			this.activeProviders = new HashSet<string>(activeProviders);

			foreach (var provider in activeProviders)
			{
				var location = manager?.GetLastKnownLocation(provider);
				if (location is not null && GeolocationImplementation.IsBetterLocation(location, BestLocation))
					BestLocation = location;
			}
		}

		void ILocationListener.OnLocationChanged(AndroidLocation location)
		{
			if (location.Accuracy <= desiredAccuracy)
			{
				if (wasRaised)
					return;

				wasRaised = true;

				LocationHandler?.Invoke(location);
				return;
			}

			lock (locationSync)
			{
				if (GeolocationImplementation.IsBetterLocation(location, BestLocation))
					BestLocation = location;
			}
		}

		void ILocationListener.OnProviderDisabled(string provider)
		{
			lock (activeProviders)
				activeProviders.Remove(provider);
		}

		void ILocationListener.OnProviderEnabled(string provider)
		{
			lock (activeProviders)
				activeProviders.Add(provider);
		}

		void ILocationListener.OnStatusChanged(string? provider, Availability status, Bundle? extras)
		{
			if (provider is null)
				return;

			switch (status)
			{
				case Availability.Available:
					((ILocationListener)this).OnProviderEnabled(provider);
					break;

				case Availability.OutOfService:
					((ILocationListener)this).OnProviderDisabled(provider);
					break;
			}
		}
	}

	class ContinuousLocationListener : Java.Lang.Object, ILocationListener
	{
		float desiredAccuracy;

		HashSet<string> activeProviders = new HashSet<string>();

		internal Action<AndroidLocation>? LocationHandler { get; set; }

		internal Action<GeolocationError>? ErrorHandler { get; set; }

		internal ContinuousLocationListener(LocationManager? manager, float desiredAccuracy, IEnumerable<string> providers)
		{
			this.desiredAccuracy = desiredAccuracy;

			foreach (var provider in providers)
			{
				if (manager is not null && manager.IsProviderEnabled(provider))
					activeProviders.Add(provider);
			}
		}

		void ILocationListener.OnLocationChanged(AndroidLocation location)
		{
			if (location.Accuracy <= desiredAccuracy)
			{
				LocationHandler?.Invoke(location);
				return;
			}
		}

		void ILocationListener.OnProviderDisabled(string provider)
		{
			lock (activeProviders)
			{
				if (activeProviders.Remove(provider) &&
					activeProviders.Count == 0)
					ErrorHandler?.Invoke(GeolocationError.PositionUnavailable);
			}
		}

		void ILocationListener.OnProviderEnabled(string provider)
		{
			lock (activeProviders)
				activeProviders.Add(provider);
		}

		void ILocationListener.OnStatusChanged(string? provider, Availability status, Bundle? extras)
		{
			if (provider is null)
				return;

			switch (status)
			{
				case Availability.Available:
					((ILocationListener)this).OnProviderEnabled(provider);
					break;

				case Availability.OutOfService:
					((ILocationListener)this).OnProviderDisabled(provider);
					break;
			}
		}
	}
}
