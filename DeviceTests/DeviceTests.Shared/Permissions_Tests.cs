using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

#if __ANDROID__
[assembly: Android.App.UsesPermission(Android.Manifest.Permission.BatteryStats)]
#endif

namespace DeviceTests
{
    public class Permissions_Tests
    {
        [Theory]
        [InlineData(PermissionType.Battery)]
        [InlineData(PermissionType.NetworkState)]
        internal void Ensure_Declared(PermissionType permission)
        {
            Permissions.EnsureDeclared(permission);
        }

        [Theory]
        [InlineData(PermissionType.LocationWhenInUse)]
        internal void Ensure_Declared_Throws(PermissionType permission)
        {
            if (DeviceInfo.Platform == DeviceInfo.Platforms.UWP)
            {
                return;
            }

            Assert.Throws<PermissionException>(() => Permissions.EnsureDeclared(permission));
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
        [InlineData(PermissionType.LocationWhenInUse)]
        internal Task Check_Status_Throws(PermissionType permission)
        {
            if (DeviceInfo.Platform == DeviceInfo.Platforms.UWP)
            {
                return Task.CompletedTask;
            }

            return Assert.ThrowsAsync<PermissionException>(async () => await Permissions.CheckStatusAsync(permission));
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
}
