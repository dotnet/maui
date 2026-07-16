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
		private readonly AndroidImageMetadata _metadata;

		public PlatformImage(Bitmap bitmap)
			: this(bitmap, null)
		{
		}

		internal PlatformImage(Bitmap bitmap, AndroidImageMetadata metadata)
		{
			_bitmap = bitmap;
			_metadata = metadata;
		}

		public IImageMetadata Metadata => _metadata;

		public float Width => _bitmap?.Width ?? 0;

		public float Height => _bitmap?.Height ?? 0;

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			var downsizedImage = _bitmap.Downsize((int)maxWidthOrHeight, disposeOriginal);
			return new PlatformImage(downsizedImage, _metadata);
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			var downsizedImage = _bitmap.Downsize((int)maxWidth, (int)maxHeight, disposeOriginal);
			return new PlatformImage(downsizedImage, _metadata);
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			using (var context = new PlatformBitmapExportContext(width: (int)width, height: (int)height, disposeBitmap: false))
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

		/// <inheritdoc cref="Save(System.IO.Stream, ImageFormat, float)"/>
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

		/// <inheritdoc/>
		public void Save(Stream stream, ImageFormat format, ImageSaveOptions options)
		{
			if (!TryGetMetadataToEmbed(format, options, out var metadata))
			{
				Save(stream, format, ClampQuality(options.Quality));
				return;
			}

			var tempPath = CreateTempJpegPath();
			try
			{
				using (var fileStream = File.Create(tempPath))
					_bitmap.Compress(Bitmap.CompressFormat.Jpeg, (int)(ClampQuality(options.Quality) * 100), fileStream);

				metadata.ApplyTo(tempPath);

				using var readStream = File.OpenRead(tempPath);
				readStream.CopyTo(stream);
			}
			finally
			{
				TryDeleteFile(tempPath);
			}
		}

		/// <inheritdoc/>
		public async Task SaveAsync(Stream stream, ImageFormat format, ImageSaveOptions options)
		{
			if (!TryGetMetadataToEmbed(format, options, out var metadata))
			{
				await SaveAsync(stream, format, ClampQuality(options.Quality));
				return;
			}

			var tempPath = CreateTempJpegPath();
			try
			{
				using (var fileStream = File.Create(tempPath))
					await _bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, (int)(ClampQuality(options.Quality) * 100), fileStream);

				metadata.ApplyTo(tempPath);

				using var readStream = File.OpenRead(tempPath);
				await readStream.CopyToAsync(stream);
			}
			finally
			{
				TryDeleteFile(tempPath);
			}
		}

		// Metadata embedding is only supported for JPEG. When it can't be applied we fall back to a
		// plain pixel-only save.
		bool TryGetMetadataToEmbed(ImageFormat format, ImageSaveOptions options, out AndroidImageMetadata metadata)
		{
			metadata = _metadata;
			return options.PreserveMetadata && metadata is not null && format == ImageFormat.Jpeg;
		}

		static float ClampQuality(float quality) => Math.Max(0f, Math.Min(1f, quality));

		static string CreateTempJpegPath()
			=> System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jpg");

		static void TryDeleteFile(string path)
		{
			try
			{ File.Delete(path); }
			catch { }
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
			// Use original stream if seekable, otherwise copy to memory stream
			if (stream.CanSeek)
			{
				return CreateImageFromSeekableStream(stream);
			}
			else
			{
				// Copy to memory stream and dispose it properly
				using var memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				memoryStream.Position = 0;
				return CreateImageFromSeekableStream(memoryStream);
			}
		}

		private static IImage CreateImageFromSeekableStream(Stream seekableStream)
		{
			Bitmap bitmap;

			// API 24+ (Android 7.0) required for ExifInterface stream constructor
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				try
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
				catch (Exception)
				{
					// Fallback: decode without EXIF orientation correction
					seekableStream.Position = 0;
					bitmap = BitmapFactory.DecodeStream(seekableStream);
				}
			}
			else
			{
				// Fallback for older Android
				bitmap = BitmapFactory.DecodeStream(seekableStream);
			}
			return new PlatformImage(bitmap);
		}

		// The options-based loader. The loading service (PlatformImageLoadingService) forwards to this,
		// mirroring the public FromStream(stream, format) overload above.
		public static IImage FromStream(Stream stream, ImageLoadOptions options)
		{
			// Use original stream if seekable, otherwise copy to memory stream
			if (stream.CanSeek)
			{
				return CreateImageFromSeekableStream(stream, options);
			}
			else
			{
				using var memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				memoryStream.Position = 0;
				return CreateImageFromSeekableStream(memoryStream, options);
			}
		}

		private static IImage CreateImageFromSeekableStream(Stream seekableStream, ImageLoadOptions options)
		{
			// Older Android has no ExifInterface stream constructor, so there is no orientation/metadata
			// to work with; just decode the pixels as-is.
			if (!OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return new PlatformImage(BitmapFactory.DecodeStream(seekableStream));
			}

			int orientation = 1;
			AndroidImageMetadata metadata = null;
			try
			{
				var exif = new ExifInterface(seekableStream);
				orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
				if (options.PreserveMetadata)
				{
					metadata = AndroidImageMetadata.Capture(exif, orientation);
				}
			}
			catch (Exception)
			{
				// If EXIF can't be read there is nothing to normalize or preserve.
				orientation = 1;
				metadata = null;
			}

			seekableStream.Position = 0;
			var bitmap = BitmapFactory.DecodeStream(seekableStream);

			// Apply orientation normalization unless the caller opted out.
			if (!options.DisableRotationNormalization && orientation != 1)
			{
				bitmap = RotateBitmap(bitmap, orientation);

				// The pixels are now upright, so any preserved metadata must report orientation = 1
				// to avoid a viewer rotating the already-corrected image a second time.
				if (metadata is not null)
				{
					metadata.Orientation = 1;
				}
			}

			return new PlatformImage(bitmap, metadata);
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
			matrix.Dispose();
			bitmap.Dispose();
			return rotated;
		}
	}
}
