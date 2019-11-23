using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformCanCapture => UIScreen.MainScreen != null;

        static string GetTempFileName()
        {
            var tempFileName = Path.ChangeExtension(Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName()), ".png");
            return tempFileName;
        }

        static async Task<ScreenshotFile> PlatformCaptureAsync()
        {
            var img = UIScreen.MainScreen.Capture();
            var filePath = GetTempFileName();
            using var bytes = img.AsPNG().AsStream();
            using var fileStream = File.Create(filePath);
            await bytes.CopyToAsync(fileStream);
            var file = new ScreenshotFile(filePath);
            return file;
        }
    }
}
