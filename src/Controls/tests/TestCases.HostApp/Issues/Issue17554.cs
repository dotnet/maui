namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17554, "[Android] DragGestureRecognizer.DropCompleted event not firing", PlatformAffected.Android)]
public class Issue17554 : ContentPage
{
	public Issue17554()
	{
		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "No events fired"
		};

		var dragSource = new Label
		{
			Text = "Drag Me",
			AutomationId = "DragSource",
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 100,
			WidthRequest = 200,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};

		var dragGesture = new DragGestureRecognizer();
		dragGesture.DragStarting += (s, e) => statusLabel.Text = "DragStarting";
		dragGesture.DropCompleted += (s, e) => statusLabel.Text = "DropCompleted";
		dragSource.GestureRecognizers.Add(dragGesture);

		// This is intentionally a plain view with NO DropGestureRecognizer,
		// which is the scenario where Android fails to fire DropCompleted.
		var nonDropTarget = new Label
		{
			Text = "Drop here (no drop target)",
			AutomationId = "NonDropTarget",
			BackgroundColor = Colors.LightGray,
			HeightRequest = 100,
			WidthRequest = 200,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { dragSource, nonDropTarget, statusLabel }
		};
	}
}
