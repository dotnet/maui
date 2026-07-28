using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

#if !MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Platform
{
	/// <summary>
	/// Windows <see cref="IImageMetadata"/> implementation. Captures a curated set of EXIF/photo
	/// properties (and the EXIF orientation) from the source image so they can be re-embedded when
	/// the image is saved as JPEG.
	/// </summary>
	class WindowsImageMetadata : IImageMetadata
	{
		// The Windows Property System name for the EXIF orientation flag.
		const string OrientationKey = "System.Photo.Orientation";

		// A curated set of common, writable EXIF/photo properties captured on load and restored on
		// save. GPS and other complex multi-value properties are intentionally omitted because they
		// do not round-trip reliably through the Windows Imaging Component.
		static readonly string[] CapturedKeys =
		{
			"System.Photo.DateTaken",
			"System.Photo.CameraManufacturer",
			"System.Photo.CameraModel",
			"System.Photo.ExposureTime",
			"System.Photo.FNumber",
			"System.Photo.FocalLength",
			"System.Photo.ISOSpeed",
			"System.Photo.Flash",
			"System.Photo.ExposureBias",
			"System.Photo.ShutterSpeed",
			"System.Photo.Aperture",
			"System.Photo.WhiteBalance",
		};

		readonly List<KeyValuePair<string, BitmapTypedValue>> _properties;

		WindowsImageMetadata(List<KeyValuePair<string, BitmapTypedValue>> properties, ushort orientation)
		{
			_properties = properties;
			Orientation = orientation;
		}

		/// <summary>
		/// The EXIF orientation (1-8) to write when saving. Set to 1 once the pixels have been rotated
		/// upright so a viewer doesn't rotate the already-upright image again.
		/// </summary>
		public ushort Orientation { get; set; }

		/// <summary>
		/// Captures the orientation plus a curated set of EXIF/photo properties from the decoder.
		/// </summary>
		public static async Task<WindowsImageMetadata> CaptureAsync(BitmapDecoder decoder)
		{
			ushort orientation = 1;

			// Orientation is read on its own so a failure here doesn't drop the rest.
			try
			{
				var orientationProps = await decoder.BitmapProperties.GetPropertiesAsync(new[] { OrientationKey });
				if (orientationProps.TryGetValue(OrientationKey, out var value) && value is not null && value.Value is ushort o)
					orientation = o;
			}
			catch
			{
				// No orientation available; assume upright.
			}

			// Each property is requested individually so an unsupported key doesn't fail the whole
			// set (GetPropertiesAsync is all-or-nothing for the keys it is given).
			var captured = new List<KeyValuePair<string, BitmapTypedValue>>();
			foreach (var key in CapturedKeys)
			{
				try
				{
					var props = await decoder.BitmapProperties.GetPropertiesAsync(new[] { key });
					if (props.TryGetValue(key, out var value) && value?.Value is not null)
						captured.Add(new KeyValuePair<string, BitmapTypedValue>(key, value));
				}
				catch
				{
					// Property not supported/available for this image; skip it.
				}
			}

			return new WindowsImageMetadata(captured, orientation);
		}

		/// <summary>
		/// Writes the captured properties (with the current <see cref="Orientation"/>) to a transcoding
		/// encoder. Falls back to writing only the orientation if the full set is rejected.
		/// </summary>
		public async Task ApplyToAsync(BitmapProperties encoderProperties)
		{
			// Build a native WinRT BitmapPropertySet (rather than a managed generic collection) so it can
			// be marshalled across the WinRT ABI without requiring AOT/unsafe code generation.
			var orientationValue = new BitmapTypedValue(Orientation, PropertyType.UInt16);

			var toSet = new BitmapPropertySet { { OrientationKey, orientationValue } };
			foreach (var property in _properties)
				toSet[property.Key] = property.Value;

			try
			{
				// SetPropertiesAsync is all-or-nothing; a single unsupported property fails the whole call.
				await encoderProperties.SetPropertiesAsync(toSet);
			}
			catch
			{
				// If the full set is rejected, at least preserve the orientation.
				try
				{
					var orientationOnly = new BitmapPropertySet { { OrientationKey, orientationValue } };
					await encoderProperties.SetPropertiesAsync(orientationOnly);
				}
				catch
				{
					// Metadata is best-effort; give up silently.
				}
			}
		}
	}
}
#endif
