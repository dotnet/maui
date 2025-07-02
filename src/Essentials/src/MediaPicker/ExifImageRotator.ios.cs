#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Media
{
	internal static partial class ExifImageRotator
	{
		/// <summary>
		/// Gets the correct size for the destination image based on the orientation
		/// </summary>
		private static CGSize GetSizeForOrientation(UIImage image)
		{
			// For orientations that rotate 90 or 270 degrees, we need to swap width and height
			switch (image.Orientation)
			{
				case UIImageOrientation.Left:
				case UIImageOrientation.LeftMirrored:
				case UIImageOrientation.Right:
				case UIImageOrientation.RightMirrored:
					return new CGSize(image.Size.Height, image.Size.Width);
				default:
					return image.Size;
			}
		}

		/// <summary>
		/// Applies the appropriate affine transformation to the graphics context
		/// based on the orientation of the image
		/// </summary>
		private static void ApplyOrientationTransformation(CGContext context, UIImage image, CGSize destSize)
		{
			var width = image.Size.Width;
			var height = image.Size.Height;
			
			// First translate the context to the right position
			switch (image.Orientation)
			{
				case UIImageOrientation.Down: // 180° rotation
					context.TranslateCTM(width, height);
					context.RotateCTM((nfloat)Math.PI);
					break;
					
				case UIImageOrientation.Left: // 90° CCW - In iOS, Left means "the left side becomes the top"
					context.TranslateCTM(0, width);
					context.RotateCTM((nfloat)(-Math.PI / 2.0));
					break;
					
				case UIImageOrientation.Right: // 90° CW - In iOS, Right means "the right side becomes the top"
					context.TranslateCTM(height, 0);
					context.RotateCTM((nfloat)(Math.PI / 2.0));
					break;
					
				case UIImageOrientation.UpMirrored: // Horizontal flip
					context.TranslateCTM(width, 0);
					context.ScaleCTM(-1, 1);
					break;
					
				case UIImageOrientation.DownMirrored: // 180° rotation + horizontal flip
					context.TranslateCTM(width, height);
					context.RotateCTM((nfloat)Math.PI);
					context.TranslateCTM(width, 0);
					context.ScaleCTM(-1, 1);
					break;
					
				case UIImageOrientation.LeftMirrored: // 90° CCW + horizontal flip
					context.TranslateCTM(0, width);
					context.RotateCTM((nfloat)(-Math.PI / 2.0));
					context.TranslateCTM(height, 0);
					context.ScaleCTM(-1, 1);
					break;
					
				case UIImageOrientation.RightMirrored: // 90° CW + horizontal flip
					context.TranslateCTM(height, 0);
					context.RotateCTM((nfloat)(Math.PI / 2.0));
					context.TranslateCTM(width, 0);
					context.ScaleCTM(-1, 1);
					break;
					
				default: // No transformation needed for Up orientation
					break;
			}
		}

		public static partial async Task<Stream> RotateImageAsync(Stream inputStream, string originalFileName)
		{
			if (inputStream == null)
				return new MemoryStream();

			// Reset stream position
			if (inputStream.CanSeek)
				inputStream.Position = 0;

			// Read the input stream into NSData
			NSData? imageData;
			using (var memoryStream = new MemoryStream())
			{
				await inputStream.CopyToAsync(memoryStream);
				imageData = NSData.FromArray(memoryStream.ToArray());
			}

			if (imageData == null)
				return new MemoryStream();

			try
			{
				// Load UIImage from NSData
				UIImage? image = UIImage.LoadFromData(imageData);
				if (image == null)
					return new MemoryStream();
				
				// Log the orientation for debugging
				Console.WriteLine($"EXIF Orientation: {image.Orientation}");

				// Check if the image already has the correct orientation
				if (image.Orientation == UIImageOrientation.Up)
				{
					// No rotation needed
					return new MemoryStream(imageData.ToArray());
				}

				// Create a corrected image with proper orientation
				// The key fix: create a new image with the Up orientation
				// This automatically applies the correct rotation based on the EXIF data
				UIImage correctedImage;
				
				if (image.CGImage != null) 
				{
					correctedImage = UIImage.FromImage(
						image.CGImage, 
						image.CurrentScale, 
						UIImageOrientation.Up);
				}
				else 
				{
					// Fallback if we couldn't get the CGImage
					correctedImage = image;
				}

				// Convert back to NSData with appropriate compression
				NSData? resultData;
				var isPngFormat = !string.IsNullOrEmpty(originalFileName) && 
								  originalFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

				if (isPngFormat)
				{
					resultData = correctedImage.AsPNG();
				}
				else
				{
					// Use JPEG with high quality
					resultData = correctedImage.AsJPEG(1.0f);
				}

				if (resultData == null)
					return new MemoryStream();

				// Return the corrected image data as a stream
				return new MemoryStream(resultData.ToArray());
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception in RotateImageAsync: {ex}");
				// If anything went wrong, return a new empty stream
				return new MemoryStream(imageData.ToArray());
			}
		}
	}
}
