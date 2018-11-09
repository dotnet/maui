using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        public static Task<MediaFile> ShowPhotoPickerAsync() =>
            PlatformShowPhotoPickerAsync(null);

        public static Task<MediaFile> ShowPhotoPickerAsync(MediaPickerOptions options) =>
            PlatformShowPhotoPickerAsync(options);
    }

    public class MediaPickerOptions
    {
    }

    public partial class MediaFile
    {
        public string FilePath { get; }

        public string FileName => Path.GetFileName(FilePath);

        public Task<Stream> OpenReadAsync() =>
            PlatformOpenReadAsync();
    }
}
