using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Handlers;

// TODO: make it public in .net 11
internal class ImageHandler2 : ImageHandler
{
    protected override ShapeableImageView CreatePlatformView()
    {
        var imageView = new ShapeableImageView(MauiMaterialContextThemeWrapper.Create(Context));

        // Enable view bounds adjustment on measure.
        // This allows the ImageView's OnMeasure method to account for the image's intrinsic
        // aspect ratio during measurement, which gives us more useful values during constrained
        // measurement passes.
        imageView.SetAdjustViewBounds(true);

        return imageView;
    }
}