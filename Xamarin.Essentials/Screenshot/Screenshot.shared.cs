using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        public static bool IsCaptureAvailable
            => PlatformCanCapture;

        public static Task<FileResult> CaptureAsync()
            => PlatformCaptureAsync();
    }
}
