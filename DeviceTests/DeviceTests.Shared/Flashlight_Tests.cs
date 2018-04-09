using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    // TEST NOTES:
    //   - a human needs to accept permissions on Android
    //   - the camera flash is not emulated on iOS simulators
    public class Flashlight_Tests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
#if __ANDROID__
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
#endif
#if __IOS__
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
#endif
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
            Flashlight.AlwaysUseCameraApi = oldCameraApi;
#endif
            await Flashlight.TurnOnAsync();
            await Flashlight.TurnOffAsync();
        }
    }
}
