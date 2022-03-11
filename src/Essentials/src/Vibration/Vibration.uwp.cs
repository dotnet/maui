using System;
using Windows.Devices.Haptics;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class VibrationImplementation : IVibration
	{
		public bool IsSupported
			=> ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice") && DefaultDevice != null;

		static VibrationDevice DefaultDevice =>
			throw new NotImplementedException("WINUI"); //VibrationDevice.GetDefault();

		public void Vibrate() 
			=> throw new NotImplementedException("WINUI");// DefaultDevice.Vibrate(duration);

		public void Vibrate(double duration) 
			=> throw new NotImplementedException("WINUI");// DefaultDevice.Vibrate(duration);

		public void Vibrate(TimeSpan duration) =>
			throw new NotImplementedException("WINUI");// DefaultDevice.Vibrate(duration);

		public void Cancel() =>
			throw new NotImplementedException("WINUI");//DefaultDevice.Cancel();
	}
}
