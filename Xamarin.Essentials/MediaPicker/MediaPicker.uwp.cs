using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static bool PlatformIsPhotoCaptureAvailable
            => false;

        static Task<MediaPickerResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => PickAsync(options, true);

        static Task<MediaPickerResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => PickAsync(options, false);

        static async Task<MediaPickerResult> PickAsync(MediaPickerOptions options, bool photo)
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
            return new MediaPickerResult(result);
        }

        static Task<MediaPickerResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => CaptureAsync(options, false);

        static Task<MediaPickerResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => CaptureAsync(options, false);

        static async Task<MediaPickerResult> CaptureAsync(MediaPickerOptions options, bool photo)
        {
            var captureUi = new CameraCaptureUI();

            if (photo)
                captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            else
                captureUi.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

            var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

            if (file != null)
                return new MediaPickerResult(file);

            return null;
        }
    }

    public partial class MediaPickerResult
    {
        internal MediaPickerResult(IStorageFile file)
           : this(file?.Path)
        {
            File = file;
            ContentType = file?.ContentType;
        }
    }
}
