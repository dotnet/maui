using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Handlers;

public class ImageHandler2 : ImageHandler
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