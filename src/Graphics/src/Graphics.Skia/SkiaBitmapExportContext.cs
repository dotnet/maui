using System;
using System.IO;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides a context for exporting bitmaps using SkiaSharp.
	/// </summary>
	public class SkiaBitmapExportContext : BitmapExportContext
	{
		private readonly bool _disposeBitmap;

		private SKBitmap _bitmap;
		private SKImage _image;
		private SKSurface _surface;
		private SKCanvas _skiaCanvas;
		private SkiaCanvas _platformCanvas;
		private ScalingCanvas _canvas;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaBitmapExportContext"/> class with the specified dimensions and properties.
		/// </summary>
		/// <param name="width">The width of the bitmap in pixels.</param>
		/// <param name="height">The height of the bitmap in pixels.</param>
		/// <param name="displayScale">The display scale factor to use.</param>
		/// <param name="dpi">The dots per inch (DPI) of the bitmap. Default is 72.</param>
		/// <param name="disposeBitmap">Whether to dispose the bitmap when this context is disposed.</param>
		/// <param name="transparent">Whether the bitmap should have an alpha channel (transparency).</param>
		/// <exception cref="InvalidOperationException">Thrown when unable to create a Skia surface.</exception>
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

		/// <summary>
		/// Gets the canvas that can be used to draw on this bitmap.
		/// </summary>
		public override ICanvas Canvas => _canvas;

		/// <summary>
		/// Gets the resulting image after drawing operations have been performed.
		/// </summary>
		public override IImage Image => new SkiaImage(Bitmap);

		/// <summary>
		/// Gets the Skia image representation of this bitmap.
		/// </summary>
		public SKImage SKImage => _image ?? (_image = _surface.Snapshot());

		/// <summary>
		/// Gets the Skia bitmap representation of this context.
		/// </summary>
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

		/// <summary>
		/// Releases all resources used by this bitmap export context.
		/// </summary>
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

		/// <summary>
		/// Writes the bitmap to the specified stream in PNG format.
		/// </summary>
		/// <param name="stream">The stream to write the bitmap to.</param>
		public override void WriteToStream(Stream stream)
		{
			using (var data = SKImage.Encode(SKEncodedImageFormat.Png, 100))
			{
				data.SaveTo(stream);
			}
		}
	}
}
