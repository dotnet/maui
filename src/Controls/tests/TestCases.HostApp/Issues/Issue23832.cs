using Microsoft.Maui.Graphics.Platform;
using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23832, "Some HEIC photos are upside down after using PlatformImage.Resize", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue23832 : ContentPage
{
    public Issue23832()
    {
        VerticalStackLayout stackLayout = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 25,
        };

        Label labelResize = new Label
        {
            AutomationId = "DrawableLabel",
            Text = "The test passes if the image is displayed correctly without being upside down.",
        };

        GraphicsView graphicsView = new GraphicsView
        {
            Drawable = new Issue23832_Drawable(),
            HeightRequest = 300,
            WidthRequest = 400,
        };

        stackLayout.Children.Add(graphicsView);
        stackLayout.Children.Add(labelResize);
        Content = stackLayout;
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