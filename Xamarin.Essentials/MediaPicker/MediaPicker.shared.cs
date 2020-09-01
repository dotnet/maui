using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        public static bool IsCaptureSupported
            => PlatformIsCaptureSupported;

        public static Task<FileResult> PickPhotoAsync(MediaPickerOptions options = null) =>
            PlatformPickPhotoAsync(options);

        public static Task<FileResult> CapturePhotoAsync(MediaPickerOptions options = null)
        {
            if (!IsCaptureSupported)
                throw new FeatureNotSupportedException();

            return PlatformCapturePhotoAsync(options);
        }

        public static Task<FileResult> PickVideoAsync(MediaPickerOptions options = null) =>
            PlatformPickVideoAsync(options);

        public static Task<FileResult> CaptureVideoAsync(MediaPickerOptions options = null)
        {
            if (!IsCaptureSupported)
                throw new FeatureNotSupportedException();

            return PlatformCaptureVideoAsync(options);
        }
    }

    public class MediaPickerOptions
    {
        public string Title { get; set; }
    }
}
