#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Windows.Devices.Geolocation;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		Geolocator? listeningGeolocator;

		public bool IsListeningForeground { get => false; }

		public async Task<Location?> GetLastKnownLocationAsync()
		{
			// no need for permissions as AllowFallbackToConsentlessPositions
			// will allow the device to return a location regardless

			var geolocator = new Geolocator
			{
				DesiredAccuracy = PositionAccuracy.Default,
			};
			geolocator.AllowFallbackToConsentlessPositions();

			var location = await geolocator.GetGeopositionAsync().AsTask();

			return location?.Coordinate?.ToLocation();
		}

		public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(request);

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			var geolocator = new Geolocator
			{
				DesiredAccuracyInMeters = request.PlatformDesiredAccuracy
			};

			CheckStatus(geolocator.LocationStatus);

			cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);

			var location = await geolocator.GetGeopositionAsync().AsTask(cancellationToken);

			return location?.Coordinate?.ToLocation();
		}

		static void CheckStatus(PositionStatus status)
		{
			switch (status)
			{
				case PositionStatus.Disabled:
				case PositionStatus.NotAvailable:
					throw new FeatureNotEnabledException("Location services are not enabled on device.");
			}
		}

		public async Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (request.MinimumTime.TotalMilliseconds < 0)
				throw new ArgumentOutOfRangeException(nameof(request), "MinimumTime must be positive.");

			if (IsListeningForeground)
				throw new InvalidOperationException("Already listening to location updates.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			listeningGeolocator = new Geolocator
			{
				DesiredAccuracyInMeters = request.PlatformDesiredAccuracy,
				ReportInterval = (uint)request.MinimumTime.TotalMilliseconds,
				MovementThreshold = request.PlatformDesiredAccuracy,
			};

			CheckStatus(listeningGeolocator.LocationStatus);

			listeningGeolocator.PositionChanged += OnLocatorPositionChanged;
			listeningGeolocator.StatusChanged += OnLocatorStatusChanged;

			return true;
		}

		public void StopListeningForeground()
		{
			if (!IsListeningForeground || listeningGeolocator == null)
				return;

			listeningGeolocator.PositionChanged -= OnLocatorPositionChanged;
			listeningGeolocator.StatusChanged -= OnLocatorStatusChanged;

			listeningGeolocator = null;
		}

		void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs e) =>
			OnLocationChanged(e.Position.ToLocation());

		void OnLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs e)
		{
			if (!IsListeningForeground)
				return;

			StopListeningForeground();

			GeolocationError error;
			switch (e.Status)
			{
				case PositionStatus.Disabled:
					error = GeolocationError.Unauthorized;
					break;

				case PositionStatus.NoData:
					error = GeolocationError.PositionUnavailable;
					break;

				default:
					return;
			}

			OnLocationError(error);
		}
	}
}
