using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class ImageExtensions
	{
		public static UIImage AsUIImage(this IImage image)
		{
			if (image is PlatformImage platformImage)
			{
				return platformImage.PlatformRepresentation;
			}

			if (image != null)
			{
				Logger.Warn($"{nameof(ImageExtensions)}.{nameof(AsUIImage)}: Unable to get UIImage from {nameof(image)}. Expected an image of type {nameof(PlatformImage)} however an image of type {0} was received.", image.GetType());
			}

			return null;
		}
	}
}
