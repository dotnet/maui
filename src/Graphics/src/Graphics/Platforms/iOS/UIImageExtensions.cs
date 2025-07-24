using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class UIImageExtensions
	{
		public static UIImage ScaleImage(this UIImage target, float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
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