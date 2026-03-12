using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34210, "SwipeItem ignores FontImageSource rendered size on Android", PlatformAffected.Android)]
public class Issue34210 : ContentPage
{
	public Issue34210()
	{
		// FontImageSource.Size=20 — Android bug makes this render at ~50px (containerHeight/2)
		var swipeItem = new SwipeItem
		{
			Text = "Action",
			BackgroundColor = Colors.SteelBlue,
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ion",
				Glyph = "\uf30c", // star/bookmark glyph in Ionicons
				Size = 20,
				Color = Colors.White
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
						Text = "← Swipe right",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						AutomationId = "SwipeContent"
					}
				}
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = 16,
			Spacing = 8,
			Children = { swipeView }
		};
	}
}
