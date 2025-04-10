using System.Reflection;
using Maui.Controls.Sample.Issues;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28725, "ImagePaint is not rendering in View", PlatformAffected.UWP)]
public class Issue28725 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Padding = new Thickness(12, 24),
				Children =
				{
					CreateButton(ResizeMode.Fit),
					CreateButton(ResizeMode.Stretch),
					CreateButton(ResizeMode.Bleed),
				}
			}
		});
	}

	Button CreateButton(ResizeMode resizeMode)
	{
		var button = new Button
		{
			AutomationId = $"{resizeMode}Button",
			Text = resizeMode.ToString(),
		};
		button.Clicked += (s, e) => Navigation.PushAsync(new Issue28725_ContentPage(resizeMode));
		return button;
	}
}

public class Issue28725_ContentPage : TestContentPage, IDrawable
{
	ResizeMode _resizeMode;
	public Issue28725_ContentPage(ResizeMode resizeMode)
	{
		_resizeMode = resizeMode;
	}

	protected override void Init()
	{
		var rootLayout = new VerticalStackLayout();
		GraphicsView graphicsView = new GraphicsView()
		{
			HeightRequest = 500, 
			WidthRequest = 400
		};

		Label label = new Label
		{
			AutomationId = "Label",
			Text = "The test should pass if the image is displayed and resized correctly. If the image is not displayed or not resized correctly, the test has failed.",
		};

		Button button = new Button
		{
			Text = "Back",
			AutomationId = "BackButton"
		};
		button.Clicked += (s, e) => Navigation.PopAsync();

		graphicsView.Drawable = this;
		rootLayout.Add(graphicsView);
		rootLayout.Add(label);
		rootLayout.Add(button);
		Content = rootLayout;
	}

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

			var resized1 = image.Downsize(100, 200);
			canvas.SetFillImage(resized1);
			canvas.FillRectangle(0, currentY, 240, resized1.Height);
			currentY += resized1.Height + spacing;

			// Label before second image
			canvas.DrawString("Downsize (100)", 0, currentY, dirtyRect.Width, 30, HorizontalAlignment.Left, VerticalAlignment.Top);
			currentY += 30;

			var resized2 = image.Downsize(100);
			canvas.SetFillImage(resized2);
			canvas.FillRectangle(0, currentY, 240, resized2.Height);
			currentY += resized2.Height + spacing;

			// Label before third image
			canvas.DrawString($"Resize (100, 200) - Mode: {_resizeMode}", 0, currentY, dirtyRect.Width, 30, HorizontalAlignment.Left, VerticalAlignment.Top);
			currentY += 30;

			var resized3 = image.Resize(100, 200, _resizeMode);
			canvas.SetFillImage(resized3);
			canvas.FillRectangle(0, currentY, 240, resized3.Height);
			currentY += resized3.Height + spacing;

		}
	}
}