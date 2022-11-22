using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Graphics.Win2D
{
	internal class W2DImage : IImage
	{
		private readonly ICanvasResourceCreator _creator;
		private CanvasBitmap _bitmap;

		public W2DImage(ICanvasResourceCreator creator, CanvasBitmap bitmap)
		{
			_creator = creator;
			_bitmap = bitmap;
		}

		public CanvasBitmap PlatformImage => _bitmap;

		public void Dispose()
		{
			var bitmap = Interlocked.Exchange(ref _bitmap, null);
			bitmap?.Dispose();
		}

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			if (Width > maxWidthOrHeight || Height > maxWidthOrHeight)
			{
				using (var memoryStream = new InMemoryRandomAccessStream())
				{
					Save(memoryStream.AsStreamForWrite());
					memoryStream.Seek(0);

					// ReSharper disable once AccessToDisposedClosure
					var newBitmap = AsyncPump.Run(async () => await CanvasBitmap.LoadAsync(_creator, memoryStream, 96));
					using (var memoryStream2 = new InMemoryRandomAccessStream())
					{
						// ReSharper disable once AccessToDisposedClosure
						AsyncPump.Run(async () => await newBitmap.SaveAsync(memoryStream2, CanvasBitmapFileFormat.Png));

						memoryStream2.Seek(0);
						var newImage = FromStream(memoryStream2.AsStreamForRead());
						if (disposeOriginal)
							_bitmap.Dispose();

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

		public float Width => (float)_bitmap.Size.Width;

		public float Height => (float)_bitmap.Size.Height;

		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
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

		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
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
			throw new NotImplementedException();
		}

		public IImage ToImage(int width, int height, float scale = 1f)
		{
			throw new NotImplementedException();
		}

		public static IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png)
		{
			var creator = W2DGraphicsService.Creator;
			if (creator == null)
				throw new Exception("No resource creator has been registered globally or for this thread.");

			var bitmap = AsyncPump.Run(async () => await CanvasBitmap.LoadAsync(creator, stream.AsRandomAccessStream()));
			return new W2DImage(creator, bitmap);
		}
	}
}
