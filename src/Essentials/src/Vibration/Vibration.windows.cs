using System;
using Windows.Devices.Haptics;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Devices
{
	partial class VibrationImplementation : IVibration
	{
		public bool IsSupported
			=> ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice") && DefaultDevice != null;

		static VibrationDevice DefaultDevice =>
			throw new NotImplementedException("WINUI"); //VibrationDevice.GetDefault();

		void PlatformVibrate()
			=> throw new NotImplementedException("WINUI");// DefaultDevice.Vibrate(duration);

		void PlatformVibrate(TimeSpan duration) =>
			throw new NotImplementedException("WINUI");// DefaultDevice.Vibrate(duration);

		void PlatformCancel() =>
			throw new NotImplementedException("WINUI");//DefaultDevice.Cancel();
	}
}
