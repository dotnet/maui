#if ANDROID
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33239, "Unexpected high Image ByteCount when loading an image from resources using ImageSource", PlatformAffected.Android)]
public class Issue33239 : ContentPage
{
    Label _resultLabel;
    Image image;
    public Issue33239()
    {
        var instructionLabel = new Label
        {
            Text = "Tap 'Measure ByteCount' to verify the image memory usage is reasonable (not excessive).",
            Margin = new Thickness(10)
        };

        image = new Image
        {
            Source = ImageSource.FromResource("Controls.TestCases.HostApp.Resources.Images.royals.png", typeof(Issue33239).Assembly),
            HeightRequest = 200,
            Margin = new Thickness(10),
            AutomationId = "TestImage"
        };

        var measureButton = new Microsoft.Maui.Controls.Button
        {
            Text = "Measure ByteCount",
            AutomationId = "MeasureButton",
            Margin = new Thickness(10)
        };
        measureButton.Clicked += OnMeasureClicked;

        _resultLabel = new Label
        {
            AutomationId = "ResultLabel",
            Margin = new Thickness(10),
            LineBreakMode = LineBreakMode.WordWrap
        };

        Content = new StackLayout
        {
            Children = { instructionLabel, image, measureButton, _resultLabel }
        };
    }

    private void OnMeasureClicked(object sender, EventArgs e)
    {
#if ANDROID
        Android.Graphics.Bitmap processedBitmap = null;
        if ((image?.Handler?.PlatformView is ImageView imageView))
        {
            if (imageView.Drawable is BitmapDrawable drawable)
            {
                processedBitmap = drawable.Bitmap?.Copy(drawable.Bitmap.GetConfig(), true);
            }

            if (processedBitmap == null && Bitmap.Config.Argb8888 != null)
            {
                processedBitmap = Bitmap.CreateBitmap(imageView.Width, imageView.Height, Bitmap.Config.Argb8888);
                if (processedBitmap != null)
                {
                    Canvas canvas = new Canvas(processedBitmap);
                    imageView.Draw(canvas);
                }
            }
        }
        _resultLabel.Text = $"Image ByteCount: {processedBitmap?.ByteCount ?? 0}";
#endif
    }
}
