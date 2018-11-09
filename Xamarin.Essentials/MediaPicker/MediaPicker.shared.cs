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

        public static event EventHandler<MediaPickedEventArgs> MediaPicked;

        static void OnMediaPicked(MediaPickedEventArgs e) =>
            MediaPicked?.Invoke(null, e);
    }

    public class MediaPickerOptions
    {
    }

    public class MediaPickedEventArgs : EventArgs
    {
        public MediaPickedEventArgs(bool canceled)
        {
            IsCanceled = canceled;
        }

        public MediaPickedEventArgs(MediaFile file)
        {
            File = file;
        }

        public bool IsCanceled { get; }

        public MediaFile File { get; }
    }

    public partial class MediaFile
    {
        public string FilePath { get; }

        public string FileName => Path.GetFileName(FilePath);

        public Task<Stream> OpenReadAsync() =>
            PlatformOpenReadAsync();
    }
}
