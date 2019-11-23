using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WatchKit;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformCanCapture => false; // On WatchOs 3 this appears to be possible if activated through settings.

        static Task<ScreenshotFile> PlatformCaptureAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
