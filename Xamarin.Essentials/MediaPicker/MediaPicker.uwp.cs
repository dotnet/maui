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

        static async Task<MediaFile> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
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
            {
                OnMediaPicked(new MediaPickedEventArgs(true));
                return null;
            }

            // picked
            var mediaFile = new MediaFile(result);
            OnMediaPicked(new MediaPickedEventArgs(mediaFile));
            return mediaFile;
        }
    }

    public partial class MediaFile
    {
        readonly IStorageFile file;

        public MediaFile(IStorageFile file)
        {
            this.file = file;

            FilePath = file.Path;
        }

        Task<Stream> PlatformOpenReadAsync() =>
            file.OpenStreamForReadAsync();
    }
}
