using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using ImageIO;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformImage : IImage
	{
		private UIImage _image;
		private readonly AppleImageMetadata _metadata;

		public PlatformImage(UIImage image)
			: this(image, null)
		{
		}

		internal PlatformImage(UIImage image, AppleImageMetadata metadata)
		{
			_image = image;
			_metadata = metadata;
		}

		public IImageMetadata Metadata => _metadata;

		public float Width => (float)(_image?.Size.Width ?? 0);

		public float Height => (float)(_image?.Size.Height ?? 0);

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			var scaledImage = _image.ScaleImage(maxWidthOrHeight, maxWidthOrHeight, disposeOriginal);
			return new PlatformImage(scaledImage, _metadata);
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			var scaledImage = _image.ScaleImage(maxWidth, maxHeight, disposeOriginal);
			return new PlatformImage(scaledImage, _metadata);
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			using (var context = new PlatformBitmapExportContext((int)width, (int)height, 1))
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
					_image.Dispose();
				}

				return context.Image;
			}
		}

		public UIImage PlatformRepresentation => _image;

		/// <summary>
		/// Saves the contents of this image to the provided <see cref="Stream"/> object.
		/// </summary>
		/// <param name="stream">The destination stream the bytes of this image will be saved to.</param>
		/// <param name="format">The destination format of the image.</param>
		/// <param name="quality">The destination quality of the image.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is less than 0 or more than 1.</exception>
		/// <exception cref="Exception">Thrown when this image has no data.</exception>
		/// <remarks>
		/// <para>Only <see cref="ImageFormat.Png"/> and <see cref="ImageFormat.Jpeg"/> are supported on this platform.</para>
		/// <para>Setting <paramref name="quality"/> is only supported for images with <see cref="ImageFormat.Jpeg"/>.</para>
		/// </remarks>
		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			var data = CreateData(format, quality);
			data.AsStream().CopyTo(stream);
		}

		/// <inheritdoc cref="Save(System.IO.Stream, ImageFormat, float)" />
		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			var data = CreateData(format, quality);
			await data.AsStream().CopyToAsync(stream);
		}

		/// <inheritdoc/>
		public void Save(Stream stream, ImageFormat format, ImageSaveOptions options)
		{
			using var data = CreateData(format, options);
			data.AsStream().CopyTo(stream);
		}

		/// <inheritdoc/>
		public Task SaveAsync(Stream stream, ImageFormat format, ImageSaveOptions options)
		{
			// NSData-backed streams don't reliably support async reads on CoreCLR (CopyToAsync throws
			// inside UnmanagedMemoryStream.ReadAsync), so copy synchronously — this streams the encoded
			// bytes straight to the destination without an extra managed buffer.
			Save(stream, format, options);
			return Task.CompletedTask;
		}

		private NSData CreateData(ImageFormat format, ImageSaveOptions options)
		{
			// Metadata embedding is supported for JPEG and PNG via ImageIO.
			if (options.PreserveMetadata && _metadata is not null &&
				(format == ImageFormat.Jpeg || format == ImageFormat.Png))
			{
				var data = CreateDataWithMetadata(format, options.Quality);
				if (data is not null)
					return data;
			}

			return CreateData(format, ClampQuality(options.Quality));
		}

		private NSData CreateDataWithMetadata(ImageFormat format, float quality)
		{
			var cgImage = _image?.CGImage;
			if (cgImage is null)
				return null;

			var uti = format == ImageFormat.Png ? "public.png" : "public.jpeg";
			var outputData = new NSMutableData();
			using var destination = CGImageDestination.Create(outputData, uti, 1);
			if (destination is null)
			{
				outputData.Dispose();
				return null;
			}

			using var properties = _metadata.BuildProperties(quality, includeQuality: format == ImageFormat.Jpeg);
			destination.AddImage(cgImage, properties);
			if (!destination.Close())
			{
				outputData.Dispose();
				return null;
			}

			return outputData;
		}

		static float ClampQuality(float quality) => Math.Max(0f, Math.Min(1f, quality));

		private NSData CreateData(ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			NSData data;
			switch (format)
			{
				case ImageFormat.Jpeg:
					data = _image.AsJPEG(quality);
					break;
				default:
					data = _image.AsPNG();
					break;
			}

			if (data == null)
			{
				throw new Exception($"Unable to write the image in the {format} format.");
			}

			return data;
		}

		public void Dispose()
		{
			var disp = Interlocked.Exchange(ref _image, null);
			disp?.Dispose();
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, MathF.Round(dirtyRect.Width), MathF.Round(dirtyRect.Height));
		}

		public IImage ToPlatformImage()
			=> this;

		public static IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png)
		{
			var data = NSData.FromStream(stream);
			var image = UIImage.LoadFromData(data);
			return new PlatformImage(image.NormalizeOrientation(disposeOriginal: true));
		}

		public static IImage FromStream(Stream stream, ImageLoadOptions options)
		{
			using var data = NSData.FromStream(stream);

			var metadata = options.PreserveMetadata && data is not null
				? AppleImageMetadata.Capture(data)
				: null;

			var image = UIImage.LoadFromData(data);

			if (options.DisableRotationNormalization)
			{
				return new PlatformImage(image, metadata);
			}

			var normalized = image.NormalizeOrientation(disposeOriginal: true);

			// Pixels are upright now, so any preserved metadata must report orientation = 1.
			if (metadata is not null)
			{
				metadata.Orientation = 1;
			}

			return new PlatformImage(normalized, metadata);
		}
	}
}
