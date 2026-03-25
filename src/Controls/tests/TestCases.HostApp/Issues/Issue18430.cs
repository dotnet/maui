using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 18430, "CanvasDrawingSession Exception caused on Windows", PlatformAffected.UWP)]
public class Issue18430 : ContentPage
{
	public Issue18430()
	{
		var label = new Label
		{
			Text = "Test should pass only if no exception is thrown and the image should clipped",
			AutomationId = "Issue18430DescriptionLabel",

		};

		var graphicsView = new GraphicsView
		{
			HeightRequest = 300,
			WidthRequest = 400,
			Drawable = new Issue18430ClippingDrawable()
		};

		var layout = new VerticalStackLayout
		{
			Children =
			{
				label,
				graphicsView
			}
		};

		Content = new ScrollView { Content = layout };
	}
}

public class Issue18430ClippingDrawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		IImage image;
		var assembly = GetType().GetTypeInfo().Assembly;
		using (var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png"))
		{
			image = PlatformImage.FromStream(stream);
		}

		if (image != null)
		{
			float imageX = 10;
			float imageY = 10;

			float circleCenterX = imageX + image.Width / 2;
			float circleCenterY = imageY + image.Height / 2;
			float radius = Math.Min(image.Width, image.Height) / 2;

			PathF path = new PathF();
			path.AppendCircle(circleCenterX, circleCenterY, radius);

			canvas.ClipPath(path);
			canvas.DrawImage(image, imageX, imageY, image.Width, image.Height);
		}
	}
}