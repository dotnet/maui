using System;
using System.IO;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaBitmapExportContext : BitmapExportContext
	{
		private readonly bool _disposeBitmap;

		private SKBitmap _bitmap;
		private SKImage _image;
		private SKSurface _surface;
		private SKCanvas _skiaCanvas;
		private SkiaCanvas _platformCanvas;
		private ScalingCanvas _canvas;

		public SkiaBitmapExportContext(
			int width,
			int height,
			float displayScale,
			int dpi = 72,
			bool disposeBitmap = true,
			bool transparent = true) : base(width, height, dpi)
		{
			if (transparent)
			{
				var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
				_surface = SKSurface.Create(imageInfo);
			}
			else
			{
				var imageInfo = new SKImageInfo(width, height, SKColorType.Rgb565, SKAlphaType.Opaque);
				_surface = SKSurface.Create(imageInfo);
			}

			if (_surface == null)
			{
				throw new InvalidOperationException("Unable to create a Skia surface.");
			}

			_skiaCanvas = _surface.Canvas;
			_platformCanvas = new SkiaCanvas
			{
				Canvas = _skiaCanvas,
				DisplayScale = displayScale
			};
			_canvas = new ScalingCanvas(_platformCanvas);
			_disposeBitmap = disposeBitmap;
		}

		public override ICanvas Canvas => _canvas;

		public override IImage Image => new SkiaImage(Bitmap);

		public SKImage SKImage => _image ?? (_image = _surface.Snapshot());

		public SKBitmap Bitmap
		{
			get
			{
				if (_bitmap == null)
				{
					_bitmap = SKBitmap.FromImage(SKImage);
				}

				return _bitmap;
			}
		}

		public override void Dispose()
		{
			if (_platformCanvas != null)
			{
				_platformCanvas.Dispose();
				_platformCanvas = null!;
			}

			if (_skiaCanvas != null)
			{
				_skiaCanvas.Dispose();
				_skiaCanvas = null!;
			}

			if (_surface != null)
			{
				_surface.Dispose();
				_surface = null!;
			}

			if (_image != null && _disposeBitmap)
			{
				_image.Dispose();
				_image = null;
			}

			if (_bitmap != null && _disposeBitmap)
			{
				_bitmap.Dispose();
				_bitmap = null;
			}

			_canvas = null!;

			base.Dispose();
		}

		public override void WriteToStream(Stream stream)
		{
			using (var data = SKImage.Encode(SKEncodedImageFormat.Png, 100))
			{
				data.SaveTo(stream);
			}
		}
	}
}
