using System;
using System.IO;
using Android.Graphics;
using Android.Media;
using Stream = System.IO.Stream;

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
			// Use the original stream when seekable; otherwise copy to memory so EXIF can be read.
			if (stream.CanSeek)
			{
				return CreateImage(stream, options);
			}

			using var memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			memoryStream.Position = 0;
			return CreateImage(memoryStream, options);
		}

		static IImage CreateImage(Stream seekableStream, ImageLoadOptions options)
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
				bitmap = PlatformImage.RotateBitmap(bitmap, orientation);

				// The pixels are now upright, so any preserved metadata must report orientation = 1
				// to avoid a viewer rotating the already-corrected image a second time.
				if (metadata is not null)
				{
					metadata.Orientation = 1;
				}
			}

			return new PlatformImage(bitmap, metadata);
		}
	}
}
