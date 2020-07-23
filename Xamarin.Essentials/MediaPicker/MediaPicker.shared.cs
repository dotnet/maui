using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        public static Task<MediaPickerResult> ShowPhotoPickerAsync() =>
            PlatformShowPhotoPickerAsync(null);

        public static Task<MediaPickerResult> ShowPhotoPickerAsync(MediaPickerOptions options) =>
            PlatformShowPhotoPickerAsync(options);
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
