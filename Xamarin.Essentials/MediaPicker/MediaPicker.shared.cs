using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        public static Task<MediaPickedEventArgs> ShowPhotoPickerAsync() =>
            PlatformShowPhotoPickerAsync(null);

        public static Task<MediaPickedEventArgs> ShowPhotoPickerAsync(MediaPickerOptions options) =>
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

        public MediaPickedEventArgs(string path)
        {
            Path = path;
        }

        public bool IsCanceled { get; }

        public string Path { get; }
    }
}
