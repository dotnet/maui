using System;
using Windows.Devices.Haptics;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Essentials
{
	public static partial class Vibration
	{
		internal static bool IsSupported
			=> ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice") && DefaultDevice != null;

		static VibrationDevice DefaultDevice =>
			throw new NotImplementedException("WINUI"); //VibrationDevice.GetDefault();

		static void PlatformVibrate(TimeSpan duration) =>
			throw new NotImplementedException("WINUI");// DefaultDevice.Vibrate(duration);

		static void PlatformCancel() =>
			throw new NotImplementedException("WINUI");//DefaultDevice.Cancel();
	}
}
