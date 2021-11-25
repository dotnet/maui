using System;
using System.IO;
using CoreGraphics;
using Microsoft.Maui.Graphics.PlatformCG;
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
				throw new Exception(string.Format(GraphicsiOS.unable_to_create_bitmap, bitmapWidth, bitmapHeight));
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

		public CGImage CGImage => _bitmapContext.ToImage();

		public override IImage Image => new PlatformImage(UIImage);

		public override void Dispose()
		{
			if (_bitmapContext != null)
			{
				_bitmapContext.Dispose();
				_bitmapContext = null;
			}

			if (_canvas != null)
			{
				_canvas.Dispose();
				_canvas = null;
			}

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
