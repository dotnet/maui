namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7580, "Changing visibility on a SwipeItem causes multiple items to be executed", PlatformAffected.All)]
public class Issue7580 : ContentPage
{
	public Issue7580()
	{
		var invokeCountLabel = new Label
		{
			Text = "InvokeCount: 0",
			AutomationId = "InvokeCountLabel",
			FontSize = 20
		};

		var statusLabel = new Label
		{
			Text = "Status: Ready",
			AutomationId = "StatusLabel",
			FontSize = 16
		};

		int invokeCount = 0;
		bool isCompleted = true;

		var setNotCompleted = new SwipeItem
		{
			Text = "Set not completed",
			BackgroundColor = Colors.Red,
			IsVisible = isCompleted,
		};

		var setCompleted = new SwipeItem
		{
			Text = "Set completed",
			BackgroundColor = Colors.Green,
			IsVisible = !isCompleted,
		};

		var toggleCommand = new Command(() =>
		{
			invokeCount++;
			isCompleted = !isCompleted;

			setNotCompleted.IsVisible = isCompleted;
			setCompleted.IsVisible = !isCompleted;

			invokeCountLabel.Text = $"InvokeCount: {invokeCount}";
			statusLabel.Text = $"Status: IsCompleted={isCompleted}";

			System.Diagnostics.Debug.WriteLine($"[Issue7580] Toggle invoked #{invokeCount}, IsCompleted={isCompleted}");
		});

		setNotCompleted.Command = toggleCommand;
		setCompleted.Command = toggleCommand;

		var swipeItems = new SwipeItems
		{
			Mode = SwipeMode.Execute
		};
		swipeItems.Add(setNotCompleted);
		swipeItems.Add(setCompleted);

		var swipeContent = new Grid
		{
			HeightRequest = 60,
			BackgroundColor = Colors.LightGray,
		};
		swipeContent.Add(new Label
		{
			Text = "Swipe right to toggle",
			AutomationId = "SwipeTarget",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		});

		var swipeView = new SwipeView
		{
			LeftItems = swipeItems,
			Content = swipeContent,
			Threshold = 250
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "Issue 7580 - SwipeItem visibility toggle bug",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				},
				new Label
				{
					Text = "Swipe right on the gray area. InvokeCount should be 1 after each swipe.",
					FontSize = 14
				},
				invokeCountLabel,
				statusLabel,
				swipeView
			}
		};
	}
}
