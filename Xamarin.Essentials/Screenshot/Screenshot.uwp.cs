using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformCanCapture => true;

        static async Task<ScreenshotFile> PlatformCaptureAsync()
        {
            var element = Window.Current.Content as FrameworkElement;
            var fileOnDisk = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(Path.ChangeExtension(Path.GetRandomFileName(), ".png"));
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element, (int)element.ActualWidth, (int)element.ActualHeight);
            var pixels = await renderTargetBitmap.GetPixelsAsync();

            var mediaFile = new ScreenshotFile(fileOnDisk.Path);

            using (var stream = await fileOnDisk.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                var bytes = pixels.ToArray();
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)element.ActualWidth,
                    (uint)element.ActualHeight,
                    96,
                    96,
                    bytes);

                await encoder.FlushAsync();
            }

            return mediaFile;
        }
    }
}
