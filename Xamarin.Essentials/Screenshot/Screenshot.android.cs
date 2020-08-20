using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Util;
using Android.Views;

using Path = System.IO.Path;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        static bool PlatformCanCapture =>
            Platform.WindowManager.DefaultDisplay?.Flags.HasFlag(DisplayFlags.Secure) == false;

        static async Task<FileResult> PlatformCaptureAsync()
        {
            var view = Platform.GetCurrentActivity(true)?.Window?.DecorView?.RootView;
            if (view == null)
                throw new NullReferenceException("Unable to find the main window.");

            var path = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString() + ".png");

            using (var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888))
            using (var canvas = new Canvas(bitmap))
            {
                var drawable = view.Background;
                if (drawable != null)
                    drawable.Draw(canvas);
                else
                    canvas.DrawColor(Color.White);

                view.Draw(canvas);

                using var stream = File.Create(path);
                var success = await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                if (!success)
                    throw new AndroidException("Unable to save screenshot file.");
            }

            return new FileResult(path);
        }
    }
}
