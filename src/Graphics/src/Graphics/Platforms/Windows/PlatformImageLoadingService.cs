using System;
using System.ComponentModel;
using System.IO;
#if !MAUI_GRAPHICS_WIN2D
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
#endif

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="IImageLoadingService"/> which
	/// loads images into a new <see cref="IImage"/> instance.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	public class W2DImageLoadingService
#else
	public class PlatformImageLoadingService
#endif
		: IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return PlatformImage.FromStream(stream, formatHint);
		}

#if !MAUI_GRAPHICS_WIN2D
		// Loads the image applying the supplied options (EXIF orientation normalization and metadata
		// capture). Unlike the plain FromStream overload, this normalizes orientation by default so the
		// returned pixels are upright, matching the behavior of the other platforms.
		IImage IImageLoadingService.FromStream(Stream stream, ImageLoadOptions options)
			=> AsyncPump.Run(() => FromStreamAsync(stream, options));

		static async Task<IImage> FromStreamAsync(Stream stream, ImageLoadOptions options)
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
	}

#if MAUI_GRAPHICS_WIN2D
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SkiaImageLoadingService : W2DImageLoadingService { }
#endif
}
