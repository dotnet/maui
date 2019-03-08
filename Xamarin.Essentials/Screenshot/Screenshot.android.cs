using System;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Java.Util;
using Environment = Android.OS.Environment;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        static bool PlatformCanCapture => !Platform.WindowManager.DefaultDisplay.Flags.HasFlag(DisplayFlags.Secure);

        static string GetTempFileName() => Path.Combine(Environment.ExternalStorageDirectory.Path, Path.GetTempFileName());

        static async Task<MediaFile> PlatformCaptureAsync()
        {
            var path = GetTempFileName();

            var view = Platform.GetCurrentActivity(false)?.FindViewById(Resource.Id.Content).RootView;
            view.DrawingCacheEnabled = true;
            var bitmap = Bitmap.CreateBitmap(view.GetDrawingCache(true));
            using (var stream = File.Create(path))
            {
                var success = await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                if (!success)
                    throw new AndroidException("Failure to compress bitmap to file!");
            }
            return new MediaFile(path);
        }
    }
}
