using System;
using System.IO;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformBitmapExportContext : BitmapExportContext
	{
		private CGBitmapContext _bitmapContext;
		private PlatformCanvas _canvas;

		public PlatformBitmapExportContext(int width, int height, float displayScale, int dpi = 72, int border = 0) : base(width, height, dpi)
		{
			var bitmapWidth = width + border * 2;
			var bitmapHeight = height + border * 2;

			var colorspace = CGColorSpace.CreateDeviceRGB();
			_bitmapContext = new CGBitmapContext(IntPtr.Zero, bitmapWidth, bitmapHeight, 8, 4 * bitmapWidth, colorspace, CGBitmapFlags.PremultipliedFirst);
			if (_bitmapContext == null)
			{
				throw new Exception(string.Format("Unable to allocate enough memory to create a bitmap of size {0}x{1}.", bitmapWidth, bitmapHeight));
			}

			_bitmapContext.SetStrokeColorSpace(colorspace);
			_bitmapContext.SetFillColorSpace(colorspace);

			_canvas = new PlatformCanvas(() => colorspace)
			{
				Context = _bitmapContext,
				DisplayScale = displayScale
			};

			_bitmapContext.SetPatternPhase(new System.Drawing.SizeF(border, border));

			_canvas.Scale(1, -1);
			_canvas.Translate(0, -height);
			_canvas.Translate(border, -border);
		}

		public override ICanvas Canvas => _canvas;

		public UIImage UIImage => UIImage.FromImage(_bitmapContext.ToImage());

		public PlatformImage PlatformImage => new PlatformImage(UIImage);

		public CGImage CGImage => _bitmapContext.ToImage();

		public override IImage Image => PlatformImage;

		public override void Dispose()
		{
			_bitmapContext?.Dispose();
			_bitmapContext = null;

			_canvas?.Dispose();
			_canvas = null;

			base.Dispose();
		}

		public override void WriteToStream(Stream aStream)
		{
			var image = UIImage;
			var data = image.AsPNG();
			data.AsStream().CopyTo(aStream);
		}
	}
}
