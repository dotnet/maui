namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30649, "GraphicsView event handlers are triggered even when IsEnabled is set to False", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue30649 : ContentPage
{
	Label statusLabel;
	GraphicsView graphicsView;
	public Issue30649()
	{
		Label label = new Label
		{
			Text = "The test passes if GraphicsView event handlers are not triggered when IsEnabled is set to False.",
		};

		graphicsView = new GraphicsView
		{
			Drawable = new Issue30649_Drawable(),
			AutomationId = "Issue30649_GraphicsView",
			HeightRequest = 300,
			WidthRequest = 300,
			IsEnabled = false,
		};
		graphicsView.StartInteraction += OnStartInteraction;

		statusLabel = new Label
		{
			Text = "Success",
			AutomationId = "Issue30649_Label",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		Content = new StackLayout
		{
			Children =
			{
				label,
				graphicsView,
				statusLabel
			}
		};
	}

	private void OnStartInteraction(object sender, TouchEventArgs e)
	{
		statusLabel.Text = "Failure";
	}
}

public class Issue30649_Drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		// Draw a simple rectangle
		canvas.StrokeColor = Colors.Blue;
		canvas.FillColor = Colors.Gray;
		canvas.StrokeSize = 4;

		// Create rectangle that fills most of the available area
		float width = dirtyRect.Width * 0.8f;
		float height = dirtyRect.Height * 0.8f;
		float x = (dirtyRect.Width - width) / 2;
		float y = (dirtyRect.Height - height) / 2;

		// Draw the rectangle
		canvas.FillRectangle(x, y, width, height);
		canvas.DrawRectangle(x, y, width, height);
	}
}