#nullable enable
using System;
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
		if (data == null)
			return inputStream;
			
		using var image = UIImage.LoadFromData(data);
		
		if (image?.CGImage == null)
			return inputStream;

		// Check if rotation is needed based on image orientation
		if (image.Orientation == UIImageOrientation.Up)
			return inputStream;

		// Get the size for the corrected orientation
		var (width, height) = GetSizeForOrientation(image);
		
		// Create a new image with correct orientation
		var renderer = new UIGraphicsImageRenderer(new CGSize(width, height));
		var correctedImage = renderer.CreateImage((context) =>
		{
			ApplyOrientationTransformation(context.CGContext, image, width, height);
			image.Draw(new CGRect(0, 0, image.Size.Width, image.Size.Height));
		});
		
		if (correctedImage != null && correctedImage.CGImage != null)
		{
			// Create image with Up orientation
			var finalImage = UIImage.FromImage(correctedImage.CGImage, correctedImage.CurrentScale, UIImageOrientation.Up);
			
			var outputStream = new MemoryStream();
			var imageData = finalImage.AsJPEG(1.0f);
			if (imageData != null)
			{
				await imageData.AsStream().CopyToAsync(outputStream);
				outputStream.Position = 0;
				
				return outputStream;
			}
		}

		return inputStream;
	}

	static (nfloat width, nfloat height) GetSizeForOrientation(UIImage image)
	{
		return image.Orientation switch
		{
			UIImageOrientation.Left or
			UIImageOrientation.LeftMirrored or
			UIImageOrientation.Right or
			UIImageOrientation.RightMirrored => (image.Size.Height, image.Size.Width),
			_ => (image.Size.Width, image.Size.Height)
		};
	}

	static void ApplyOrientationTransformation(CGContext context, UIImage image, nfloat width, nfloat height)
	{
		switch (image.Orientation)
		{
			case UIImageOrientation.Down:
			case UIImageOrientation.DownMirrored:
				context.TranslateCTM(width, height);
				context.RotateCTM((nfloat)Math.PI);
				break;

			case UIImageOrientation.Left:
			case UIImageOrientation.LeftMirrored:
				context.TranslateCTM(width, 0);
				context.RotateCTM((nfloat)(Math.PI / 2));
				break;

			case UIImageOrientation.Right:
			case UIImageOrientation.RightMirrored:
				context.TranslateCTM(0, height);
				context.RotateCTM((nfloat)(-Math.PI / 2));
				break;

			case UIImageOrientation.Up:
			case UIImageOrientation.UpMirrored:
				break;
		}

		switch (image.Orientation)
		{
			case UIImageOrientation.UpMirrored:
			case UIImageOrientation.DownMirrored:
				context.TranslateCTM(width, 0);
				context.ScaleCTM(-1, 1);
				break;

			case UIImageOrientation.LeftMirrored:
			case UIImageOrientation.RightMirrored:
				context.TranslateCTM(height, 0);
				context.ScaleCTM(-1, 1);
				break;
		}
	}
}
