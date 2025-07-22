namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30649, "GraphicsView event handlers should respect IsEnabled property", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue30649 : TestContentPage
{
	protected override void Init()
	{
		var eventCountLabel = new Label
		{
			AutomationId = "EventCountLabel",
			Text = "Touch Events: 0",
			HorizontalOptions = LayoutOptions.Center
		};

		var touchCount = 0;

		var graphicsView = new GraphicsView
		{
			AutomationId = "TestGraphicsView",
			HeightRequest = 200,
			WidthRequest = 300,
			BackgroundColor = Colors.LightBlue,
			Drawable = new Issue34Drawable(),
			HorizontalOptions = LayoutOptions.Center
		};

		// Add event handlers to track touch interactions
		graphicsView.StartInteraction += (s, e) =>
		{
			touchCount++;
			eventCountLabel.Text = $"Touch Events: {touchCount}";
		};

		var toggleButton = new Button
		{
			AutomationId = "ToggleIsEnabledButton",
			Text = "Disable GraphicsView",
			HorizontalOptions = LayoutOptions.Center
		};

		toggleButton.Clicked += (s, e) =>
		{
			graphicsView.IsEnabled = !graphicsView.IsEnabled;
			toggleButton.Text = graphicsView.IsEnabled ? "Disable GraphicsView" : "Enable GraphicsView";
		};

		var resetButton = new Button
		{
			AutomationId = "ResetCountButton",
			Text = "Reset Count",
			HorizontalOptions = LayoutOptions.Center
		};

		resetButton.Clicked += (s, e) =>
		{
			touchCount = 0;
			eventCountLabel.Text = $"Touch Events: {touchCount}";
		};

		var instructionsLabel = new Label
		{
			Text = "1. Tap the blue GraphicsView area - events should be counted\n" +
				   "2. Tap 'Disable GraphicsView'\n" +
				   "3. Tap the blue area again - no events should be counted\n" +
				   "4. Tap 'Enable GraphicsView'\n" +
				   "5. Tap the blue area - events should be counted again",
			FontSize = 12,
			Margin = new Thickness(10)
		};

		Content = new StackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children =
			{
				instructionsLabel,
				graphicsView,
				eventCountLabel,
				toggleButton,
				resetButton
			}
		};
	}
}

public class Issue34Drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.FillColor = Colors.LightBlue;
		canvas.FillRectangle(dirtyRect);
		
		canvas.StrokeColor = Colors.DarkBlue;
		canvas.StrokeSize = 2;
		canvas.DrawRectangle(dirtyRect);
		
		canvas.FontColor = Colors.DarkBlue;
		canvas.FontSize = 16;
		canvas.DrawString("Tap here to trigger events", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
	}
}