using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Flashlight_Tests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Turn_On_Off(bool oldCameraApi)
        {
            if (DeviceInfo.Platform == DeviceInfo.Platforms.UWP)
            {
                await Utils.OnMainThread(async () =>
                {
                    await Assert.ThrowsAsync<FeatureNotSupportedException>(() => Flashlight.TurnOnAsync());
                });
                return;
            }
#if __ANDROID__
            // API 23+ we need user interaction for camera permission
            // can't really test so easily on device.
            if (Platform.HasApiLevel(Android.OS.BuildVersionCodes.M))
                return;

            Flashlight.AlwaysUseCameraApi = oldCameraApi;
#elif __IOS__
            // TODO: remove this as soon as the test harness can filter
            // the iOS simulator does not emulate a flashlight
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
                return;
#endif
            await Flashlight.TurnOnAsync();
            await Flashlight.TurnOffAsync();
        }
    }
}
