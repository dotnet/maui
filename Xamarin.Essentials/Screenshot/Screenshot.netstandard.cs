using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        static bool PlatformCanCapture =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<ScreenshotFile> PlatformCaptureAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
