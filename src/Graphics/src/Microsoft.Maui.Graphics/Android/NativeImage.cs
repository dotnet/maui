using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;

namespace Microsoft.Maui.Graphics.Android
{
    public class NativeImage : IImage
    {
        private Bitmap _bitmap;

        public NativeImage(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public float Width => _bitmap.Width;

        public float Height => _bitmap.Height;

        public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
        {
            var downsizedImage = _bitmap.Downsize((int) maxWidthOrHeight, disposeOriginal);
            return new NativeImage(downsizedImage);
        }

        public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
        {
            var downsizedImage = _bitmap.Downsize((int) maxWidth, (int) maxHeight, disposeOriginal);
            return new NativeImage(downsizedImage);
        }

        public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
        {
            using (var context = new NativeBitmapExportContext((int) width, (int) height))
            {
                var fx = width / Width;
                var fy = height / Height;

                var w = Width;
                var h = Height;

                var x = 0f;
                var y = 0f;

                if (resizeMode == ResizeMode.Fit)
                {
                    if (fx < fy)
                    {
                        w *= fx;
                        h *= fx;
                    }
                    else
                    {
                        w *= fy;
                        h *= fy;
                    }

                    x = (width - w) / 2;
                    y = (height - h) / 2;
                }
                else if (resizeMode == ResizeMode.Bleed)
                {
                    if (fx > fy)
                    {
                        w *= fx;
                        h *= fx;
                    }
                    else
                    {
                        w *= fy;
                        h *= fy;
                    }

                    x = (width - w) / 2;
                    y = (height - h) / 2;
                }
                else
                {
                    w = width;
                    h = height;
                }

                context.Canvas.DrawImage(this, x, y, w, h);
                return context.Image;
            }
        }

        public Bitmap NativeRepresentation => _bitmap;
        
        public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
        {
            switch (format)
            {
                case ImageFormat.Jpeg:
                    _bitmap.Compress(Bitmap.CompressFormat.Jpeg, (int) (quality * 100), stream);
                    break;
                default:
                    _bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                    break;
            }
        }

        public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
        {
            switch (format)
            {
                case ImageFormat.Jpeg:
                    await _bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, (int) (quality * 100), stream);
                    break;
                default:
                    await _bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                    break;
            }
        }

        public void Dispose()
        {
            var disp = Interlocked.Exchange(ref _bitmap, null);
            disp?.Dispose();
        }

        public void Draw(ICanvas canvas, RectangleF dirtyRect)
        {
            canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, (float) Math.Round(dirtyRect.Width), (float) Math.Round(dirtyRect.Height));
        }
    }
}