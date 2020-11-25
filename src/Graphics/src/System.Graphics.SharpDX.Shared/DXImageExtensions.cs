using System.IO;
using SharpDX.Direct2D1;

namespace System.Graphics.SharpDX
{
    public static class DXImageExtensions
    {
        public static Bitmap AsBitmap(this IImage image)
        {
            if (image is DXImage dxImage)
            {
                return dxImage.NativeImage;
            }

            if (image is VirtualImage virtualImage)
            {
                using (var stream = new MemoryStream(virtualImage.Bytes))
                {
                    return DXGraphicsService.CurrentTarget.Value.LoadBitmap(stream);
                }
            }

            if (image != null)
            {
                Logger.Warn(
                    "DXImageExtensions.AsBitmap: Unable to get Bitmap from EWImage. Expected an image of type DXImage however an image of type {0} was received.",
                    image.GetType());
            }

            return null;
        }
    }
}