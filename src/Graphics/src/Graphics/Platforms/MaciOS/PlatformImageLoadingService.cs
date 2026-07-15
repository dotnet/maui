using System.IO;
#if !__MACOS__
using Foundation;
using UIKit;
#endif

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return PlatformImage.FromStream(stream, formatHint);
		}

		public IImage FromStream(Stream stream, ImageLoadOptions options)
		{
#if __MACOS__
			// macOS decodes with orientation handled by NSImage and does not capture metadata; the
			// options are accepted for API parity but only the decode is performed.
			return PlatformImage.FromStream(stream, ImageFormat.Png);
#else
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
#endif
		}
	}
}
