using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Represents an image implementation using SkiaSharp.
	/// </summary>
	public class SkiaImage : IImage
	{
		private SKBitmap _image;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaImage"/> class with the specified SkiaSharp bitmap.
		/// </summary>
		/// <param name="image">The SkiaSharp bitmap to wrap.</param>
		public SkiaImage(SKBitmap image)
		{
			_image = image;
		}

		//protected readonly IPlatformGraphics Graphics;

		/// <summary>
		/// Gets the width of the image in pixels.
		/// </summary>
		public float Width => _image.Width;

		/// <summary>
		/// Gets the height of the image in pixels.
		/// </summary>
		public float Height => _image.Height;

		/// <summary>
		/// Gets the underlying SkiaSharp bitmap that this image wraps.
		/// </summary>
		public SKBitmap PlatformRepresentation => _image;

		/// <summary>
		/// Creates a new image by downsizing this image to fit within the specified maximum dimension.
		/// </summary>
		/// <param name="maxWidthOrHeight">The maximum width or height the resulting image should have.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after downsizing.</param>
		/// <returns>A new image instance with the downsized dimensions.</returns>
		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			// todo: implement
			/*
		 var downsizedImage = image.Downsize ((int)maxWidthOrHeight, disposeOriginal);
		 return new MDImage (downsizedImage);
			*/
			return null;
		}

		/// <summary>
		/// Creates a new image by downsizing this image to fit within the specified maximum dimensions.
		/// </summary>
		/// <param name="maxWidth">The maximum width the resulting image should have.</param>
		/// <param name="maxHeight">The maximum height the resulting image should have.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after downsizing.</param>
		/// <returns>A new image instance with the downsized dimensions.</returns>
		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			/*
		 var downsizedImage = image.Downsize ((int)maxWidth, (int)maxHeight, disposeOriginal);
		 return new MDImage (downsizedImage);
			*/
			return null;
		}

		/// <summary>
		/// Creates a new image by resizing this image to the specified dimensions.
		/// </summary>
		/// <param name="width">The width of the resulting image.</param>
		/// <param name="height">The height of the resulting image.</param>
		/// <param name="resizeMode">The mode to use when resizing the image.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after resizing.</param>
		/// <returns>A new image instance with the specified dimensions.</returns>
		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			using (var context = new SkiaBitmapExportContext((int)width, (int)height, 1))
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

		/// <summary>
		/// Saves the image to a stream in the specified format.
		/// </summary>
		/// <param name="stream">The stream to save the image to.</param>
		/// <param name="format">The format to save the image in.</param>
		/// <param name="quality">The quality level to use when saving the image (0.0 to 1.0).</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when quality is outside the range of 0.0 to 1.0 or when format is not supported.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown when the specified format is not supported by Skia.</exception>
		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			var skStream = GetSkStream(format, quality);

			using (skStream)
			{
				skStream.CopyTo(stream);
			}
		}

		/// <summary>
		/// Asynchronously saves the image to a stream in the specified format.
		/// </summary>
		/// <param name="stream">The stream to save the image to.</param>
		/// <param name="format">The format to save the image in.</param>
		/// <param name="quality">The quality level to use when saving the image (0.0 to 1.0).</param>
		/// <returns>A task that represents the asynchronous save operation.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when quality is outside the range of 0.0 to 1.0 or when format is not supported.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown when the specified format is not supported by Skia.</exception>
		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			var skStream = GetSkStream(format, quality);
			using (skStream)
			{
				await skStream.CopyToAsync(stream);
			}
		}

		private Stream GetSkStream(ImageFormat format, float quality)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			// Skia quality range from 0-100, this is supported by jpeg and webp. Higher values correspond to improved visual quality, but less compression.
			const int MaxSKQuality = 100;
			var skQuality = (int)(MaxSKQuality * quality);
			SKEncodedImageFormat skEncodedImageFormat = format switch
			{
				ImageFormat.Png => SKEncodedImageFormat.Png,
				ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
				ImageFormat.Bmp or ImageFormat.Gif or ImageFormat.Tiff => throw new PlatformNotSupportedException($"Skia does not support {format} format."),
				_ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
			};

			var skData = _image.Encode(skEncodedImageFormat, skQuality);
			var skStream = skData.AsStream(streamDisposesData: true);
			return skStream;
		}

		/// <summary>
		/// Releases all resources used by this image.
		/// </summary>
		public void Dispose()
		{
			var previousValue = Interlocked.Exchange(ref _image, null);
			previousValue?.Dispose();
		}

		/// <summary>
		/// Draws this image on the specified canvas within the specified rectangle.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="dirtyRect">The rectangle in which to draw the image.</param>
		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, MathF.Round(dirtyRect.Width), MathF.Round(dirtyRect.Height));
		}

		/// <summary>
		/// Creates a platform-specific image from this image.
		/// </summary>
		/// <returns>A platform-specific image representation.</returns>
		/// <exception cref="NotSupportedException">This operation is not supported in the Skia implementation.</exception>
		public IImage ToPlatformImage()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates a new image from a stream.
		/// </summary>
		/// <param name="stream">The stream containing image data.</param>
		/// <param name="formatHint">Optional hint about the image format.</param>
		/// <returns>A new <see cref="IImage"/> instance containing the image from the stream.</returns>
		/// <exception cref="ArgumentException">Thrown when the stream cannot be decoded as an image.</exception>
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
