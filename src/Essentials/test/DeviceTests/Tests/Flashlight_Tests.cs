using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - a human needs to accept permissions on Android
	//   - the camera flash is not always available
	public class Flashlight_Tests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
#if __ANDROID__
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
#endif
		[Trait(Traits.Hardware.Flash, Traits.FeatureSupport.Supported)]
		public Task Turn_On_Off(bool oldCameraApi)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasFlash)
				return Task.CompletedTask;

#if __ANDROID__
            Flashlight.AlwaysUseCameraApi = oldCameraApi;
#else
			Utils.Unused(oldCameraApi);
#endif

			return Utils.OnMainThread(async () =>
			{
				await Flashlight.TurnOnAsync();
				await Flashlight.TurnOffAsync();
			});
		}
	}
}
