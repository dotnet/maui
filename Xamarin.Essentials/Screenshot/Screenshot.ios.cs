using System;
using System.IO;
using System.Threading.Tasks;
using CoreTelephony;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformCanCapture => UIScreen.MainScreen != null;

        static string GetTempFileName()
        {
            var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var libFolder = Path.Combine(docFolder, "..", "Library");
            var tempFileName = Path.ChangeExtension(Path.Combine(libFolder, Path.GetTempFileName()), ".png");
            return tempFileName;
        }

        static Task<MediaFile> PlatformCaptureAsync()
        {
            var img = UIScreen.MainScreen.Capture();
            var bytes = img.AsPNG().ToArray();
            var file = new MediaFile(GetTempFileName());
            File.WriteAllBytes(file.Filepath, bytes);
            return Task.FromResult(file);
        }
    }
}
