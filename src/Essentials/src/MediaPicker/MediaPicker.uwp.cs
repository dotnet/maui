using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage.Pickers;

namespace Microsoft.Maui.Essentials
{
    public static partial class MediaPicker
    {
        static bool PlatformIsCaptureSupported
            => true;

        static Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => PickAsync(options, true);

        static Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => PickAsync(options, false);

        static async Task<FileResult> PickAsync(MediaPickerOptions options, bool photo)
        {
            var picker = new FileOpenPicker();

            var defaultTypes = photo ? FilePickerFileType.Images.Value : FilePickerFileType.Videos.Value;

            // set picker properties
            foreach (var filter in defaultTypes.Select(t => t.TrimStart('*')))
                picker.FileTypeFilter.Add(filter);
            picker.SuggestedStartLocation = photo ? PickerLocationId.PicturesLibrary : PickerLocationId.VideosLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;

            // show the picker
            var result = await picker.PickSingleFileAsync();

            // cancelled
            if (result == null)
                return null;

            // picked
            return new FileResult(result);
        }

        static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => CaptureAsync(options, true);

        static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => CaptureAsync(options, false);

        static async Task<FileResult> CaptureAsync(MediaPickerOptions options, bool photo)
        {
            var captureUi = new CameraCaptureUI();

            if (photo)
                captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            else
                captureUi.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

            var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

            if (file != null)
                return new FileResult(file);

            return null;
        }
    }
}
