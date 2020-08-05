using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static bool PlatformIsPhotoCaptureAvailable =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<MediaPickerResult> PlatformPickPhotoAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<MediaPickerResult> PlatformCapturePhotoAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<MediaPickerResult> PlatformPickVideoAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<MediaPickerResult> PlatformCaptureVideoAsync(MediaPickerOptions options) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
