using System.Drawing;
using System.IO;

namespace Microsoft.Maui.Graphics.GDI
{
    public static class GDIImageExtensions
    {
        public static Bitmap AsBitmap(this IImage image)
        {
            if (image is GDIImage dxImage)
                return dxImage.NativeImage;

            if (image is VirtualImage virtualImage)
                using (var stream = new MemoryStream(virtualImage.Bytes))
                    return new Bitmap(stream);

            if (image != null)
            {
                Logger.Warn(
                    "GDIImageExtensions.AsBitmap: Unable to get Bitmap from Image. Expected an image of type GDIImage however an image of type {0} was received.",
                    image.GetType());
            }

            return null;
        }
    }
}
