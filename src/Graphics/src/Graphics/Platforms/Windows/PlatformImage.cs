using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.IO;
using Windows.Foundation;
using Windows.Storage.Streams;
#if !MAUI_GRAPHICS_WIN2D
using Windows.Graphics.Imaging;
#endif
using WinRect = Windows.Foundation.Rect;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="IImage"/>.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	internal class W2DImage
#else
	public class PlatformImage
#endif
		: IImage
	{
		private readonly ICanvasResourceCreator _creator;
		private CanvasBitmap _bitmap;
#if !MAUI_GRAPHICS_WIN2D
		private WindowsImageMetadata _metadata;
#endif

		private static readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager = new();

#if MAUI_GRAPHICS_WIN2D
		public W2DImage(
#else
		public PlatformImage(
#endif
			ICanvasResourceCreator creator, CanvasBitmap bitmap)
		{
			_creator = creator;
			_bitmap = bitmap;
		}

#if !MAUI_GRAPHICS_WIN2D
		internal PlatformImage(ICanvasResourceCreator creator, CanvasBitmap bitmap, WindowsImageMetadata metadata)
		{
			_creator = creator;
			_bitmap = bitmap;
			_metadata = metadata;
		}
#endif

		public CanvasBitmap PlatformRepresentation => _bitmap;

		public void Dispose()
		{
			var bitmap = Interlocked.Exchange(ref _bitmap, null);
			bitmap?.Dispose();
		}

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			return Downsize(maxWidthOrHeight, maxWidthOrHeight, disposeOriginal);
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			if (maxWidth <= 0 || maxHeight <= 0)
			{
				return this;
			}

			if (Width > maxWidth || Height > maxHeight)
			{
				float factorX = maxWidth / Width;
				float factorY = maxHeight / Height;

				float factor = Math.Min(factorX, factorY);

				var targetWidth = factor * Width;
				var targetHeight = factor * Height;

				return ResizeInternal(targetWidth, targetHeight, 0, 0, targetWidth, targetHeight, disposeOriginal);
			}

			return this;
		}

		IImage ResizeInternal(float canvasWidth, float canvasHeight, float drawX, float drawY, float drawWidth, float drawHeight, bool disposeOriginal)
		{
			using var renderTarget = new CanvasRenderTarget(_creator, canvasWidth, canvasHeight, _bitmap.Dpi);

			using (var drawingSession = renderTarget.CreateDrawingSession())
			{
				drawingSession.DrawImage(_bitmap, new global::Windows.Foundation.Rect(drawX, drawY, drawWidth, drawHeight));
			}

			using (var resizedStream = new InMemoryRandomAccessStream())
			{
				var saveCompletedEvent = new ManualResetEventSlim(false);
				Exception saveException = null;

				// Start the async save operation
				var saveTask = renderTarget.SaveAsync(resizedStream, CanvasBitmapFileFormat.Png).AsTask();

				saveTask.ContinueWith(task =>
				{
					if (task.Exception is not null)
					{
						saveException = task.Exception;
					}
					// Signal that the operation is complete
					saveCompletedEvent.Set();
				});

				// Wait for the signal
				saveCompletedEvent.Wait();

				// Check for any exceptions during the async operation
				if (saveException is not null)
				{
					throw saveException;
				}

				resizedStream.Seek(0);

				var newImage = FromStream(resizedStream.AsStreamForRead());

#if !MAUI_GRAPHICS_WIN2D
				// The resized pixels are already upright (orientation was normalized on load), so the
				// captured metadata carries over unchanged.
				if (newImage is PlatformImage resized)
					resized._metadata = _metadata;
#endif

				if (disposeOriginal)
				{
					_bitmap.Dispose();
				}

				return newImage;
			}
		}

		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit,
			bool disposeOriginal = false)
		{
			// Calculate scaling factors
			float scaleX = width / Width;
			float scaleY = height / Height;

			float targetWidth = Width;
			float targetHeight = Height;
			float offsetX = 0;
			float offsetY = 0;

			// Adjust dimensions based on the resize mode
			if (resizeMode == ResizeMode.Fit)
			{
				float scale = Math.Min(scaleX, scaleY);
				targetWidth *= scale;
				targetHeight *= scale;
				offsetX = (width - targetWidth) / 2;
				offsetY = (height - targetHeight) / 2;
			}
			else if (resizeMode == ResizeMode.Bleed)
			{
				float scale = Math.Max(scaleX, scaleY);
				targetWidth *= scale;
				targetHeight *= scale;
				offsetX = (width - targetWidth) / 2;
				offsetY = (height - targetHeight) / 2;
			}
			else
			{
				targetWidth = width;
				targetHeight = height;
			}

			return ResizeInternal(width, height, offsetX, offsetY, targetWidth, targetHeight, disposeOriginal);
		}

		public float Width => (float)_bitmap.Size.Width;

		public float Height => (float)_bitmap.Size.Height;

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
					AsyncPump.Run(async () => await _bitmap.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Jpeg, quality));
					break;
				default:
					AsyncPump.Run(async () => await _bitmap.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Png));
					break;
			}
		}

		/// <inheritdoc cref="Save" />
		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			switch (format)
			{
				case ImageFormat.Jpeg:
					await _bitmap.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Jpeg, quality);
					break;
				default:
					await _bitmap.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Png);
					break;
			}
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, Math.Abs(dirtyRect.Width), Math.Abs(dirtyRect.Height));
		}

#if !MAUI_GRAPHICS_WIN2D
		IImageMetadata IImage.Metadata => _metadata;

		void IImage.Save(Stream stream, ImageFormat format, ImageSaveOptions options)
			=> AsyncPump.Run(() => SaveWithOptionsAsync(stream, format, options));

		Task IImage.SaveAsync(Stream stream, ImageFormat format, ImageSaveOptions options)
			=> SaveWithOptionsAsync(stream, format, options);

		async Task SaveWithOptionsAsync(Stream stream, ImageFormat format, ImageSaveOptions options)
		{
			var quality = Math.Max(0f, Math.Min(1f, options.Quality));

			// Metadata re-embedding is only supported for JPEG on this platform.
			if (!options.PreserveMetadata || _metadata is null || format != ImageFormat.Jpeg)
			{
				await SaveAsync(stream, format, quality);
				return;
			}

			// 1. Encode the (processed) pixels to a temporary in-memory JPEG.
			using var pixelStream = new InMemoryRandomAccessStream();
			await _bitmap.SaveAsync(pixelStream, CanvasBitmapFileFormat.Jpeg, quality);
			pixelStream.Seek(0);

			// 2. Transcode into the destination while injecting the captured metadata.
			var decoder = await BitmapDecoder.CreateAsync(pixelStream);
			using var outputStream = new InMemoryRandomAccessStream();
			var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
			await _metadata.ApplyToAsync(encoder.BitmapProperties);
			await encoder.FlushAsync();

			outputStream.Seek(0);
			await outputStream.AsStreamForRead().CopyToAsync(stream);
		}

		// Loads the image applying the supplied options (EXIF orientation normalization and metadata
		// capture). Unlike the plain FromStream overload, this normalizes orientation by default so the
		// returned pixels are upright, matching the behavior of the other platforms.
		internal static IImage FromStreamWithOptions(Stream stream, ImageLoadOptions options)
			=> AsyncPump.Run(() => FromStreamWithOptionsAsync(stream, options));

		static async Task<IImage> FromStreamWithOptionsAsync(Stream stream, ImageLoadOptions options)
		{
			var creator = PlatformGraphicsService.Creator;
			if (creator is null)
			{
				throw new Exception("No resource creator has been registered globally or for this thread.");
			}

			using var randomAccessStream = new InMemoryRandomAccessStream();
			await stream.CopyToAsync(randomAccessStream.AsStreamForWrite());
			randomAccessStream.Seek(0);

			var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

			var metadata = options.PreserveMetadata
				? await WindowsImageMetadata.CaptureAsync(decoder)
				: null;

			// Win2D's CanvasBitmap.LoadAsync ignores EXIF orientation, so bake it in here unless the
			// caller opted out.
			var exifMode = options.DisableRotationNormalization
				? ExifOrientationMode.IgnoreExifOrientation
				: ExifOrientationMode.RespectExifOrientation;

			using var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
				BitmapPixelFormat.Bgra8,
				BitmapAlphaMode.Premultiplied,
				new BitmapTransform(),
				exifMode,
				ColorManagementMode.DoNotColorManage);

			var bitmap = CanvasBitmap.CreateFromSoftwareBitmap(creator, softwareBitmap);

			// If we normalized orientation, the pixels are now upright, so persist orientation as 1 to
			// avoid a viewer double-rotating the already-upright image.
			if (metadata is not null && !options.DisableRotationNormalization)
				metadata.Orientation = 1;

			return new PlatformImage(creator, bitmap, metadata);
		}
#endif

		public IImage ToPlatformImage()
		{
#if MAUI_GRAPHICS_WIN2D
			return new Platform.PlatformImage(_creator, _bitmap);
#else
			return this;
#endif
		}

		public IImage ToImage(int width, int height, float scale = 1f)
		{
			throw new NotImplementedException();
		}

		public static IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png)
		{
			var creator = PlatformGraphicsService.Creator;

			if (creator == null)
			{
				throw new Exception("No resource creator has been registered globally or for this thread.");
			}

			CanvasBitmap bitmap;

			if (stream.CanSeek)
			{
				var bitmapAsync = CanvasBitmap.LoadAsync(creator, stream.AsRandomAccessStream());
				bitmap = bitmapAsync.AsTask().GetAwaiter().GetResult();
			}
			else
			{
				using var memoryStream = recyclableMemoryStreamManager.GetStream();
				stream.CopyTo(memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);

				var bitmapAsync = CanvasBitmap.LoadAsync(creator, memoryStream.AsRandomAccessStream());
				bitmap = bitmapAsync.AsTask().GetAwaiter().GetResult();
			}

			return new PlatformImage(creator, bitmap);
		}
	}
}
