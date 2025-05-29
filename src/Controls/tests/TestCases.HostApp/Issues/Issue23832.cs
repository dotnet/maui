using Microsoft.Maui.Graphics.Platform;
using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23832, "Some HEIC photos is upside down after using PlatformImage.Resize", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue23832 : ContentPage
{
    public Issue23832()
    {
        VerticalStackLayout _stackLayout = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 25,
        };

        Label _labelResize = new Label
        {
            AutomationId = "DrawableLabel",
            Text = "The test passes if the image is displayed correctly without being upside down.",
        };

        GraphicsView _graphicsView = new GraphicsView
        {
            Drawable = new Issue23832_Drawable(),
            HeightRequest = 300,
            WidthRequest = 400,
        };

        _stackLayout.Children.Add(_graphicsView);
        _stackLayout.Children.Add(_labelResize);
        Content = _stackLayout;
    }
}

internal class Issue23832_Drawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        IImage image;
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        using (Stream stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.img_0111.heic"))
        {
            image = PlatformImage.FromStream(stream);
        }

        if (image is not null)
        {
            canvas.DrawImage(image, 0, 0, 300, 300);
        }
    }
}