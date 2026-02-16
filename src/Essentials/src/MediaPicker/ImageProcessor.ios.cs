#nullable enable
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using ImageIO;
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

		// Create a graphics context with the rotated size and draw the rotated image
		// This ensures the pixels are actually rotated, not just the orientation flag
		var size = image.Size;
		
		// Determine the size after rotation
		var rotatedSize = image.Orientation switch
		{
			UIImageOrientation.Left or UIImageOrientation.Right or
			UIImageOrientation.LeftMirrored or UIImageOrientation.RightMirrored
				=> new CGSize(size.Height, size.Width),
			_ => size
		};

		// Create a graphics context with the rotated dimensions
		UIGraphics.BeginImageContextWithOptions(rotatedSize, false, image.CurrentScale);
		
		try
		{
			var context = UIGraphics.GetCurrentContext();
			
			// Apply the appropriate transformation based on orientation
			switch (image.Orientation)
			{
				case UIImageOrientation.Right:
					context.TranslateCTM(rotatedSize.Width, 0);
					context.RotateCTM((nfloat)System.Math.PI / 2);
					break;
				case UIImageOrientation.Down:
					context.TranslateCTM(rotatedSize.Width, rotatedSize.Height);
					context.RotateCTM((nfloat)System.Math.PI);
					break;
				case UIImageOrientation.Left:
					context.TranslateCTM(0, rotatedSize.Height);
					context.RotateCTM(-(nfloat)System.Math.PI / 2);
					break;
				case UIImageOrientation.UpMirrored:
					context.TranslateCTM(rotatedSize.Width, 0);
					context.ScaleCTM(-1, 1);
					break;
				case UIImageOrientation.RightMirrored:
					context.TranslateCTM(rotatedSize.Width, rotatedSize.Height);
					context.RotateCTM((nfloat)System.Math.PI / 2);
					context.ScaleCTM(-1, 1);
					break;
				case UIImageOrientation.DownMirrored:
					context.TranslateCTM(0, rotatedSize.Height);
					context.ScaleCTM(1, -1);
					break;
				case UIImageOrientation.LeftMirrored:
					context.TranslateCTM(0, 0);
					context.RotateCTM(-(nfloat)System.Math.PI / 2);
					context.ScaleCTM(-1, 1);
					break;
				// UIImageOrientation.Up: no transformation needed
			}
			
			// Draw the image
			image.Draw(CGPoint.Empty);
			
			// Get the rotated image
			var rotatedImage = UIGraphics.GetImageFromCurrentImageContext();
			
			if (rotatedImage == null)
			{
				return inputStream;
			}
			
			// Write the rotated image back to a stream, preserving original quality
			var outputStream = new MemoryStream();
			
			// Determine output format based on original file
			NSData? imageData = null;
			if (!string.IsNullOrEmpty(originalFileName))
			{
				var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
				if (extension == ".png")
				{
					imageData = rotatedImage.AsPNG();
				}
				else
				{
					// For JPEG and other formats, use maximum quality (1.0)
					imageData = rotatedImage.AsJPEG(1f);
				}
			}
			else
			{
				// Default to JPEG with maximum quality (1.0)
				imageData = rotatedImage.AsJPEG(1f);
			}
			
			if (imageData is not null)
			{
				await imageData.AsStream().CopyToAsync(outputStream);
				outputStream.Position = 0;
				return outputStream;
			}
			
			return inputStream;
		}
		finally
		{
			UIGraphics.EndImageContext();
		}
	}

	public static partial Task<byte[]?> ExtractMetadataAsync(Stream inputStream, string? originalFileName)
	{
		if (inputStream == null)
			return Task.FromResult<byte[]?>(null);

		try
		{
			using var data = NSData.FromStream(inputStream);
			if (data == null)
				return Task.FromResult<byte[]?>(null);

			using var source = CGImageSource.FromData(data);
			if (source == null)
				return Task.FromResult<byte[]?>(null);

			// Get metadata from the first image
			var metadata = source.CopyProperties((NSDictionary?)null, 0);
			if (metadata == null)
				return Task.FromResult<byte[]?>(null);

			// Convert metadata to binary plist data
			NSError? error;
			var plistData = NSPropertyListSerialization.DataWithPropertyList(metadata, NSPropertyListFormat.Binary, 0, out error);
			if (plistData == null || error != null)
				return Task.FromResult<byte[]?>(null);

			return Task.FromResult<byte[]?>(plistData.ToArray());
		}
		catch
		{
			return Task.FromResult<byte[]?>(null);
		}
	}

	public static partial Task<Stream> ApplyMetadataAsync(Stream processedStream, byte[] metadata, string? originalFileName)
	{
		if (processedStream == null || metadata == null || metadata.Length == 0)
			return Task.FromResult(processedStream ?? new MemoryStream());

		try
		{
			using var processedData = NSData.FromStream(processedStream);
			if (processedData == null)
				return Task.FromResult(processedStream);

			using var source = CGImageSource.FromData(processedData);
			if (source == null)
				return Task.FromResult(processedStream);

			// Restore metadata from NSData
			using var metadataNSData = NSData.FromArray(metadata);
			NSPropertyListFormat format = NSPropertyListFormat.Binary;
			NSError? error;
			var restoredMetadata = NSPropertyListSerialization.PropertyListWithData(metadataNSData, NSPropertyListReadOptions.Immutable, ref format, out error) as NSDictionary;
			if (restoredMetadata == null || error != null)
				return Task.FromResult(processedStream);

			// Create mutable data for output
			var outputData = NSMutableData.FromCapacity(0);
			if (outputData == null)
				return Task.FromResult(processedStream);

			// Determine UTI based on original filename
			string uti = "public.jpeg"; // Default to JPEG
			if (!string.IsNullOrEmpty(originalFileName))
			{
				var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
				if (ext == ".png")
					uti = "public.png";
			}

			// Create destination with metadata
			using var destination = CGImageDestination.Create(outputData, uti, 1);
			if (destination == null)
				return Task.FromResult(processedStream);

			// Add image with preserved metadata
			using var image = source.CreateImage(0, new CGImageOptions());
			if (image != null)
			{
				destination.AddImage(image, restoredMetadata);
				destination.Close();

				return Task.FromResult<Stream>(outputData.AsStream());
			}

			return Task.FromResult(processedStream);
		}
		catch
		{
			return Task.FromResult(processedStream);
		}
	}
}
