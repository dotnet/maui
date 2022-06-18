using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Drawing = System.Drawing;


namespace Microsoft.Maui.Graphics.GDI
{
	internal class GDIImage : IImage
	{
		private Bitmap _bitmap;

		public GDIImage(Bitmap bitmap)
		{
			this._bitmap = bitmap;
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
				float factor ;

				if (Width > Height)
					factor = maxWidthOrHeight / Width;
				else
					factor = maxWidthOrHeight / Height;

				var w = (int) Math.Round(factor * Width);
				var h = (int) Math.Round(factor * Height);

				var copy = new Bitmap(_bitmap, w, h);
				var newImage = new GDIImage(copy);

				if (disposeOriginal)
					_bitmap.Dispose();

				return newImage;
			}

			return this;
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			throw new NotImplementedException();
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			throw new NotImplementedException();
		}

		public float Width => _bitmap.Size.Width;

		public float Height => _bitmap.Size.Height;

		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			try
			{
				if (format == ImageFormat.Jpeg)
				{
					var jgpEncoder = GetEncoder(Drawing.Imaging.ImageFormat.Jpeg);
					var myEncoder = Encoder.Quality;
					using (var myEncoderParameters = new EncoderParameters(1))
					{
						myEncoderParameters.Param[0] = new EncoderParameter(myEncoder, (long)(quality * 100));
						_bitmap.Save(stream, jgpEncoder, myEncoderParameters);
					}
				}
				else
				{
					_bitmap.Save(stream, Drawing.Imaging.ImageFormat.Png);
				}
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc);
			}
		}

		private ImageCodecInfo GetEncoder(Drawing.Imaging.ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}

			return null;
		}

		public Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			return Task.Factory.StartNew(() => Save(stream, format, quality));
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, Math.Abs(dirtyRect.Width), Math.Abs(dirtyRect.Height));
		}

		public IImage ToImage(int width, int height, float scale = 1f)
		{
			using var context = new GDIBitmapExportContext(width, height, scale);
			context.Canvas.Scale(scale, scale);
			Draw(context.Canvas, new RectF(0, 0, (float)width / scale, (float)height / scale));
			return context.Image;
		}


		public static IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png)
		{
			var bitmap = new Bitmap(stream);
			return new GDIImage(bitmap);
		}
	}
}
