namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39484, "SwipeItem background should reset when cleared", PlatformAffected.Android)]
public class Issue39484 : ContentPage
{
	public Issue39484()
	{
		BackgroundColor = Colors.White;

		var swipeItem = new SwipeItem
		{
			Text = "Action",
			AutomationId = "Action",
			BackgroundColor = Colors.Orange
		};

		var swipeItems = new SwipeItems
		{
			Mode = SwipeMode.Reveal,
			SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen
		};
		swipeItems.Add(swipeItem);

		var swipeView = new SwipeView
		{
			AutomationId = "BackgroundSwipeView",
			HeightRequest = 100,
			BackgroundColor = Colors.LightBlue,
			RightItems = swipeItems,
			Content = new Label
			{
				Text = "Swipe left",
				AutomationId = "SwipeContent",
				BackgroundColor = Colors.White,
				TextColor = Colors.Black,
				Padding = 20,
				VerticalTextAlignment = TextAlignment.Center
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "Tap Open, Clear background, then Restore background. The action should stay open and orange should disappear when cleared.",
					TextColor = Colors.Black
				},
				swipeView,
				new Button
				{
					Text = "Open",
					AutomationId = "OpenSwipeViewButton",
					Command = new Command(() => swipeView.Open(OpenSwipeItem.RightItems, false))
				},
				new Button
				{
					Text = "Clear background",
					AutomationId = "ClearBackgroundButton",
					Command = new Command(() => swipeItem.BackgroundColor = null)
				},
				new Button
				{
					Text = "Restore background",
					AutomationId = "RestoreBackgroundButton",
					Command = new Command(() => swipeItem.BackgroundColor = Colors.Orange)
				}
			}
		};
	}
}
