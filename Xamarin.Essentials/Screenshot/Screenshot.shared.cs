using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        public static bool CanCapture => PlatformCanCapture;

        public static Task<MediaFile> CaptureAsync()
            => PlatformCaptureAsync();
    }
}
