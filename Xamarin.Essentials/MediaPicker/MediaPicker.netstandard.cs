using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static Task<MediaPickedEventArgs> PlatformShowPhotoPickerAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
