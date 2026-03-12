namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33110, "GraphicsView dirtyRect dimensions should be integers, not fractional values", PlatformAffected.UWP | PlatformAffected.Android)]
public class Issue33110 : ContentPage
{
	public Issue33110()
	{
		Label descriptionLabel = new Label
		{
			Text = "The GraphicsView below has a fixed size of 100x50. Tap 'Check' to verify that the dirtyRect dimensions passed to Draw are integer values.",
		};

		Label resultLabel = new Label
		{
			Text = "Pending",
			AutomationId = "ResultLabel"
		};

		Issue33110Drawable drawable = new Issue33110Drawable();

		GraphicsView graphicsView = new GraphicsView
		{
			Drawable = drawable,
			WidthRequest = 100,
			HeightRequest = 50,
			HorizontalOptions = LayoutOptions.Start,
		};

		Button checkButton = new Button
		{
			Text = "Check",
			AutomationId = "CheckButton",
			HorizontalOptions = LayoutOptions.Start,
		};

		checkButton.Clicked += (s, e) =>
			resultLabel.Text = drawable.HasIntegerDimensions ? "Pass" : "Fail";

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 12,
			Children =
			{
				descriptionLabel,
				graphicsView,
				checkButton,
				resultLabel
			}
		};
	}
}

class Issue33110Drawable : IDrawable
{
	public bool HasIntegerDimensions { get; set; }

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.FillColor = Colors.Blue;
		canvas.FillRectangle(dirtyRect);

		HasIntegerDimensions = Math.Abs(dirtyRect.Width - MathF.Round(dirtyRect.Width)) < 0.01f
							&& Math.Abs(dirtyRect.Height - MathF.Round(dirtyRect.Height)) < 0.01f;
	}
}
