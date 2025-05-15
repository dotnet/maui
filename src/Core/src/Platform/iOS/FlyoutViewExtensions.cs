using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

internal static class FlyoutViewExtensions
{
	internal static UIImage? AutoResizeFlyoutIcon(UIImage image)
	{
		if (image == null || image.Size.Width <= 0 || image.Size.Height <= 0)
		{
			return null;
		}

		CGSize newSize = image.Size;

		if (image.Size.Width > image.Size.Height) //Wide
		{
			newSize.Width = 24;
			newSize.Height = newSize.Width * image.Size.Height / image.Size.Width;
		}
		else if (image.Size.Width < image.Size.Height) //Tall
		{
			newSize.Height = 24;
			newSize.Width = newSize.Height * image.Size.Width / image.Size.Height;
		}
		else //Square
		{
			newSize.Width = 24;
			newSize.Height = newSize.Width;
		}

		UIImage? resizedImage = null;
		try
		{
			UIGraphics.BeginImageContextWithOptions(newSize, false, 0);
			image.Draw(new CGRect(0, 0, newSize.Width, newSize.Height));
			resizedImage = UIGraphics.GetImageFromCurrentImageContext();
		}
		finally
		{
			UIGraphics.EndImageContext();
		}


		return resizedImage;
	}
}