namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23074, "SwipeItem IconImageSource should allow more configuration", PlatformAffected.All)]
public class Issue23074 : ContentPage
{
	public Issue23074()
	{
		// FontImageSource SwipeItem: Color set directly on FontImageSource should be used for tinting
		var fontSwipeItem = new SwipeItem
		{
			Text = "Font",
			AutomationId = "FontSwipeItem",
			BackgroundColor = Colors.CornflowerBlue,
			IconImageSource = new FontImageSource
			{
				Glyph = "\u2605", // \u2605 star
				FontFamily = "OpenSansRegular",
				Size = 30,
				Color = Colors.Yellow
			}
		};

		// FontImageSource SwipeItem without explicit Color: should fall back to text color for tinting
		var noColorFontSwipeItem = new SwipeItem
		{
			Text = "FontNoColor",
			AutomationId = "FontNoColorSwipeItem",
			BackgroundColor = Colors.LightYellow,
			IconImageSource = new FontImageSource
			{
				Glyph = "\u2605",
				FontFamily = "OpenSansRegular",
				Size = 30
			}
		};

		// SVG SwipeItem: SVG image should preserve its original colors (red cancel icon, not tinted blue)
		var svgSwipeItem = new SwipeItem
		{
			Text = "Cancel",
			AutomationId = "SvgSwipeItem",
			BackgroundColor = Colors.LightGray,
			IconImageSource = "cancel_red.png"
		};

		var swipeContent = new Grid
		{
			HeightRequest = 80,
			BackgroundColor = Colors.LightGray,
			AutomationId = "SwipeContent"
		};
		swipeContent.Add(new Label
		{
			Text = "Swipe left to see icons",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		});

		var swipeView = new SwipeView
		{
			AutomationId = "SwipeViewWithIcons",
			RightItems = new SwipeItems { fontSwipeItem, noColorFontSwipeItem, svgSwipeItem },
			Content = swipeContent
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(16),
			Children = { swipeView }
		};
	}
}
