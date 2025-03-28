﻿using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.IO;
using Windows.Storage.Streams;

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

		public CanvasBitmap PlatformRepresentation => _bitmap;

		public void Dispose()
		{
			var bitmap = Interlocked.Exchange(ref _bitmap, null);
			bitmap?.Dispose();
		}

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			if (Width <= maxWidthOrHeight && Height <= maxWidthOrHeight)
			{
				return this;
			}

			// Calculate new dimensions while maintaining aspect ratio
			float scaleFactor = Math.Min(
				maxWidthOrHeight / Width,
				maxWidthOrHeight / Height
			);

			int newWidth = (int)(Width * scaleFactor);
			int newHeight = (int)(Height * scaleFactor);

			using (var memoryStream = new InMemoryRandomAccessStream())
			{
				Save(memoryStream.AsStreamForWrite());
				memoryStream.Seek(0);

				// Load original bitmap
				var originalBitmap = AsyncPump.Run(async () =>
					await CanvasBitmap.LoadAsync(_creator, memoryStream, 96));

				// Create render target with new dimensions
				using (var renderTarget = new CanvasRenderTarget(_creator, newWidth, newHeight, 96))
				{
					using (var drawingSession = renderTarget.CreateDrawingSession())
					{
						// Draw the original bitmap scaled to new dimensions
						drawingSession.DrawImage(originalBitmap,
							new global::Windows.Foundation.Rect(0, 0, newWidth, newHeight));
					}

					// Save resized image to a new memory stream
					using (var resizedStream = new InMemoryRandomAccessStream())
					{
						AsyncPump.Run(async () =>
							await renderTarget.SaveAsync(resizedStream, CanvasBitmapFileFormat.Png));

						resizedStream.Seek(0);
						var newImage = FromStream(resizedStream.AsStreamForRead());

						if (disposeOriginal)
							_bitmap.Dispose();

						return newImage;
					}
				}
			}
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

			if (creator is null)
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
