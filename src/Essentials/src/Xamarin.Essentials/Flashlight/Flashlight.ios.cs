using System.Threading.Tasks;
using AVFoundation;

namespace Xamarin.Essentials
{
    public static partial class Flashlight
    {
        static Task PlatformTurnOnAsync()
        {
            Toggle(true);

            return Task.CompletedTask;
        }

        static Task PlatformTurnOffAsync()
        {
            Toggle(false);

            return Task.CompletedTask;
        }

        static void Toggle(bool on)
        {
            var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);
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
