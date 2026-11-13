namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34917, "[net 11.0][iOS,MacCatalyst] SwipeView.Open() throws ArgumentException on second programmatic call", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34917 : ContentPage
{
	const string OpenRightButtonId = "OpenSwipeButton";
	const string OpenBottomButtonId = "OpenBottomSwipeButton";
	const string CloseButtonId = "CloseSwipeButton";
	const string StatusLabelId = "StatusLabel";

	readonly SwipeView _swipeView;

	public Issue34917()
	{
		var openRightButton = new Button
		{
			Text = "Open RightItems",
			AutomationId = OpenRightButtonId
		};

		var openBottomButton = new Button
		{
			Text = "Open BottomItems",
			AutomationId = OpenBottomButtonId
		};

		var closeButton = new Button
		{
			Text = "Close SwipeView",
			AutomationId = CloseButtonId
		};

		var statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = StatusLabelId,
			HorizontalOptions = LayoutOptions.Center
		};

		var rightSwipeItem = new SwipeItem
		{
			Text = "Right Action",
			BackgroundColor = Colors.LightBlue
		};

		var bottomSwipeItem = new SwipeItem
		{
			Text = "Bottom Action",
			BackgroundColor = Colors.LightGreen
		};

		_swipeView = new SwipeView
		{
			HeightRequest = 60,
			RightItems = new SwipeItems { rightSwipeItem },
			BottomItems = new SwipeItems { bottomSwipeItem },
			Content = new Grid
			{
				BackgroundColor = Colors.LightGray,
				Children =
				{
					new Label
					{
						Text = "Swipe content",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			}
		};

		openRightButton.Clicked += (s, e) =>
		{
			try
			{
				_swipeView.Open(OpenSwipeItem.RightItems);
				statusLabel.Text = "Success";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Error: {ex.GetType().Name}";
			}
		};

		openBottomButton.Clicked += (s, e) =>
		{
			try
			{
				_swipeView.Open(OpenSwipeItem.BottomItems);
				statusLabel.Text = "Success";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Error: {ex.GetType().Name}";
			}
		};

		closeButton.Clicked += (s, e) =>
		{
			_swipeView.Close();
		};

		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Padding = new Thickness(20),
			Children =
			{
				openRightButton,
				openBottomButton,
				closeButton,
				_swipeView,
				statusLabel
			}
		};
	}
}
