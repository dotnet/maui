using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        static bool PlatformCanCapture =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<ScreenshotFile> PlatformCaptureAsync() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
