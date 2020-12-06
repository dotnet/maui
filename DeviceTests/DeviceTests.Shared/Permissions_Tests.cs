using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Permissions_Tests
    {
        [Theory]
        [InlineData("Battery")]
        [InlineData("NetworkState")]
        [InlineData("LocationWhenInUse")]
        internal void Ensure_Declared(string permission)
        {
            switch (permission)
            {
                case "Battery":
                    Permissions.EnsureDeclared<Permissions.Battery>();
                    break;
                case "NetworkState":
                    Permissions.EnsureDeclared<Permissions.NetworkState>();
                    break;
                case "LocationWhenInUse":
                    Permissions.EnsureDeclared<Permissions.LocationWhenInUse>();
                    break;
            }
        }

        [Theory]
        [InlineData("Battery", PermissionStatus.Granted)]
        [InlineData("NetworkState", PermissionStatus.Granted)]
        internal async Task Check_Status(string permission, PermissionStatus expectedStatus)
        {
            var status = PermissionStatus.Unknown;
            switch (permission)
            {
                case "Battery":
                    status = await Permissions.CheckStatusAsync<Permissions.Battery>();
                    break;
                case "NetworkState":
                    status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();
                    break;
            }

            Assert.Equal(expectedStatus, status);
        }

        [Theory]
        [InlineData("Battery", PermissionStatus.Granted)]
        [InlineData("NetworkState", PermissionStatus.Granted)]
        internal async Task Request(string permission, PermissionStatus expectedStatus)
        {
            var status = PermissionStatus.Unknown;
            switch (permission)
            {
                case "Battery":
                    status = await Permissions.RequestAsync<Permissions.Battery>();
                    break;
                case "NetworkState":
                    status = await Permissions.RequestAsync<Permissions.NetworkState>();
                    break;
            }

            Assert.Equal(expectedStatus, status);
        }

        [Fact]
        [Trait(Traits.UI, Traits.FeatureSupport.Supported)]
        public async Task Request_NotMainThread()
        {
            await Task.Run(async () =>
            {
                await Assert.ThrowsAsync<PermissionException>(async () => await Permissions.RequestAsync<Permissions.LocationWhenInUse>());
            });
        }
    }
}
