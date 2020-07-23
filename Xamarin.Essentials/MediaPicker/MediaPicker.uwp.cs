using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static readonly string[] defaultVideoFileTypes = new[] { ".mp4", ".wmv", ".avi" };
        static readonly string[] defaultImageFileTypes = new[] { ".jpeg", ".jpg", ".png", ".gif", ".bmp" };

        static async Task<MediaPickerResult> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
        {
            var picker = new FileOpenPicker();

            // set picker properties
            foreach (var filter in defaultImageFileTypes)
                picker.FileTypeFilter.Add(filter);
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;

            // show the picker
            var result = await picker.PickSingleFileAsync();

            // cancelled
            if (result == null)
                return null;

            // picked
            return new MediaPickerResult(result);
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
