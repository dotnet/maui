namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34210, "SwipeItem ignores FontImageSource rendered size on Android", PlatformAffected.Android)]
public class Issue34210 : ContentPage
{
	public Issue34210()
	{
		var swipeItem = new SwipeItem
		{
			Text = "Action",
			BackgroundColor = Colors.Blue,
			IconImageSource = new FontImageSource
			{
				FontFamily = "FA",
				Glyph = "\uf0f3",
				Size = 20,
			}
		};

		var swipeView = new SwipeView
		{
			HeightRequest = 100,
			LeftItems = new SwipeItems { swipeItem },
			Content = new Grid
			{
				HeightRequest = 100,
				BackgroundColor = Colors.LightGray,
				Children =
				{
					new Label
					{
						Text = "Swipe right to reveal",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						AutomationId = "SwipeContent"
					}
				}
			}
		};

		var openButton = new Button
		{
			Text = "Open SwipeView",
			AutomationId = "OpenSwipeViewButton"
		};

		openButton.Clicked += (sender, e) =>
		{
			swipeView.Open(OpenSwipeItem.LeftItems);
		};

		Content = new VerticalStackLayout
		{
			Padding = 16,
			Spacing = 8,
			Children = { swipeView, openButton }
		};
	}
}
