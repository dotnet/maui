using System;
using CoreGraphics;
using SceneKit;
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
			if (!OperatingSystem.IsIOSVersionAtLeast(17))
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

			using (var renderer = new UIGraphicsImageRenderer(target.Size))
			{
				var resultImage = renderer.CreateImage((UIGraphicsImageRendererContext imageContext) =>
				{ 
					var cgcontext = imageContext.CGContext;
					cgcontext.DrawImage(new CGRect(CGPoint.Empty, size), target.CGImage);

					if (disposeOriginal)
					{
						target.Dispose();
					}			
				});

				return resultImage;
			}
		}

		public static UIImage NormalizeOrientation(this UIImage target, bool disposeOriginal = false)
		{
			if (target.Orientation == UIImageOrientation.Up)
			{
				return target;
			}

			if (!OperatingSystem.IsIOSVersionAtLeast(17))
			{
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

			using (var renderer = new UIGraphicsImageRenderer(target.Size))
			{
				var resultImage = renderer.CreateImage((UIGraphicsImageRendererContext imageContext) =>
				{ 
					var cgcontext = imageContext.CGContext;
					cgcontext.DrawImage(new CGRect(0, 0, target.Size.Width, target.Size.Height), target.CGImage);
					if (disposeOriginal)
					{
						target.Dispose();
					}
				});

				return resultImage;
			}			
		}
	}
}
