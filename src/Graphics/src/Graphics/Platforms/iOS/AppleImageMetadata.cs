#nullable enable

using System;
using Foundation;
using ImageIO;

namespace Microsoft.Maui.Graphics.Platform
{
	/// <summary>
	/// iOS/MacCatalyst <see cref="IImageMetadata"/> implementation. Captures the source image's
	/// CGImage properties (EXIF/GPS/etc.) as a binary plist so they can be re-embedded (JPEG/PNG) when
	/// the image is saved.
	/// </summary>
	class AppleImageMetadata : IImageMetadata
	{
		// kCGImagePropertyOrientation's string key is "Orientation".
		static readonly NSString OrientationKey = new NSString("Orientation");

		// Per-image lossy compression quality key ("kCGImageDestinationLossyCompressionQuality").
		static readonly NSString LossyCompressionQualityKey = new NSString("kCGImageDestinationLossyCompressionQuality");

		readonly byte[] _properties;

		AppleImageMetadata(byte[] properties, int orientation)
		{
			_properties = properties;
			Orientation = orientation;
		}

		/// <summary>
		/// The EXIF orientation (1-8) to write when saving. Set to 1 once the pixels have been rotated
		/// upright so a viewer doesn't rotate the already-upright image again.
		/// </summary>
		public int Orientation { get; set; }

		/// <summary>
		/// Returns a copy that shares this instance's captured properties but reports a different EXIF
		/// <see cref="Orientation"/>. Used after a transform bakes the orientation into the pixels, so the
		/// saved metadata reflects the upright pixels instead of the original orientation.
		/// </summary>
		public AppleImageMetadata WithOrientation(int orientation) =>
			new AppleImageMetadata(_properties, orientation);

		public static AppleImageMetadata? Capture(NSData data)
		{
			using var source = CGImageSource.FromData(data);
			if (source is null)
				return null;

			var properties = source.CopyProperties((NSDictionary?)null, 0);
			if (properties is null)
				return null;

			var orientation = (properties[OrientationKey] as NSNumber)?.Int32Value ?? 1;

			var plist = NSPropertyListSerialization.DataWithPropertyList(
				properties, NSPropertyListFormat.Binary, 0, out var error);
			if (plist is null || error is not null)
				return null;

			return new AppleImageMetadata(plist.ToArray(), orientation);
		}

		/// <summary>
		/// Reconstructs the properties dictionary, overriding the orientation and (for JPEG) applying the
		/// requested quality, ready to pass to the image destination when adding the image.
		/// </summary>
		public NSMutableDictionary BuildProperties(float quality, bool includeQuality)
		{
			NSMutableDictionary mutable;
			using (var data = NSData.FromArray(_properties))
			{
				var format = NSPropertyListFormat.Binary;
				var restored = NSPropertyListSerialization.PropertyListWithData(
					data, NSPropertyListReadOptions.Immutable, ref format, out _) as NSDictionary;
				mutable = restored is null ? new NSMutableDictionary() : new NSMutableDictionary(restored);
			}

			mutable[OrientationKey] = new NSNumber(Orientation);
			if (includeQuality)
				mutable[LossyCompressionQualityKey] = new NSNumber(Math.Max(0f, Math.Min(1f, quality)));

			return mutable;
		}
	}
}
