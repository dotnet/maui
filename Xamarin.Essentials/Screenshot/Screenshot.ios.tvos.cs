using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformCanCapture =>
            UIScreen.MainScreen != null;

        static Task<FileResult> PlatformCaptureAsync()
        {
            var img = UIScreen.MainScreen.Capture();
            var result = new UIImageFileResult(img);

            return Task.FromResult<FileResult>(result);
        }
    }
}
