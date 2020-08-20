using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        static bool PlatformCanCapture =>
            Platform.WindowManager.DefaultDisplay?.Flags.HasFlag(DisplayFlags.Secure) == false;

        static Task<ScreenshotResult> PlatformCaptureAsync()
        {
            var view = Platform.GetCurrentActivity(true)?.Window?.DecorView?.RootView;
            if (view == null)
                throw new NullReferenceException("Unable to find the main window.");

            var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);

            using (var canvas = new Canvas(bitmap))
            {
                var drawable = view.Background;
                if (drawable != null)
                    drawable.Draw(canvas);
                else
                    canvas.DrawColor(Color.White);

                view.Draw(canvas);
            }

            var result = new ScreenshotResult(bitmap);

            return Task.FromResult(result);
        }
    }

    public partial class ScreenshotResult
    {
        readonly Bitmap bmp;

        internal ScreenshotResult(Bitmap bmp)
            : base()
        {
            this.bmp = bmp;

            Width = bmp.Width;
            Height = bmp.Height;
        }

        internal async Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format)
        {
            var stream = new MemoryStream();

            var f = format switch
            {
                ScreenshotFormat.Jpeg => Bitmap.CompressFormat.Jpeg,
                _ => Bitmap.CompressFormat.Png,
            };

            await bmp.CompressAsync(f, 100, stream).ConfigureAwait(false);
            stream.Position = 0;

            return stream;
        }
    }
}
