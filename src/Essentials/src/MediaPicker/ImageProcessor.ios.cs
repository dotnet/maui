#nullable enable
using System.IO;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Essentials;

internal static partial class ImageProcessor
{
	public static partial async Task<Stream> RotateImageAsync(Stream inputStream, string? originalFileName)
	{
		using var data = NSData.FromStream(inputStream);

		if (data is null)
		{
			return inputStream;
		}

		using var image = UIImage.LoadFromData(data);
		
		if (image?.CGImage is null)
		{
			return inputStream;
		}

		// Check if rotation is needed based on image orientation
		if (image.Orientation == UIImageOrientation.Up)
		{
			return inputStream;
		}

		// Create a new image with corrected orientation metadata (no pixel manipulation)
		// This preserves the original image data while fixing the display orientation
		var correctedImage = UIImage.FromImage(image.CGImage, image.CurrentScale, UIImageOrientation.Up);
		
		// Write the corrected image back to a stream, preserving original quality
		var outputStream = new MemoryStream();
		
		// Determine output format based on original file
		NSData? imageData = null;
		if (!string.IsNullOrEmpty(originalFileName))
		{
			var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
			if (extension == ".png")
			{
				imageData = correctedImage.AsPNG();
			}
			else
			{
				// For JPEG and other formats, use maximum quality (1.0)
				// People can downscale themselves through the MediaPickerOptions
				imageData = correctedImage.AsJPEG(1f);
			}
		}
		else
		{
			// Default to JPEG with maximum quality (1.0)
			imageData = correctedImage.AsJPEG(1f);
		}
		
		if (imageData is not null)
		{
			await imageData.AsStream().CopyToAsync(outputStream);
			outputStream.Position = 0;
			return outputStream;
		}

		return inputStream;
	}
}
