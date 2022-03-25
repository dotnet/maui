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
			captureDevice.Dispose();
			captureDevice = null;
		}
	}
}
