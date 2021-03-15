using UIKit;

namespace Microsoft.Maui.Graphics.CoreGraphics
{
    public static class NativeImageExtensions
    {
        public static UIImage AsUIImage(this IImage image)
        {
            if (image is NativeImage nativeImage)
            {
                return nativeImage.NativeRepresentation;
            }

            if (image != null)
            {
                Logger.Warn("MTImageExtensions.AsUIImage: Unable to get UIImage from NativeImage. Expected an image of type NativeImage however an image of type {0} was received.", image.GetType());
            }

            return null;
        }
    }
}