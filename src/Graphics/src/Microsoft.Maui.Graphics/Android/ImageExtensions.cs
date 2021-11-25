using Android.Graphics;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class ImageExtensions
	{
		public static Bitmap AsBitmap(this IImage image)
		{
			if (image is PlatformImage mdimage)
			{
				return mdimage.PlatformRepresentation;
			}

			if (image != null)
			{
				Logger.Warn($"MDImageExtensions.AsBitmap: Unable to get Bitmap from Image. Expected an image of type {nameof(PlatformImage)} however an image of type {0} was received.", image.GetType());
			}

			return null;
		}
	}
}
