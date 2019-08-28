using System.Text;
using System.Threading.Tasks;
using Tizen.NUI;
using Tizen.Security.SecureRepository;

namespace Xamarin.Essentials
{
    public partial class Screenshot
    {
        internal static bool PlatformCanCapture => false;

        static Task<ScreenshotFile> PlatformCaptureAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
