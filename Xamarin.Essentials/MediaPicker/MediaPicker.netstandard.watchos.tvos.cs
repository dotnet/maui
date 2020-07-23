using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static Task<MediaPickerResult> PlatformShowPhotoPickerAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
