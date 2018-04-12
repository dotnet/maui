using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    // TEST NOTES:
    //   - a human needs to accept permissions on all systems
    public class Geolocation_Tests
    {
        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task Get_LastKnownLocation_Is_Something()
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            Assert.NotNull(location);

            Assert.True(location.Accuracy > 0);
            Assert.NotEqual(0.0, location.Latitude);
            Assert.NotEqual(0.0, location.Longitude);

            Assert.NotEqual(DateTimeOffset.MaxValue, location.TimestampUtc);
            Assert.NotEqual(DateTimeOffset.MinValue, location.TimestampUtc);

            // before right now, but after yesterday
            Assert.True(location.TimestampUtc < DateTimeOffset.UtcNow);
            Assert.True(location.TimestampUtc > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));
        }

        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task Get_Location_Is_Something()
        {
            var location = await Geolocation.GetLocationAsync();

            Assert.NotNull(location);

            Assert.True(location.Accuracy > 0);
            Assert.NotEqual(0.0, location.Latitude);
            Assert.NotEqual(0.0, location.Longitude);

            Assert.NotEqual(DateTimeOffset.MaxValue, location.TimestampUtc);
            Assert.NotEqual(DateTimeOffset.MinValue, location.TimestampUtc);

            // before right now, but after yesterday
            Assert.True(location.TimestampUtc < DateTimeOffset.UtcNow);
            Assert.True(location.TimestampUtc > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));
        }

        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task Get_Location_With_Request_Is_Something()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            Assert.NotNull(location);

            Assert.True(location.Accuracy > 0);
            Assert.NotEqual(0.0, location.Latitude);
            Assert.NotEqual(0.0, location.Longitude);

            Assert.NotEqual(DateTimeOffset.MaxValue, location.TimestampUtc);
            Assert.NotEqual(DateTimeOffset.MinValue, location.TimestampUtc);

            // before right now, but after yesterday
            Assert.True(location.TimestampUtc < DateTimeOffset.UtcNow);
            Assert.True(location.TimestampUtc > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));
        }
    }
}
