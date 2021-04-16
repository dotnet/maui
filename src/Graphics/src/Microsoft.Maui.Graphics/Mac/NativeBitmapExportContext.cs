using System;
using System.IO;
using AppKit;
using CoreGraphics;
using Microsoft.Maui.Graphics.CoreGraphics;

namespace Microsoft.Maui.Graphics.Native
{
    public class NativeBitmapExportContext : BitmapExportContext
    {
        private readonly CGBitmapContext _bitmapContext;
        private readonly NativeCanvas _canvas;

        public NativeBitmapExportContext(int width, int height, float displayScale = 1, int dpi = 72, int border = 0) : base(width, height, dpi)
        {
            var bitmapWidth = width + border * 2;
            var bitmapHeight = height + border * 2;

            var colorspace = CGColorSpace.CreateDeviceRGB();
            _bitmapContext = new CGBitmapContext(IntPtr.Zero, bitmapWidth, bitmapHeight, 8, 4 * bitmapWidth, colorspace, CGBitmapFlags.PremultipliedFirst);
            if (_bitmapContext == null)
            {
                throw new Exception(string.Format(GraphicsMac.unable_to_create_bitmap, bitmapWidth, bitmapHeight));
            }

            _bitmapContext.SetStrokeColorSpace(colorspace);
            _bitmapContext.SetFillColorSpace(colorspace);

            _canvas = new NativeCanvas(() => colorspace)
            {
                Context = _bitmapContext,
                DisplayScale = displayScale
            };

            _bitmapContext.SetPatternPhase(new CGSize(border, border));

            _canvas.Scale(1, -1);
            _canvas.Translate(0, -height);
            _canvas.Translate(border, -border);
        }

        public CGBitmapContext BitmapContext => _bitmapContext;

        public override ICanvas Canvas => _canvas;

        public NSImage NSImage
        {
            get
            {
                var cgimage = CGImage;
                return new NSImage(cgimage, new CGSize(cgimage.Width, cgimage.Height));
            }
        }

        public CGImage CGImage => _bitmapContext.ToImage();

        public override IImage Image => new NativeImage(NSImage);

        public override void Dispose()
        {
            _bitmapContext?.Dispose();
            base.Dispose();
        }

        public override void WriteToStream(Stream aStream)
        {
            var image = NSImage;
            var data = image.AsPng();
            data.AsStream().CopyTo(aStream);
        }
    }
}