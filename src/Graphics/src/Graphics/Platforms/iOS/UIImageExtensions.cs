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
			var format = new UIGraphicsImageRendererFormat
			{
				Opaque = false,
				Scale = target.CurrentScale,
			};

			using (var renderer = new UIGraphicsImageRenderer(size, format))
			{
				var resultImage = renderer.CreateImage((UIGraphicsImageRendererContext imageContext) =>
				{
					var cgcontext = imageContext.CGContext;

					// The image is drawn upside down because Core Graphics uses a bottom-left origin, 
					// whereas UIKit uses a top-left origin. Adjust the coordinate system to align with UIKit's top-left origin.
					cgcontext.TranslateCTM(0, (nfloat)size.Height);
					cgcontext.ScaleCTM(1, -1);
					cgcontext.DrawImage(new CGRect(CGPoint.Empty, size), target.CGImage);
					cgcontext.ScaleCTM(1, -1);
					cgcontext.TranslateCTM(0, -(nfloat)size.Height);

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