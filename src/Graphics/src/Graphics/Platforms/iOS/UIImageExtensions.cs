using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class UIImageExtensions
	{
		public static UIImage ScaleImage(this UIImage target, float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			float originalWidth = (float)target.Size.Width;
			float originalHeight = (float)target.Size.Height;

			float scale = originalWidth / maxWidth;

			float targetWidth = originalWidth / scale;
			float targetHeight = originalHeight / scale;

			if (targetHeight > maxHeight)
			{
				scale = targetHeight / maxHeight;
				targetHeight = targetHeight / scale;
				targetWidth = targetWidth / scale;
			}

			return ScaleImage(target, new CGSize(targetWidth, targetHeight), disposeOriginal);
		}

		public static UIImage ScaleImage(this UIImage target, CGSize size, bool disposeOriginal = false)
		{
			UIGraphics.BeginImageContext(size);
			target.Draw(new CGRect(CGPoint.Empty, size));
			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			if (disposeOriginal)
			{
				target.Dispose();
			}

			return image;
		}

		public static UIImage NormalizeOrientation(this UIImage target, bool disposeOriginal = false)
		{
			if (target.Orientation == UIImageOrientation.Up)
			{
				return target;
			}

			UIGraphics.BeginImageContextWithOptions(target.Size, false, target.CurrentScale);
			target.Draw(CGPoint.Empty);
			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			if (disposeOriginal)
			{
				target.Dispose();
			}

			return image;
		}

		internal static UIImage ResizeImageSource(this UIImage sourceImage, nfloat maxWidth, nfloat maxHeight, CGSize originalImageSize, bool shouldScaleUp = false)
		{
			if (sourceImage is null || sourceImage.CGImage is null)
			{
				return null;
			}

			maxWidth = (nfloat)Math.Min(maxWidth, originalImageSize.Width);
			maxHeight = (nfloat)Math.Min(maxHeight, originalImageSize.Height);

			var sourceSize = sourceImage.Size;

			float maxResizeFactor = (float)Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1 && !shouldScaleUp)
			{
				return sourceImage;
			}

			return UIImage.FromImage(sourceImage.CGImage, sourceImage.CurrentScale / maxResizeFactor, sourceImage.Orientation);
		}
	}
}
