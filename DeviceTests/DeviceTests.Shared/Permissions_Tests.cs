using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Permissions_Tests
    {
        [Theory]
        [InlineData(PermissionType.Battery)]
        [InlineData(PermissionType.NetworkState)]
        [InlineData(PermissionType.LocationWhenInUse)]
        internal void Ensure_Declared(PermissionType permission)
        {
            Permissions.EnsureDeclared(permission);
        }

        [Theory]
        [InlineData(PermissionType.Battery, PermissionStatus.Granted)]
        [InlineData(PermissionType.NetworkState, PermissionStatus.Granted)]
        internal async Task Check_Status(PermissionType permission, PermissionStatus expectedStatus)
        {
            var status = await Permissions.CheckStatusAsync(permission);

            Assert.Equal(expectedStatus, status);
        }

        [Theory]
        [InlineData(PermissionType.Battery, PermissionStatus.Granted)]
        [InlineData(PermissionType.NetworkState, PermissionStatus.Granted)]
        internal async Task Request(PermissionType permission, PermissionStatus expectedStatus)
        {
            var status = await Permissions.CheckStatusAsync(permission);

            Assert.Equal(expectedStatus, status);
        }
    }

    public class A_Permissions_Tests
    {
        [Fact]
        public async Task Request_NotMainThread()
        {
            await Task.Run(async () =>
            {
                await Assert.ThrowsAsync<PermissionException>(() => Permissions.RequestAsync(PermissionType.LocationWhenInUse));
            });
        }
    }
}
