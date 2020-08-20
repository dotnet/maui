using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformCanCapture =>
            true;

        static async Task<FileResult> PlatformCaptureAsync()
        {
            var element = Window.Current?.Content as FrameworkElement;
            if (element == null)
                throw new InvalidOperationException("Unable to find main window content.");

            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            var fileOnDisk = await cacheFolder.CreateFileAsync(Guid.NewGuid().ToString() + ".png");

            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element, (int)element.ActualWidth, (int)element.ActualHeight);
            var pixels = await renderTargetBitmap.GetPixelsAsync();
            var bytes = pixels.ToArray();

            using (var stream = await fileOnDisk.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)element.ActualWidth, (uint)element.ActualHeight, 96, 96, bytes);

                await encoder.FlushAsync();
            }

            return new FileResult(fileOnDisk);
        }
    }
}
