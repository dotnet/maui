using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        static bool PlatformCanCapture =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<MediaFile> PlatformCaptureAsync() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
