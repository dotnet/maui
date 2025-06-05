using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Stream = System.IO.Stream;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformImage : IImage
	{
		private Bitmap _bitmap;

		public PlatformImage(Bitmap bitmap)
		{
			_bitmap = bitmap;
		}

		public float Width => _bitmap?.Width ?? 0;

		public float Height => _bitmap?.Height ?? 0;

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

				if (disposeOriginal)
				{
					_bitmap.Recycle();
					_bitmap.Dispose();
				}

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
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, MathF.Round(dirtyRect.Width), MathF.Round(dirtyRect.Height));
		}

		public IImage ToPlatformImage()
			=> this;

		public static IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			// Use original stream if it's seekable, else copy to memory stream
			var seekableStream = stream.CanSeek ? stream : new MemoryStream();
			if (!stream.CanSeek)
			{
				stream.CopyTo(seekableStream);
				seekableStream.Position = 0;
			}
			Bitmap bitmap;
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				// Read EXIF orientation
				var exif = new ExifInterface(seekableStream);
				var orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
				seekableStream.Position = 0;
				bitmap = BitmapFactory.DecodeStream(seekableStream);
				// Apply rotation only if needed
				if (orientation != 1)
				{
					bitmap = RotateBitmap(bitmap, orientation);
				}
			}
			else
			{
				// Fallback for older Android
				bitmap = BitmapFactory.DecodeStream(seekableStream);
			}
			return new PlatformImage(bitmap);
		}

		static Bitmap RotateBitmap(Bitmap bitmap, int orientation)
		{
			// EXIF orientation has 8 possible values. See: https://jdhao.github.io/2019/07/31/image_rotation_exif_info/#exif-orientation-flag
			Matrix matrix = new Matrix();
			switch (orientation)
			{
				case 2: // Flip horizontal
					matrix.PreScale(-1, 1);
					break;
				case 3: // Rotate 180
					matrix.PostRotate(180);
					break;
				case 4: // Flip vertical
					matrix.PreScale(1, -1);
					break;
				case 5: // Transpose (flip vertical + rotate 90)
					matrix.PreScale(1, -1);
					matrix.PostRotate(90);
					break;
				case 6: // Rotate 90
					matrix.PostRotate(90);
					break;
				case 7: // Transverse (flip vertical + rotate 270)
					matrix.PreScale(1, -1);
					matrix.PostRotate(270);
					break;
				case 8: // Rotate 270
					matrix.PostRotate(270);
					break;
				default:
					return bitmap;
			}
			var rotated = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
			bitmap.Dispose();
			return rotated;
		}
	}
}
