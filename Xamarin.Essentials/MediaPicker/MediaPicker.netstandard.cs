using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static Task<MediaFile> PlatformShowPhotoPickerAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class MediaFile
    {
        Task<Stream> PlatformOpenReadAsync() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
