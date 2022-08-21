using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Tizen.Location;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		Location lastKnownLocation = new Location();

		public bool IsListening { get => false; }

		public Task<Location> GetLastKnownLocationAsync() => Task.FromResult(lastKnownLocation);

		public async Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
		{
			_ = request ?? throw new ArgumentNullException(nameof(request));

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			Locator service = null;
			var gps = PlatformUtils.GetFeatureInfo<bool>("location.gps");
			var wps = PlatformUtils.GetFeatureInfo<bool>("location.wps");
			if (gps)
			{
				if (wps)
					service = new Locator(LocationType.Hybrid);
				else
					service = new Locator(LocationType.Gps);
			}
			else
			{
				if (wps)
					service = new Locator(LocationType.Wps);
				else
					service = new Locator(LocationType.Passive);
			}

			var tcs = new TaskCompletionSource<bool>();

			cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
			cancellationToken.Register(() =>
			{
				service?.Stop();
				tcs.TrySetResult(false);
			});

			double KmToMetersPerSecond(double km) => km * 0.277778;
			service.LocationChanged += (s, e) =>
			{
				if (e.Location != null)
				{
					lastKnownLocation.Accuracy = e.Location.Accuracy;
					lastKnownLocation.Altitude = e.Location.Altitude;
					lastKnownLocation.Course = e.Location.Direction;
					lastKnownLocation.Latitude = e.Location.Latitude;
					lastKnownLocation.Longitude = e.Location.Longitude;
					lastKnownLocation.Speed = KmToMetersPerSecond(e.Location.Speed);
					lastKnownLocation.Timestamp = e.Location.Timestamp;
				}
				service?.Stop();
				tcs.TrySetResult(true);
			};
			service.Start();

			await tcs.Task;

			return lastKnownLocation;
		}

		public Task<bool> StartListeningForegroundAsync(ListeningRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<bool> StopListeningForegroundAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
