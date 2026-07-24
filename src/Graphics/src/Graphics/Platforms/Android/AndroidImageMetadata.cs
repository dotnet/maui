using System;
using System.Collections.Generic;
using Android.Media;

namespace Microsoft.Maui.Graphics.Platform
{
	/// <summary>
	/// Android <see cref="IImageMetadata"/> implementation that captures a snapshot of the source
	/// image's EXIF tags so they can be re-embedded (JPEG only) when the image is saved.
	/// </summary>
	class AndroidImageMetadata : IImageMetadata
	{
		// A curated set of EXIF tags worth preserving across a load/transform/save round trip.
		// Orientation is handled separately (see Orientation) because the pixels may be rotated upright.
		static readonly string[] PreservedTags =
		{
			ExifInterface.TagDatetime,
			ExifInterface.TagDatetimeOriginal,
			ExifInterface.TagDatetimeDigitized,
			ExifInterface.TagMake,
			ExifInterface.TagModel,
			ExifInterface.TagSoftware,
			ExifInterface.TagArtist,
			ExifInterface.TagCopyright,
			ExifInterface.TagImageDescription,
			ExifInterface.TagUserComment,
			ExifInterface.TagExposureTime,
			ExifInterface.TagFNumber,
			ExifInterface.TagApertureValue,
			ExifInterface.TagShutterSpeedValue,
			ExifInterface.TagFocalLength,
			ExifInterface.TagFocalLengthIn35mmFilm,
			ExifInterface.TagFlash,
			ExifInterface.TagWhiteBalance,
			ExifInterface.TagExposureProgram,
			ExifInterface.TagExposureBiasValue,
			ExifInterface.TagMeteringMode,
			ExifInterface.TagGpsLatitude,
			ExifInterface.TagGpsLatitudeRef,
			ExifInterface.TagGpsLongitude,
			ExifInterface.TagGpsLongitudeRef,
			ExifInterface.TagGpsAltitude,
			ExifInterface.TagGpsAltitudeRef,
			ExifInterface.TagGpsTimestamp,
			ExifInterface.TagGpsDatestamp,
			ExifInterface.TagGpsProcessingMethod,
		};

		readonly Dictionary<string, string> _tags;

		AndroidImageMetadata(Dictionary<string, string> tags, int orientation)
		{
			_tags = tags;
			Orientation = orientation;
		}

		/// <summary>
		/// The EXIF orientation value (1-8) to write when saving. Set to 1 after the pixels have been
		/// rotated upright, so that a viewer does not rotate the already-upright image a second time.
		/// </summary>
		public int Orientation { get; set; }

		/// <summary>Reads the preserved EXIF tags (and orientation) from an already-parsed source.</summary>
		public static AndroidImageMetadata Capture(ExifInterface source, int orientation)
		{
			var tags = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (var tag in PreservedTags)
			{
				var value = source.GetAttribute(tag);
				if (!string.IsNullOrEmpty(value))
					tags[tag] = value;
			}

			return new AndroidImageMetadata(tags, orientation);
		}

		/// <summary>Writes the captured tags (and the current <see cref="Orientation"/>) into a JPEG file.</summary>
		public void ApplyTo(string jpegFilePath)
		{
			var exif = new ExifInterface(jpegFilePath);
			foreach (var kvp in _tags)
			{
				try
				{
					exif.SetAttribute(kvp.Key, kvp.Value);
				}
				catch
				{
					// Skip any tag that can't be written on this platform version.
				}
			}

			exif.SetAttribute(ExifInterface.TagOrientation, Orientation.ToString());
			exif.SaveAttributes();
		}
	}
}
