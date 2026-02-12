namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24252, "PanGestureRecognizer behaves differently on Windows to other platforms", PlatformAffected.UWP)]
public class Issue24252 : ContentPage
{
	public Issue24252()
	{
		var statusLabel = new Label
		{
			Text = "None",
			AutomationId = "StatusLabel"
		};

		// Parent: large blue area with PanGestureRecognizer
		var parent = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			WidthRequest = 300,
			HeightRequest = 300,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var parentPan = new PanGestureRecognizer();
		parentPan.PanUpdated += (s, e) =>
		{
			if (e.StatusType == GestureStatus.Started)
			{
				statusLabel.Text = "Parent triggered";
			}
		};
		parent.GestureRecognizers.Add(parentPan);

		var child = new Image
		{
			Source = "dotnet_bot.png",
			WidthRequest = 100,
			HeightRequest = 100,
			BackgroundColor = Colors.Orange,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "ChildImage"
		};

		var childPan = new PanGestureRecognizer();
		childPan.PanUpdated += (s, e) =>
		{
			if (e.StatusType == GestureStatus.Started)
			{
				statusLabel.Text = "Child triggered";
			}
		};
		child.GestureRecognizers.Add(childPan);

		parent.Children.Add(child);

		Content = new VerticalStackLayout
		{
			Children = { statusLabel, parent }
		};
	}
}