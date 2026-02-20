namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31917, "SwipeItemView and SwipeItem background doesn't update on AppTheme change (Light/Dark)", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.macOS)]
public class Issue31917 : ContentPage
{
	readonly SwipeItem _swipeItem;
	readonly SwipeItemView _swipeItemView;
	readonly Label _swipeItemColorLabel;
	readonly Label _swipeItemViewColorLabel;

	public Issue31917()
	{
		_swipeItem = new SwipeItem
		{
			Text = "Action",
			AutomationId = "TestSwipeItem"
		};
		_swipeItem.SetAppThemeColor(SwipeItem.BackgroundColorProperty, Colors.Yellow, Colors.Purple);

		_swipeItemView = new SwipeItemView
		{
			AutomationId = "TestSwipeItemView",
			Content = new Label
			{
				Text = "Custom",
				TextColor = Colors.White,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		_swipeItemView.SetAppThemeColor(SwipeItemView.BackgroundColorProperty, Colors.Blue, Colors.Red);

		var swipeView = new SwipeView
		{
			LeftItems = new SwipeItems { _swipeItem },
			RightItems = new SwipeItems { _swipeItemView },
			Content = new Label
			{
				Text = "Swipe left or right to reveal items",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			},
			HeightRequest = 60
		};

		_swipeItemColorLabel = new Label
		{
			AutomationId = "SwipeItemColorLabel",
			Text = "Not checked"
		};

		_swipeItemViewColorLabel = new Label
		{
			AutomationId = "SwipeItemViewColorLabel",
			Text = "Not checked"
		};

		var changeThemeButton = new Button
		{
			Text = "Change Theme",
			AutomationId = "changeThemeButton",
			Command = new Command(() =>
			{
				Application.Current!.UserAppTheme = Application.Current!.UserAppTheme != AppTheme.Dark
					? AppTheme.Dark
					: AppTheme.Light;
				UpdateColorLabels();
			})
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				swipeView,
				changeThemeButton,
				_swipeItemColorLabel,
				_swipeItemViewColorLabel
			}
		};
	}

	void UpdateColorLabels()
	{
		var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

		var expectedSwipeItemColor = isDark ? Colors.Purple : Colors.Yellow;
		var actualSwipeItemColor = _swipeItem.BackgroundColor;
		_swipeItemColorLabel.Text = actualSwipeItemColor == expectedSwipeItemColor ? "PASS" : "FAIL";

		var expectedSwipeItemViewColor = isDark ? Colors.Red : Colors.Blue;
		var actualSwipeItemViewColor = _swipeItemView.BackgroundColor;
		_swipeItemViewColorLabel.Text = actualSwipeItemViewColor == expectedSwipeItemViewColor ? "PASS" : "FAIL";
	}
}
