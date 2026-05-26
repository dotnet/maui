namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26593, "SwipeView UI Test", PlatformAffected.iOS)]
public partial class SwipeViewUITest : ContentPage
{
	public SwipeViewUITest()
	{
		this.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

		SwipeItem favoriteSwipeItem = new SwipeItem
		{
			Text = "Coffee",
			IconImageSource = "coffee.png",
			BackgroundColor = Colors.Brown
		};

		// SwipeView content
		Grid grid = new Grid
		{
			HeightRequest = 60,
			BackgroundColor = Colors.LightGray
		};
		grid.Add(new Label
		{
			Text = "Swipe right",
			AutomationId = "SwipeRight",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		});

		SwipeView swipeView = new SwipeView
		{
			RightItems = new SwipeItems { favoriteSwipeItem },
			Content = grid
		};

		Content = swipeView;

	}
}