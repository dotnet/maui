using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public partial class Screenshot
    {
        internal static bool PlatformCanCapture =>
            false;

        static Task<FileResult> PlatformCaptureAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
