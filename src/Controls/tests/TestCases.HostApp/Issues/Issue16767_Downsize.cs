using System.Reflection;
using Maui.Controls.Sample.Issues;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28725, "Downsize function in W2DImage class", PlatformAffected.UWP)]

public class Issue16767_DownSize : TestContentPage
{
	GraphicsView downSizeGraphicsView;
	Issue16767_DownsizeDrawable downSizeDrawable;

	protected override void Init()
	{
		var rootLayout = new VerticalStackLayout();

		downSizeGraphicsView = new GraphicsView()
		{
			BackgroundColor = Colors.Red,
			HeightRequest = 400,
			WidthRequest = 400
		};

		Label descriptionLabel = new Label()
		{
			Text = "The test should pass if the image is displayed and resized correctly. If the image is not displayed or not downsized correctly, the test has failed.",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "descriptionLabel"
		};

		downSizeDrawable = new Issue16767_DownsizeDrawable();
		downSizeGraphicsView.Drawable = downSizeDrawable;
		rootLayout.Add(downSizeGraphicsView);

		rootLayout.Add(descriptionLabel);
		Content = rootLayout;
	}
}

public class Issue16767_DownsizeDrawable : IDrawable
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
			float spacing = 20;
			float currentY = 0;

			canvas.FontColor = Colors.Black;
			canvas.FontSize = 16;

			// Label before first image
			canvas.DrawString("Downsize (100, 200)", 0, currentY, dirtyRect.Width, 30, HorizontalAlignment.Left, VerticalAlignment.Top);
			currentY += 30;

			var downsized1 = image.Downsize(100, 200);
			canvas.SetFillImage(downsized1);
			canvas.FillRectangle(0, currentY, 240, downsized1.Height);
			currentY += downsized1.Height + spacing;

			// Label before second image
			canvas.DrawString("Downsize (100)", 0, currentY, dirtyRect.Width, 30, HorizontalAlignment.Left, VerticalAlignment.Top);
			currentY += 30;

			var downsized2 = image.Downsize(100);
			canvas.SetFillImage(downsized2);
			canvas.FillRectangle(0, currentY, 240, downsized2.Height);
		}
	}
}
