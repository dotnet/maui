using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class UIImageExtensions
	{
		public static UIImage ScaleImage(this UIImage target, float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			if (maxWidth <= 0 || maxHeight <= 0)
			{
				return target;
			}

			if (target.Size.Width > maxWidth || target.Size.Height > maxHeight)
			{
				float factorX = maxWidth / (float)target.Size.Width;
				float factorY = maxHeight / (float)target.Size.Height;

				float factor = Math.Min(factorX, factorY);

				float targetWidth = factor * (float)target.Size.Width;
				float targetHeight = factor * (float)target.Size.Height;

				return ScaleImage(target, new CGSize(targetWidth, targetHeight), disposeOriginal);
			}

			return target;
		}

		internal static UIImage ResizeImageSource(this UIImage sourceImage, nfloat maxWidth, nfloat maxHeight, CGSize originalImageSize, bool shouldScaleUp = false)
		{
			if (sourceImage?.CGImage is null)
			{
				return null;
			}

			maxWidth = (nfloat)Math.Min(maxWidth, originalImageSize.Width);
			maxHeight = (nfloat)Math.Min(maxHeight, originalImageSize.Height);

			var sourceSize = sourceImage.Size;

			float maxResizeFactor = (float)Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1 && !shouldScaleUp)
				return sourceImage;

			var resizedImage = UIImage.FromImage(sourceImage.CGImage, sourceImage.CurrentScale / maxResizeFactor, sourceImage.Orientation);

			// Preserve the rendering mode to maintain color behavior
			resizedImage = resizedImage.ImageWithRenderingMode(sourceImage.RenderingMode);

			return resizedImage;
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

			var renderer = new UIGraphicsImageRenderer(target.Size, new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = target.CurrentScale,
			});

			var image = renderer.CreateImage((context) =>
			{
				target.Draw(CGPoint.Empty);
			});


			if (disposeOriginal)
			{
				target.Dispose();
			}

			return image;
		}
	}
}