using Android.Graphics;

namespace System.Graphics.Android
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
                Logger.Warn("MDImageExtensions.AsBitmap: Unable to get Bitmap from EWImage. Expected an image of type MDImage however an image of type {0} was received.", image.GetType());
            }

            return null;
        }
    }
}