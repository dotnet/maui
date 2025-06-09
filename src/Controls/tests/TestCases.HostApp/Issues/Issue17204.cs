namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17204, "SwipeView does not work correctly on iOS when opened programmatically", PlatformAffected.iOS)]
public class Issue17204 : ContentPage
{
	public Issue17204()
	{
		var verticalStack = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20)
		};

		var swipeView = new SwipeView
		{
			Background = Colors.LightPink,
		};

		var label = new Label
		{
			HeightRequest = 200,
			VerticalTextAlignment = TextAlignment.Center,
			Text = "Programmatic swipe works"
		};

		var swipeItemView = new SwipeItemView
		{
			Background = Colors.LightGreen,
			Content = label,
		};

		var swipeItems = new SwipeItems();
		swipeItems.Add(swipeItemView);
		swipeView.RightItems = swipeItems;

		swipeView.Content = new Label
		{
			HeightRequest = 200,
			VerticalTextAlignment = TextAlignment.Center,
			Text = "Swipe view"
		};

		var button = new Button
		{
			Text = "Open SwipeView",
			AutomationId = "OpenSwipeViewButton"
		};

		button.Clicked += (sender, e) =>
		{
			swipeView.Open(OpenSwipeItem.RightItems, false);
		};

		verticalStack.Add(swipeView);
		verticalStack.Add(button);
		Content = verticalStack;
	}
}