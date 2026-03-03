namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19331, "SwipeItems referencing causes crash on Android", PlatformAffected.Android)]
public class Issue19331 : ContentPage
{
	public const string StatusLabelId = "StatusLabel19331";
	public const string SwipeRow1Id = "SwipeRow1_19331";
	public const string SwipeRow2Id = "SwipeRow2_19331";
	public const string SwipeRow3Id = "SwipeRow3_19331";

	public Issue19331()
	{
		var sharedSwipeItems = new SwipeItems
		{
			new SwipeItem
			{
				Text = "Delete",
				BackgroundColor = Colors.Red
			}
		};

		var statusLabel = new Label
		{
			AutomationId = StatusLabelId,
			Text = "Swipe rows to test",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10)
		};

		var row1Content = new Grid
		{
			BackgroundColor = Colors.LightGray,
			HeightRequest = 60,
			Padding = new Thickness(12, 0)
		};
		row1Content.Add(new Label
		{
			AutomationId = SwipeRow1Id,
			Text = "Row 1 - Swipe left",
			VerticalOptions = LayoutOptions.Center
		});

		var row2Content = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 60,
			Padding = new Thickness(12, 0)
		};
		row2Content.Add(new Label
		{
			AutomationId = SwipeRow2Id,
			Text = "Row 2 - Swipe left",
			VerticalOptions = LayoutOptions.Center
		});

		var row3Content = new Grid
		{
			BackgroundColor = Colors.LightGreen,
			HeightRequest = 60,
			Padding = new Thickness(12, 0)
		};
		row3Content.Add(new Label
		{
			AutomationId = SwipeRow3Id,
			Text = "Row 3 - Swipe left",
			VerticalOptions = LayoutOptions.Center
		});

		Content = new VerticalStackLayout
		{
			Children =
			{
				statusLabel,
				new SwipeView { LeftItems = sharedSwipeItems, Content = row1Content },
				new SwipeView { LeftItems = sharedSwipeItems, Content = row2Content },
				new SwipeView { LeftItems = sharedSwipeItems, Content = row3Content },
			}
		};
	}
}
