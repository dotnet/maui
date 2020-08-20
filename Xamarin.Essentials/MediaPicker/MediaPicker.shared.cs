using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        public static bool IsCaptureAvailable
            => PlatformIsCaptureAvailable;

        public static Task<FileResult> PickPhotoAsync(MediaPickerOptions options = null) =>
            PlatformPickPhotoAsync(options);

        public static Task<FileResult> CapturePhotoAsync(MediaPickerOptions options = null) =>
            PlatformCapturePhotoAsync(options);

        public static Task<FileResult> PickVideoAsync(MediaPickerOptions options = null) =>
            PlatformPickVideoAsync(options);

        public static Task<FileResult> CaptureVideoAsync(MediaPickerOptions options = null) =>
            PlatformCaptureVideoAsync(options);
    }

    public class MediaPickerOptions
    {
        public string Title { get; set; }
    }
}
