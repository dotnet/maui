using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - a human needs to accept permissions on Android
	//   - the camera flash is not always available
	[Category("Flashlight")]
	public class Flashlight_Tests
	{
		[UITheory]
		[InlineData(true)]
		[InlineData(false)]
#if __ANDROID__
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
#endif
		[Trait(Traits.Hardware.Flash, Traits.FeatureSupport.Supported)]
		public async Task Turn_On_Off(bool oldCameraApi)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasFlash)
				return;

#if __ANDROID__
			(Flashlight.Default as FlashlightImplementation).AlwaysUseCameraApi = oldCameraApi;
#else
			Utils.Unused(oldCameraApi);
#endif

				await Flashlight.TurnOnAsync();
				await Flashlight.TurnOffAsync();
		}
	}
}
