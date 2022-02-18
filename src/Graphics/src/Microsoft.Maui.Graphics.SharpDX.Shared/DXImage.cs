using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct2D1;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public class DXImage : IImage
	{
		private Bitmap _bitmap;

		public DXImage(Bitmap bitmap)
		{
			_bitmap = bitmap;
		}

		public Bitmap NativeImage => _bitmap;

		public void Dispose()
		{
			var bitmap = Interlocked.Exchange(ref _bitmap, null);
			bitmap?.Dispose();
		}

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			if (Width > maxWidthOrHeight || Height > maxWidthOrHeight)
			{
				float factor;

				if (Width > Height)
				{
					factor = maxWidthOrHeight / Width;
				}
				else
				{
					factor = maxWidthOrHeight / Height;
				}

				var w = (int) Math.Round(factor * Width);
				var h = (int) Math.Round(factor * Height);

				using (var memoryStream = new MemoryStream())
				{
					Save(memoryStream);
					memoryStream.Position = 0;

					global::SharpDX.WIC.Bitmap wicBitmap = WicBitmapExtensions.LoadFromStream(memoryStream);
					using (var memoryStream2 = new MemoryStream())
					{
						wicBitmap.WritePngToStream(memoryStream2, w, h);
						memoryStream2.Position = 0;
						var newImage = FromStream(memoryStream2);
						if (disposeOriginal)
						{
							_bitmap.Dispose();
						}

						return newImage;
					}
				}
			}

			return this;
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			throw new NotImplementedException();
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit,
			bool disposeOriginal = false)
		{
			throw new NotImplementedException();
		}

		public float Width => _bitmap.Size.Width;

		public float Height => _bitmap.Size.Height;

		public void Save(Stream stream, Graphics.ImageFormat format = Graphics.ImageFormat.Png, float quality = 1)
		{
			switch (format)
			{
				case Graphics.ImageFormat.Jpeg:
					_bitmap.EncodeImage(ImageFormat.Jpeg, stream);
					break;
				default:
					_bitmap.EncodeImage(ImageFormat.Png, stream);
					break;
			}
		}

		public Task SaveAsync(Stream stream, Graphics.ImageFormat format = Graphics.ImageFormat.Png, float quality = 1)
		{
			return Task.Factory.StartNew(() => Save(stream, format, quality));
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, Math.Abs(dirtyRect.Width), Math.Abs(dirtyRect.Height));
		}

		public IImage ToImage(int width, int height, float scale = 1f)
		{
			using var context = new DXBitmapExportContext(width, height, scale);
			context.Canvas.Scale(scale, scale);
			Draw(context.Canvas, new RectF(0, 0, (float)width / scale, (float)height / scale));
			return context.Image;
		}


		public static IImage FromStream(Stream stream, Graphics.ImageFormat format = Graphics.ImageFormat.Png)
		{
			var bitmap = DXGraphicsService.CurrentTarget.Value.LoadBitmap(stream);
			return new DXImage(bitmap);
		}
	}
}
