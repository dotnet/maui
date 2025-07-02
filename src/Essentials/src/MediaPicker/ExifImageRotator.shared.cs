#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// Utility class for handling image rotation based on EXIF orientation data.
	/// This is a cross-platform shared interface and implementation details.
	/// </summary>
	internal static partial class ExifImageRotator
	{
		/// <summary>
		/// Determines if the image needs rotation based on the provided options.
		/// </summary>
		/// <param name="options">The media picker options.</param>
		/// <returns>True if rotation is needed based on the provided options.</returns>
		public static bool IsRotationNeeded(MediaPickerOptions? options)
		{
			return options?.RotateImage ?? false;
		}

		/// <summary>
		/// Processes an image stream by applying rotation based on EXIF data if needed.
		/// Each platform will provide its own implementation of this method.
		/// </summary>
		/// <param name="inputStream">The input image stream.</param>
		/// <param name="originalFileName">The original filename to determine format preservation logic.</param>
		/// <returns>A new stream containing the processed image with proper orientation.</returns>
		public static partial Task<Stream> RotateImageAsync(Stream inputStream, string originalFileName);
	}
}
