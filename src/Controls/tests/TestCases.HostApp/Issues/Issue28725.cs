using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28725, "ImagePaint is not rendering in View", PlatformAffected.UWP)]
public class Issue28725 : ContentPage
{
	public Issue28725()
	{
		VerticalStackLayout _stackLayout = new VerticalStackLayout();
		GraphicsView _graphicsView = new GraphicsView
		{
			Drawable = new Issue28725_ImagePaint(),
			HeightRequest = 500,
			WidthRequest = 400
		};

		Label _label = new Label
		{
			AutomationId = "Label",
			Text = "The image should be rendered properly, otherwise the test fails"
		};

		_stackLayout.Children.Add(_graphicsView);
		_stackLayout.Children.Add(_label);
		Content = _stackLayout;
	}
}

public class Issue28725_ImagePaint : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		IImage image;
		Assembly assembly = GetType().GetTypeInfo().Assembly;
		using (Stream stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.groceries.png"))
		{
			image = PlatformImage.FromStream(stream);
		}

		if (image != null)
		{
			canvas.SetFillImage(image.Downsize(100));
			canvas.FillRectangle(0, 0, 240, 300);
		}
	}
}