using System.Threading.Tasks;
using AVFoundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		public Task<bool> IsSupportedAsync()
		{
			var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
			bool isSupported = captureDevice is not null &&
				(captureDevice.HasFlash || captureDevice.HasTorch);
			return Task.FromResult(isSupported);
		}

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
