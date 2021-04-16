using AppKit;

namespace Microsoft.Maui.Graphics.Native
{
    public static class DrawableExtensions
    {
        public static NativeImage ToMultiResolutionImage(this IDrawable drawable, int width, int height)
        {
            if (drawable == null)
                return null;

            var normalImage = drawable.ToImage(width, height);
            var normalNativeImage = normalImage.AsNSImage();
            var normalImageData = normalNativeImage.AsTiff();
            var normalImageRep = (NSBitmapImageRep) NSBitmapImageRep.ImageRepFromData(normalImageData);

            var retinaImage = drawable.ToImage(width * 2, height * 2, 2);
            var retinaNativeImage = retinaImage.AsNSImage();
            var retinaImageData = retinaNativeImage.AsTiff();
            var retinaImageRep = (NSBitmapImageRep) NSBitmapImageRep.ImageRepFromData(retinaImageData);

            var combinedImage = new NSImage {MatchesOnMultipleResolution = true};
            combinedImage.AddRepresentations(new NSImageRep[] {normalImageRep, retinaImageRep});

            return new NativeImage(combinedImage);
        }
    }
}