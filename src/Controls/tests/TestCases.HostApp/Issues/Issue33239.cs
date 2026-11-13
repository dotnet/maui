#if ANDROID
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33239, "Unexpected high Image ByteCount when loading an image from resources using ImageSource", PlatformAffected.Android)]
public class Issue33239 : ContentPage
{
    Label _fileImageLabel;
    Label _resourceImageLabel;
    Image _resourceImage;
    Image _fileImage;

    public Issue33239()
    {
        var instructionLabel = new Label
        {
            Text = "Tap 'Measure ByteCount' to compare memory usage between stream-loaded (FromResource) and file-loaded (Source=\"royals.png\") images. Both should have similar byte counts after the fix.",
            Margin = new Thickness(10)
        };

        // Image loaded via stream/resource path (the broken path before fix)
        _resourceImage = new Image
        {
            Source = ImageSource.FromResource("Controls.TestCases.HostApp.Resources.Images.royals.png", typeof(Issue33239).Assembly),
            HeightRequest = 200,
            Margin = new Thickness(10),
            AutomationId = "ResourceImage"
        };

        // Image loaded via file path (always works correctly)
        _fileImage = new Image
        {
            Source = "royals.png",
            HeightRequest = 200,
            Margin = new Thickness(10),
            AutomationId = "FileImage"
        };

        var measureButton = new Microsoft.Maui.Controls.Button
        {
            Text = "Measure ByteCount",
            AutomationId = "MeasureButton",
            Margin = new Thickness(10)
        };
        measureButton.Clicked += OnMeasureClicked;

        _fileImageLabel = new Label
        {
            AutomationId = "FileImageLabel",
            Margin = new Thickness(10),
            LineBreakMode = LineBreakMode.WordWrap
        };

        _resourceImageLabel = new Label
        {
            AutomationId = "ResourceImageLabel",
            Margin = new Thickness(10),
            LineBreakMode = LineBreakMode.WordWrap
        };

        Content = new StackLayout
        {
            Children = { instructionLabel, _resourceImage, _fileImage, measureButton, _resourceImageLabel, _fileImageLabel }
        };
    }

    private void OnMeasureClicked(object sender, EventArgs e)
    {
#if ANDROID
        int resourceByteCount = GetBitmapByteCount(_resourceImage);
        int fileByteCount = GetBitmapByteCount(_fileImage);

        _resourceImageLabel.Text = $"ByteCount:{resourceByteCount}";
        _fileImageLabel.Text = $"ByteCount:{fileByteCount}";
#endif
    }

#if ANDROID
    static int GetBitmapByteCount(Image image)
    {
        if (image?.Handler?.PlatformView is not ImageView imageView)
            return 0;

        if (imageView.Drawable is BitmapDrawable drawable && drawable.Bitmap != null)
            return drawable.Bitmap.ByteCount;

        // Fallback: render into a bitmap
        if (imageView.Width > 0 && imageView.Height > 0 && Bitmap.Config.Argb8888 != null)
        {
            using var bitmap = Bitmap.CreateBitmap(imageView.Width, imageView.Height, Bitmap.Config.Argb8888);
            if (bitmap != null)
            {
                var canvas = new Canvas(bitmap);
                imageView.Draw(canvas);
                return bitmap.ByteCount;
            }
        }

        return 0;
    }
#endif
}
