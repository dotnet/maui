using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - a human needs to accept permissions on all systems
	[Category("Geolocation")]
	public class Geolocation_Tests
	{
		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task Get_LastKnownLocation_Is_Something()
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
			});

			var location = await Geolocation.GetLastKnownLocationAsync().ConfigureAwait(false);

			Assert.NotNull(location);

			Assert.True(location.Accuracy > 0);
			Assert.NotEqual(0.0, location.Latitude);
			Assert.NotEqual(0.0, location.Longitude);

			Assert.NotEqual(DateTimeOffset.MaxValue, location.Timestamp);
			Assert.NotEqual(DateTimeOffset.MinValue, location.Timestamp);

			// before right now, but after yesterday
			Assert.True(location.Timestamp < DateTimeOffset.UtcNow);
			Assert.True(location.Timestamp > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task Get_Location_Is_Something()
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
			});

			var location = await Geolocation.GetLocationAsync().ConfigureAwait(false);

			Assert.NotNull(location);

			Assert.True(location.Accuracy > 0);
			Assert.NotEqual(0.0, location.Latitude);
			Assert.NotEqual(0.0, location.Longitude);

			Assert.NotEqual(DateTimeOffset.MaxValue, location.Timestamp);
			Assert.NotEqual(DateTimeOffset.MinValue, location.Timestamp);

			// before right now, but after yesterday
			Assert.True(location.Timestamp < DateTimeOffset.UtcNow);
			Assert.True(location.Timestamp > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task Get_Location_With_Request_Is_Something()
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
			});

			var request = new GeolocationRequest(GeolocationAccuracy.Best);
			request.RequestFullAccuracy = true;
			var location = await Geolocation.GetLocationAsync(request).ConfigureAwait(false);

			Assert.NotNull(location);

			Assert.True(location.Accuracy > 0);
			Assert.False(location.ReducedAccuracy);
			Assert.NotEqual(0.0, location.Latitude);
			Assert.NotEqual(0.0, location.Longitude);

			Assert.NotEqual(DateTimeOffset.MaxValue, location.Timestamp);
			Assert.NotEqual(DateTimeOffset.MinValue, location.Timestamp);

			// before right now, but after yesterday
			Assert.True(location.Timestamp < DateTimeOffset.UtcNow);
			Assert.True(location.Timestamp > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task Geolocation_IsListeningForeground()
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
			});

			var request = new GeolocationListeningRequest(GeolocationAccuracy.Best);
			request.DesiredAccuracy = GeolocationAccuracy.Low;
			request.MinimumTime = TimeSpan.FromSeconds(5);

			bool hasServiceStarted = await Geolocation.StartListeningForegroundAsync(request);

			bool isListeningForeground = Geolocation.IsListeningForeground;

			Assert.Equal(hasServiceStarted, isListeningForeground);
		}
	}
}
