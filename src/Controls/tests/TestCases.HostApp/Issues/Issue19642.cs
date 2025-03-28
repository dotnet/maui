using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;
using Microsoft.Maui.Graphics.Platform;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 19642, "NullReferenceException when using ImagePaint on Mac/iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue19642 : TestContentPage
{
	Issue19642_ImagePaintDrawable _drawable;
	VerticalStackLayout rootLayout;
	Label _descriptionLabel;

	protected override void Init()
	{
		rootLayout = new VerticalStackLayout();
		GraphicsView graphicsView = new GraphicsView() { HeightRequest = 500, WidthRequest = 400 };
		_drawable = new Issue19642_ImagePaintDrawable();
		_descriptionLabel = new Label() { AutomationId = "TestLabel", Text = "The image should be displayed and should not be inverted. If the image is inverted or not displayed, the test has failed." };

		graphicsView.Drawable = _drawable;
		rootLayout.Add(graphicsView);
		rootLayout.Add(_descriptionLabel);
		Content = rootLayout;
	}
}

internal class Issue19642_ImagePaintDrawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		IImage image;
		var assembly = GetType().GetTypeInfo().Assembly;
		using (var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png"))
		{
			image = PlatformImage.FromStream(stream);
		}

		if (image is not null)
		{
			canvas.SetFillImage(image.Downsize(100));
			canvas.FillRectangle(0, 0, 240, 300);
		}
	}
}