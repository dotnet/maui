using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static bool PlatformIsCaptureAvailable
            => false;

        static async Task<MediaPickerResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => new MediaPickerResult(await FilePicker.PickAsync(new PickOptions
            {
                 FileTypes = FilePickerFileType.Images
            }));

        static Task<MediaPickerResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => PlatformPickPhotoAsync(options);

        static async Task<MediaPickerResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => new MediaPickerResult(await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Videos
            }));

        static Task<MediaPickerResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => PlatformPickVideoAsync(options);
    }
}
