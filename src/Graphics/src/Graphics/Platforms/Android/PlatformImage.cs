using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformImage : IImage
	{
		private Bitmap _bitmap;

		public PlatformImage(Bitmap bitmap)
		{
			_bitmap = bitmap;
		}

		public float Width => _bitmap.Width;

		public float Height => _bitmap.Height;

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			var downsizedImage = _bitmap.Downsize((int)maxWidthOrHeight, disposeOriginal);
			return new PlatformImage(downsizedImage);
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			var downsizedImage = _bitmap.Downsize((int)maxWidth, (int)maxHeight, disposeOriginal);
			return new PlatformImage(downsizedImage);
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			using (var context = new PlatformBitmapExportContext(width: (int)width, height: (int)height, disposeBitmap: disposeOriginal))
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

		public Bitmap PlatformRepresentation => _bitmap;

		/// <summary>
		/// Saves the contents of this image to the provided <see cref="Stream"/> object.
		/// </summary>
		/// <param name="stream">The destination stream the bytes of this image will be saved to.</param>
		/// <param name="format">The destination format of the image.</param>
		/// <param name="quality">The destination quality of the image.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is less than 0 or more than 1.</exception>
		/// <remarks>
		/// <para>Only <see cref="ImageFormat.Png"/> and <see cref="ImageFormat.Jpeg"/> are supported on this platform.</para>
		/// <para>Setting <paramref name="quality"/> is only supported for images with <see cref="ImageFormat.Jpeg"/>.</para>
		/// </remarks>
		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			switch (format)
			{
				case ImageFormat.Jpeg:
					_bitmap.Compress(Bitmap.CompressFormat.Jpeg, (int)(quality * 100), stream);
					break;
				default:
					_bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
					break;
			}
		}

		/// <inheritdoc cref="Save"/>
		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			switch (format)
			{
				case ImageFormat.Jpeg:
					await _bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, (int)(quality * 100), stream);
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

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, (float)Math.Round(dirtyRect.Width), (float)Math.Round(dirtyRect.Height));
		}

		public IImage ToPlatformImage()
			=> this;

		public static IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			var bitmap = BitmapFactory.DecodeStream(stream);
			return new PlatformImage(bitmap);
		}
	}
}
