using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaImage : IImage
	{
		private SKBitmap _image;

		public SkiaImage(SKBitmap image)
		{
			_image = image;
		}

		//protected readonly IPlatformGraphics Graphics;

		public float Width => _image.Width;

		public float Height => _image.Height;

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			// todo: implement
			/*
		 var downsizedImage = image.Downsize ((int)maxWidthOrHeight, disposeOriginal);
		 return new MDImage (downsizedImage);
			*/
			return null;
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			/*
		 var downsizedImage = image.Downsize ((int)maxWidth, (int)maxHeight, disposeOriginal);
		 return new MDImage (downsizedImage);
			*/
			return null;
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			using (var context = new SkiaBitmapExportContext((int) width, (int) height, 1))
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

		public SKBitmap PlatformRepresentation => _image;

		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			throw new NotImplementedException();
			// todo: implement me

			/*
		 switch (format)
		 {
			case ImageFormat.Jpeg:
			   image.Compress (Bitmap.CompressFormat.Jpeg, (int)(quality * 100), stream);
			   break;
			default:
			   image.Compress (Bitmap.CompressFormat.Png, 100, stream);
			   break;
		 }
			*/
		}

		public Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			throw new NotImplementedException();
			// todo: implement me

			/*
		 switch (format)
		 {
			case ImageFormat.Jpeg:
			   await image.CompressAsync (Bitmap.CompressFormat.Jpeg, (int)(quality * 100), stream);
			   break;
			default:
			   await image.CompressAsync (Bitmap.CompressFormat.Png, 100, stream);
			   break;
		 }
			*/
		}

		public void Dispose()
		{
			var previousValue = Interlocked.Exchange(ref _image, null);
			previousValue?.Dispose();
		}

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, (float)Math.Round(dirtyRect.Width), (float)Math.Round(dirtyRect.Height));
		}

		public PlatformImage ToPlatformImage()
		{
			throw new NotSupportedException();
		}

		public static IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			using (var s = new SKManagedStream(stream))
			{
				using (var codec = SKCodec.Create(s))
				{
					var info = codec.Info;
					var bitmap = new SKBitmap(info.Width, info.Height, info.ColorType, info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul);

					var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(out _));
					if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
					{
						return new SkiaImage(bitmap);
					}
				}
			}

			return null;
		}
	}
}
