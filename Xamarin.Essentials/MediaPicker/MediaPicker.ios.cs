using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static Task<MediaFile> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
        {
            return Task.FromResult<MediaFile>(null);
        }
    }

    public partial class MediaFile
    {
        public MediaFile(string file)
        {
            FilePath = file;
        }

        Task<Stream> PlatformOpenReadAsync() =>
            Task.FromResult((Stream)File.OpenRead(FilePath));
    }
}
