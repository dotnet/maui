using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
    public static class ImageExtensions
    {
        public static byte[] AsBytes(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
        {
            if (target == null)
                return null;

            using (var stream = new MemoryStream())
            {
                target.Save(stream, format, quality);
                return stream.ToArray();
            }
        }

        public static Stream AsStream(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
        {
            if (target == null)
                return null;

            var stream = new MemoryStream();
            target.Save(stream, format, quality);
            stream.Position = 0;

            return stream;
        }

        public static async Task<byte[]> AsBytesAsync(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
        {
            if (target == null)
                return null;

            using (var stream = new MemoryStream())
            {
                await target.SaveAsync(stream, format, quality);
                return stream.ToArray();
            }
        }
        
        public static string AsBase64(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
        {
            if (target == null)
                return null;

            var bytes = target.AsBytes(format, quality);
            return Convert.ToBase64String(bytes);
        }

        public static Paint AsPaint(this IImage target)
        {
            if (target == null)
                return null;

            return new Paint {Image = target};
        }

        public static void SetFillImage(this ICanvas canvas, IImage image)
        {
            if (canvas != null)
            {
                var paint = image.AsPaint();
                if (paint != null)
                {
                    canvas.SetFillPaint(paint, 0, 0, 0, 0);
                }
                else
                {
                    canvas.FillColor = Colors.White;
                }
            }
        }
    }
}
