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
            AutomationId = "ResizeLabel",
            Text = "Resize image"
        };

        GraphicsView _graphicsViewResize = new GraphicsView
        {
            Drawable = new Issue23832_ResizeDrawable(),
            HeightRequest = 300,
            WidthRequest = 400,
        };

        Label _labelDownsize = new Label
        {
            Text = "Resize image"
        };

        GraphicsView _graphicsViewDownsize = new GraphicsView
        {
            Drawable = new Issue23832_ResizeDrawable(),
            HeightRequest = 300,
            WidthRequest = 400,
        };

        _stackLayout.Children.Add(_labelResize);
        _stackLayout.Children.Add(_graphicsViewResize);
        _stackLayout.Children.Add(_labelDownsize);
        _stackLayout.Children.Add(_graphicsViewDownsize);
        Content = _stackLayout;
    }

}

internal class Issue23832_ResizeDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        IImage image;
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        using (Stream stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.img_0111.heic"))
        {
            image = PlatformImage.FromStream(stream);
        }

        if (image != null)
        {
            IImage newImage = image.Resize(400, 300, ResizeMode.Fit, false);
            canvas.DrawImage(newImage, 0, 0, newImage.Width, newImage.Height);
        }
    }
}

internal class Issue23832_DownsizeDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        IImage image;
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        using (Stream stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.img_0111.heic"))
        {
            image = PlatformImage.FromStream(stream);
        }

        if (image != null)
        {
            IImage newImage = image.Downsize(400, 300);
            canvas.DrawImage(newImage, 0, 0, newImage.Width, newImage.Height);
        }
    }
}
