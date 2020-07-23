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
        public string Title { get; set; }
    }

    public partial class MediaFile : FileBase
    {
        public MediaFile(string fullPath)
            : base(fullPath)
        {
        }

        public MediaFile(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
        }

        public MediaFile(FileBase file)
            : base(file)
        {
        }
    }
}
