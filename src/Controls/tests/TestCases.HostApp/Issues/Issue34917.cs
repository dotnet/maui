namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34917, "SwipeView.Open crashes with ArgumentException on second call", PlatformAffected.iOS)]
public class Issue34917 : TestContentPage
{
	const string OpenButtonId = "OpenButton";
	const string CloseButtonId = "CloseButton";
	const string StatusLabelId = "StatusLabel";

	protected override void Init()
	{
		Title = "Issue 34917";

		var statusLabel = new Label
		{
			AutomationId = StatusLabelId,
			Text = "Ready"
		};

		var swipeItem = new SwipeItem
		{
			BackgroundColor = Colors.Red,
			Text = "Action"
		};

		var swipeView = new SwipeView
		{
			HeightRequest = 60,
			WidthRequest = 300,
			LeftItems = new SwipeItems { swipeItem },
			Content = new Grid
			{
				BackgroundColor = Colors.Gray,
				Children =
				{
					new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						Text = "Swipe content"
					}
				}
			}
		};

		var openButton = new Button
		{
			AutomationId = OpenButtonId,
			Text = "Open SwipeView"
		};

		var closeButton = new Button
		{
			AutomationId = CloseButtonId,
			Text = "Close SwipeView"
		};

		int openCount = 0;

		openButton.Clicked += (s, e) =>
		{
			try
			{
				openCount++;
				swipeView.Open(OpenSwipeItem.LeftItems);
				statusLabel.Text = $"Opened {openCount}";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Error: {ex.GetType().Name}";
			}
		};

		closeButton.Clicked += (s, e) =>
		{
			swipeView.Close();
			statusLabel.Text = "Closed";
		};

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 20,
			Children = { openButton, closeButton, swipeView, statusLabel }
		};
	}
}
