namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6016, "SwipeView Threshold changes width and offset of the side menu", PlatformAffected.Android)]
public class Issue6016 : ContentPage
{
	public Issue6016()
	{
		var defaultSwipeItem = new SwipeItem
		{
			Text = "Action",
			BackgroundColor = Colors.LightBlue,
			AutomationId = "DefaultSwipeItem"
		};

		var defaultSwipeView = new SwipeView
		{
			LeftItems = new SwipeItems { defaultSwipeItem },
			AutomationId = "DefaultSwipeView",
			Content = new Grid
			{
				HeightRequest = 60,
				BackgroundColor = Colors.LightGray,
				Children =
				{
					new Label
					{
						Text = "No Threshold",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						AutomationId = "DefaultContent"
					}
				}
			}
		};

		var thresholdSwipeItem = new SwipeItem
		{
			Text = "Action",
			BackgroundColor = Colors.LightBlue,
			AutomationId = "ThresholdSwipeItem"
		};

		// Threshold = 200 should NOT affect menu width (it should stay at default 100)
		var thresholdSwipeView = new SwipeView
		{
			LeftItems = new SwipeItems { thresholdSwipeItem },
			Threshold = 200,
			AutomationId = "ThresholdSwipeView",
			Content = new Grid
			{
				HeightRequest = 60,
				BackgroundColor = Colors.LightGray,
				Children =
				{
					new Label
					{
						Text = "Threshold 200",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						AutomationId = "ThresholdContent"
					}
				}
			}
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children =
			{
				new Label { Text = "SwipeView Threshold Test", FontSize = 16, FontAttributes = FontAttributes.Bold },
				new Label { Text = "Both rows should show same-width menu when opened" },
				defaultSwipeView,
				thresholdSwipeView
			}
		};
	}
}
