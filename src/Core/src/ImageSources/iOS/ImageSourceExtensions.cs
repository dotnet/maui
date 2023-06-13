using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static partial class ImageSourceExtensions
	{
		internal static UIImage? GetPlatformImage(this IFontImageSource imageSource, IFontManager fontManager, float scale)
		{
			var font = fontManager.GetFont(imageSource.Font);
			var color = (imageSource.Color ?? Colors.White).ToPlatform();
			var glyph = (NSString)imageSource.Glyph;
#pragma warning disable CS8604
			var attString = new NSAttributedString(glyph, font, color);
			var imagesize = glyph.GetSizeUsingAttributes(attString.GetUIKitAttributes(0, out _)!);
#pragma warning restore CS8604
			UIGraphics.BeginImageContextWithOptions(imagesize, false, scale);
			var ctx = new NSStringDrawingContext();

			var boundingRect = attString.GetBoundingRect(imagesize, 0, ctx);
			attString.DrawString(new CGRect(
				imagesize.Width / 2 - boundingRect.Size.Width / 2,
				imagesize.Height / 2 - boundingRect.Size.Height / 2,
				imagesize.Width,
				imagesize.Height));

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
		}

		internal static UIImage? GetPlatformImage(this IFileImageSource imageSource)
		{
			var filename = imageSource.File;
			return File.Exists(filename)
						? UIImage.FromFile(filename)
						: UIImage.FromBundle(filename);
		}
	}
}
