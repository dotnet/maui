using System.Threading.Tasks;
using AVFoundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		public Task TurnOnAsync()
		{
			Toggle(true);

			return Task.CompletedTask;
		}

		public Task TurnOffAsync()
		{
			Toggle(false);

			return Task.CompletedTask;
		}

		void Toggle(bool on)
		{
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
#pragma warning disable CA1422 // Validate platform compatibility
			var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
			if (captureDevice == null || !(captureDevice.HasFlash || captureDevice.HasTorch))
				throw new FeatureNotSupportedException();

			captureDevice.LockForConfiguration(out var error);

			if (error == null)
			{
				if (on)
				{
					if (captureDevice.HasTorch)
						captureDevice.SetTorchModeLevel(AVCaptureDevice.MaxAvailableTorchLevel, out var torchErr);
					if (captureDevice.HasFlash)
						captureDevice.FlashMode = AVCaptureFlashMode.On;
				}
				else
				{
					if (captureDevice.HasTorch)
						captureDevice.TorchMode = AVCaptureTorchMode.Off;
					if (captureDevice.HasFlash)
						captureDevice.FlashMode = AVCaptureFlashMode.Off;
				}
			}

			captureDevice.UnlockForConfiguration();

#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
			captureDevice.Dispose();
			captureDevice = null;
		}
	}
}
