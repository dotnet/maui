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

    public partial class MediaFile : FileBase
    {
        public MediaFile(string fullPath)
            : base(fullPath)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.MediaPicker);
        }

        public MediaFile(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.MediaPicker);
        }

        public MediaFile(FileBase file)
            : base(file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.MediaPicker);
        }
    }
}
