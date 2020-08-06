using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        public static bool IsCaptureAvailable
            => PlatformIsCaptureAvailable;

        public static Task<MediaPickerResult> PickPhotoAsync(MediaPickerOptions options = null) =>
            PlatformPickPhotoAsync(options);

        public static Task<MediaPickerResult> CapturePhotoAsync(MediaPickerOptions options = null) =>
            PlatformCapturePhotoAsync(options);

        public static Task<MediaPickerResult> PickVideoAsync(MediaPickerOptions options = null) =>
            PlatformPickVideoAsync(options);

        public static Task<MediaPickerResult> CaptureVideoAsync(MediaPickerOptions options = null) =>
            PlatformCaptureVideoAsync(options);
    }

    public class MediaPickerOptions
    {
        public string Title { get; set; }
    }

    public partial class MediaPickerResult : FileBase
    {
        public MediaPickerResult(string fullPath)
            : base(fullPath)
        {
        }

        public MediaPickerResult(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
        }

        public MediaPickerResult(FileBase file)
            : base(file)
        {
        }
    }
}
