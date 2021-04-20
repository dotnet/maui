using Android.Graphics;

namespace Microsoft.Maui.Graphics.Native
{
	public static class ImageExtensions
	{
		public static Bitmap AsBitmap(this IImage image)
		{
			if (image is NativeImage mdimage)
			{
				return mdimage.NativeRepresentation;
			}

			if (image != null)
			{
				Logger.Warn("MDImageExtensions.AsBitmap: Unable to get Bitmap from Image. Expected an image of type NativeImage however an image of type {0} was received.", image.GetType());
			}

			return null;
		}
	}
}
