namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35752, "Android DragGestureRecognizer DragStarting fires prematurely on tap", PlatformAffected.Android)]
public class Issue35752 : TestContentPage
{
	protected override void Init()
	{
		var statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = "StatusLabel"
		};

		var dragCountLabel = new Label
		{
			Text = "0",
			AutomationId = "DragStartCount"
		};

		int dragStartCount = 0;

		var dragRecognizer = new DragGestureRecognizer();
		dragRecognizer.DragStarting += (s, e) =>
		{
			dragStartCount++;
			dragCountLabel.Text = dragStartCount.ToString();
			statusLabel.Text = "DragStarting fired";
		};

		var dragBox = new BoxView
		{
			HeightRequest = 100,
			WidthRequest = 200,
			BackgroundColor = Colors.Blue,
			AutomationId = "DragBox",
			GestureRecognizers = { dragRecognizer }
		};

		var instructions = new Label
		{
			Text = "Quick tap the blue box - DragStarting should NOT fire. " +
				   "Long press or drag it - DragStarting SHOULD fire.",
			AutomationId = "TestLoaded"
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children =
			{
				instructions,
				dragBox,
				new Label { Text = "Status:" },
				statusLabel,
				new Label { Text = "DragStart count:" },
				dragCountLabel
			}
		};
	}
}
