using System;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Vibration")]
	public class Vibration_Tests
	{
		[Fact
#if WINDOWS
			(Skip = "Not supported on Windows? See Vibration implementation")
#endif
			]
		public void Vibrate()
		{
#if __ANDROID__
            // API 23+ we need user interaction for camera permission
            // can't really test so easily on device.
            if (OperatingSystem.IsAndroidVersionAtLeast(23))
                return;
#elif __IOS__
			// TODO: remove this as soon as the test harness can filter
			// the iOS simulator does not emulate a flashlight
			if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DevicePlatform.iOS)
				return;
#endif

			Vibration.Vibrate();
		}

		[Fact
#if WINDOWS
			(Skip = "Not supported on Windows? See Vibration implementation")
#endif
			]
		public void Vibrate_Cancel()
		{
#if __ANDROID__
            // API 23+ we need user interaction for camera permission
            // can't really test so easily on device.
            if (OperatingSystem.IsAndroidVersionAtLeast(23))
                return;
#elif __IOS__
			// TODO: remove this as soon as the test harness can filter
			// the iOS simulator does not emulate a flashlight
			if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DevicePlatform.iOS)
				return;
#endif

			Vibration.Vibrate();
			Vibration.Cancel();
		}
	}
}
